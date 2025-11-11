using System;
using System.Collections.Generic;
using System.Linq;
using ReceiptPrinterEmulator.Emulator;
using ReceiptPrinterEmulator.EscPos.Commands.ESC;
using ReceiptPrinterEmulator.EscPos.Commands.FS;
using ReceiptPrinterEmulator.EscPos.Commands.GS;
using ReceiptPrinterEmulator.Logging;

namespace ReceiptPrinterEmulator.EscPos;

public class EscPosInterpreter
{
    private readonly ReceiptPrinter _printer;
    private readonly List<byte> _printBuffer;
    private readonly List<byte> _commandBuffer;
    private readonly List<BaseCommand> _commandRegistry;

    private int _maxCommandPrefixLength;

    private bool _interpretingCommandPrefix;
    private bool _interpretingCommandArgs;
    private BaseCommand? _activeCommand;

    public EscPosInterpreter(ReceiptPrinter printer)
    {
        _printer = printer;
        _printBuffer = [];
        _commandBuffer = [];
        _commandRegistry = [];

        _maxCommandPrefixLength = 0;

        _interpretingCommandPrefix = false;
        _interpretingCommandArgs = false;
        _activeCommand = null;

        RegisterCommands();
    }

    #region Command registry

    private void RegisterCommands()
    {
        // ESC = 0x1B
        RegisterCommand(new InitializePrinterCommand());
        RegisterCommand(new ItalicOffCommand());
        RegisterCommand(new ItalicOnCommand());
        RegisterCommand(new SelectFontCommand());
        RegisterCommand(new SelectCharsetCommand());
        RegisterCommand(new SelectCharTableCommand());
        RegisterCommand(new SelectJustificationCommand());
        RegisterCommand(new SetDefaultLineSpacingCommand());
        RegisterCommand(new SetLineSpacingCommand());
        RegisterCommand(new ToggleEmphasizeCommand());
        RegisterCommand(new ToggleUnderlineCommand());
        RegisterCommand(new ToggleUpsideDownCommand());
        RegisterCommand(new SetPrintTextMode()); // 0x1B, 0x21, n
        RegisterCommand(new PaperFullCut()); // 0x1B, 0x6D
        RegisterCommand(new PaperPartialCut()); // 0x1B, 0x69
        RegisterCommand(new PaperPrintFeednLines()); // 0x1B, 0x64
        RegisterCommand(new PaperPrintFeed()); // 0x1B, 0x4A
        RegisterCommand(new GeneratePulseCommand()); // 0x1B, 0x70

        // FS = 0x1C
        RegisterCommand(new PrintStoredLogo()); // 0x1C, 0x70, n, m
        RegisterCommand(new PaperAutoCut()); // 0x1C, 0x7D, 0x60, n

        // GS = 0x1D
        RegisterCommand(new SelectCharacterSizeCommand());
        RegisterCommand(new SelectCutModeAndCutCommand());
        RegisterCommand(new GraphicsDataCommand()); // 0x1D, 0x65, n, [m, t]
        RegisterCommand(new PaperEjectCommand()); // 0x1D, 0x65, n, [m, t]
        RegisterCommand(new PrintBarcodeCommand());
        RegisterCommand(new PrintRasterBitImageCommand());
        RegisterCommand(new SetBarcodeHeightCommand());
        RegisterCommand(new SetBarcodeWidthMultiplierCommand());
        RegisterCommand(new SetBarcodeHriPrintPositionCommand());
        RegisterCommand(new SetBarcodeHriFontCommand());
        RegisterCommand(new Barcode2dDataCmd());
        RegisterCommand(new ToggleInvertedCommand());
    }

    private void RegisterCommand(BaseCommand command)
    {
        byte[] prefix = [.. command.Prefix];

        if (_commandRegistry.Any(c => c.Prefix.SequenceEqual(prefix)))
            throw new ArgumentException($"Cannot register command with duplicate prefix: {prefix}");

        Logger.Info(
            $"Register command [{command.GetType().Name}] with prefix [{string.Join(" ", prefix.Select(b => b.ToString("X2")))}]"
        );
        _commandRegistry.Add(command);

        if (prefix.Length > _maxCommandPrefixLength)
            _maxCommandPrefixLength = prefix.Length;
    }

    #endregion

    #region Buffers

    public void ClearBuffers()
    {
        FinalizePrintBuffer();
        FinalizeCommandBuffer();
    }

    private IReadOnlyList<byte> FinalizePrintBuffer()
    {
        List<byte> result = [.. _printBuffer];
        _printBuffer.Clear();
        return result;
    }

    private IReadOnlyList<byte> FinalizeCommandBuffer()
    {
        List<byte> result = [.. _commandBuffer];
        _commandBuffer.Clear();
        return result;
    }

    #endregion

    public void Interpret(ReadOnlySpan<byte> input)
    {
        for (var i = 0; i < input.Length; i++)
        {
            var currentChar = input[i];

            #region Command modes

            if (_interpretingCommandArgs)
            {
                // Reading command args: keep reading until the command is done interpreting
                _commandBuffer.Add(currentChar);

                var shouldContinue = _activeCommand!.InterpretNextChar(currentChar);

                if (!shouldContinue)
                {
                    var finalArgs = FinalizeCommandBuffer();

                    Logger.Info(
                        $"Execute [{_activeCommand.GetType().Name}] with args ["
                            + string.Join(" ", finalArgs.Take(4).Select(b => $"0x{b:X2}"))
                            + " "
                            + (finalArgs.Count > 4 ? "..." : "")
                            + "]"
                    );

                    _activeCommand.Execute(_printer);
                    _activeCommand = null;

                    _interpretingCommandPrefix = false;
                    _interpretingCommandArgs = false;
                }

                continue;
            }

            if (_interpretingCommandPrefix)
            {
                // Reading command prefix: keep reading until we find a match or hit _maxCommandPrefixLength
                _commandBuffer.Add(currentChar);

                byte[] commandText = [.. _commandBuffer];
                if (commandText.Length > _maxCommandPrefixLength)
                {
                    throw new InvalidOperationException(
                        "Invalid or unsupported command encountered: "
                            + string.Join(" ", commandText.Select(b => $"0x{b:X2}"))
                    );
                }

                int commandIdx = _commandRegistry.FindIndex(c =>
                    c.Prefix.SequenceEqual(commandText)
                );
                if (commandIdx >= 0)
                {
                    // Found matching registered command
                    _activeCommand = _commandRegistry[commandIdx];
                    _activeCommand.Reset();

                    _commandBuffer.Clear();

                    if (_activeCommand.HasArgs)
                    {
                        // This command has arguments: begin interpreting those
                        _interpretingCommandPrefix = false;
                        _interpretingCommandArgs = true;
                    }
                    else
                    {
                        // This command has NO arguments: execute immediately and return to normal mode
                        _interpretingCommandPrefix = false;
                        _interpretingCommandArgs = false;

                        Logger.Info($"Execute [{_activeCommand.GetType().Name}]");

                        _activeCommand.Execute(_printer);
                        _activeCommand = null;
                    }
                }

                continue;
            }

            #endregion

            #region Normal mode

            if (currentChar == HT)
            {
                // Horizontal tab
                var b = FinalizePrintBuffer();
                if (b.Count > 0)
                    _printer.PrintText(b);
                _printer.PrintTab(b.Count);
                continue;
            }

            if (currentChar == LF || currentChar == CR)
            {
                // Print and line feed
                _printer.PrintAndLineFeed(FinalizePrintBuffer());
                continue;
            }

            if (currentChar == FF)
            {
                // Print and return to Standard mode (in Page mode)
                //throw new NotImplementedException("Not supported: page mode");
                continue;
            }

            if (currentChar == DLE)
            {
                // Prefix for real-time commands (pulse, power-off, buzzer, status, etc)
                throw new NotImplementedException("Not supported: DLE / real time commands");
            }

            if (currentChar == CAN)
            {
                // Cancel print data in Page mode
                throw new NotImplementedException("Not supported: page mode");
            }

            if (currentChar == ESC || currentChar == FS || currentChar == GS)
            {
                // ESC, FS and GS commands - begin command mode
                _printer.PrintText(FinalizePrintBuffer());
                _interpretingCommandPrefix = true;

                _commandBuffer.Clear();
                _commandBuffer.Add(currentChar);
                continue;
            }

            if (currentChar == NUL)
            {
                // Null byte outside of command context; do nothing
                continue;
            }

            // Regular character, not in command mode: append to print buffer
            _printBuffer.Add(currentChar);

            #endregion
        }
    }

    public const byte NUL = 0x00;
    public const byte HT = 0x09;
    public const byte LF = 0x0A;
    public const byte FF = 0x0C;
    public const byte CR = 0x0D;
    public const byte DLE = 0x10;
    public const byte CAN = 0x18;
    public const byte ESC = 0x1B;
    public const byte FS = 0x1C;
    public const byte GS = 0x1D;
}

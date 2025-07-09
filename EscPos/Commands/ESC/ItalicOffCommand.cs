using System;
using ReceiptPrinterEmulator.Emulator;

namespace ReceiptPrinterEmulator.EscPos.Commands.ESC;

/// <summary>
/// Turn italics mode off
/// https://github.com/song940/node-escpos/blob/84149a67306857ad98cdafcd8384fb2b942e15da/packages/printer/commands.js
/// </summary>
public class ItalicOffCommand : BaseCommandNoArgs
{
    public override ReadOnlySpan<byte> Prefix => [EscPosInterpreter.ESC, (byte)'5'];

    public override void Execute(ReceiptPrinter printer)
    {
        printer.SelectItalicMode(false);
    }
}

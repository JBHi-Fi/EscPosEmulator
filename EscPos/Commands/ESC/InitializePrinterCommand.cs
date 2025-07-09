using System;
using ReceiptPrinterEmulator.Emulator;

namespace ReceiptPrinterEmulator.EscPos.Commands.ESC;

public class InitializePrinterCommand : BaseCommandNoArgs
{
    public override ReadOnlySpan<byte> Prefix => [EscPosInterpreter.ESC, (byte)'@'];

    public override void Execute(ReceiptPrinter printer)
    {
        printer.Initialize();
    }
}

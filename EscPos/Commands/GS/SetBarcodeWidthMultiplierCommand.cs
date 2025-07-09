using System;
using ReceiptPrinterEmulator.Emulator;

namespace ReceiptPrinterEmulator.EscPos.Commands.GS;

/// <summary>
/// Set barcode width multiplier
/// </summary>
public class SetBarcodeWidthMultiplierCommand : BaseCommand
{
    public override ReadOnlySpan<byte> Prefix => [EscPosInterpreter.GS, (byte)'w'];
    public override bool HasArgs => true;

    private byte _n;

    public override void Reset()
    {
        _n = 0;
    }

    public override bool InterpretNextChar(byte c)
    {
        _n = c;
        return false;
    }

    public override void Execute(ReceiptPrinter printer)
    {
        printer.SetBarcodeWidthMultiplier(_n);
    }
}

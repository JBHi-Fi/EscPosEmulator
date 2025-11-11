using System;
using ReceiptPrinterEmulator.Emulator;
using ReceiptPrinterEmulator.Emulator.Enums;

namespace ReceiptPrinterEmulator.EscPos.Commands.GS;

/// <summary>
/// Set print position of HRI characters for barcodes
/// </summary>
public class SetBarcodeHriPrintPositionCommand : BaseCommand
{
    public override ReadOnlySpan<byte> Prefix => [EscPosInterpreter.GS, (byte)'H'];
    public override bool HasArgs => true;

    private int _n;

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
        printer.SetBarcodeHriPrintPosition(_n switch
        {
            0 or 48 => HriPrintPosition.NotPrinted,
            1 or 49 => HriPrintPosition.Above,
            2 or 50 => HriPrintPosition.Below,
            3 or 51 => HriPrintPosition.AboveAndBelow,
            _ => HriPrintPosition.NotPrinted,
        });
    }
}

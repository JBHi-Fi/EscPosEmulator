using System;
using ReceiptPrinterEmulator.Emulator;
using ReceiptPrinterEmulator.Emulator.Enums;

namespace ReceiptPrinterEmulator.EscPos.Commands.GS;

/// <summary>
/// Set barcode font for HRI characters
/// </summary>
public class SetBarcodeHriFontCommand : BaseCommand
{
    public override ReadOnlySpan<byte> Prefix => [EscPosInterpreter.GS, (byte)'f'];
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
        printer.SetBarcodeHriFont(_n switch
        {
            0 or 48 => PrinterFont.FontA,
            1 or 49 => PrinterFont.FontB,
            2 or 50 => PrinterFont.FontC,
            3 or 51 => PrinterFont.FontD,
            _ => PrinterFont.FontA,
        });
    }
}

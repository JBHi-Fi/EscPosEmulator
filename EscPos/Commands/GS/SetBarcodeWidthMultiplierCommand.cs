using ReceiptPrinterEmulator.Emulator;

namespace ReceiptPrinterEmulator.EscPos.Commands.GS;

/// <summary>
/// Set barcode width multiplier
/// </summary>
public class SetBarcodeWidthMultiplierCommand : BaseCommand
{
    public override string Prefix => EscPosInterpreter.GS + "w";
    public override bool HasArgs => true;

    private int _n;

    public override void Reset()
    {
        _n = 0;
    }

    public override bool InterpretNextChar(char c)
    {
        _n = c;
        return false;
    }

    public override void Execute(ReceiptPrinter printer, string? args)
    {
        printer.SetBarcodeWidthMultiplier(_n);
    }
}

using ReceiptPrinterEmulator.Emulator;

namespace ReceiptPrinterEmulator.EscPos.Commands.GS;

/// <summary>
/// Turn inverted mode on/off
/// </summary>
public class ToggleInvertedCommand : BaseCommand
{
    public override string Prefix => EscPosInterpreter.GS + "B";
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
        if (_n is 0 or 48)
            printer.SelectInvertedMode(false);
        else if (_n is 1 or 49)
            printer.SelectInvertedMode(true);
    }
}

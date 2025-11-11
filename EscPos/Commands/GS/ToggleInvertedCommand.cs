using System;
using ReceiptPrinterEmulator.Emulator;

namespace ReceiptPrinterEmulator.EscPos.Commands.GS;

/// <summary>
/// Turn inverted mode on/off
/// </summary>
public class ToggleInvertedCommand : BaseCommand
{
    public override ReadOnlySpan<byte> Prefix => [EscPosInterpreter.GS, (byte)'B'];
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
        if (_n is 0 or 48)
            printer.SelectInvertedMode(false);
        else if (_n is 1 or 49)
            printer.SelectInvertedMode(true);
    }
}

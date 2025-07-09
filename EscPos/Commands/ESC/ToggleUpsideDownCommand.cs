using System;
using ReceiptPrinterEmulator.Emulator;

namespace ReceiptPrinterEmulator.EscPos.Commands.ESC;

/// <summary>
/// Turn Upside down mode on/off
/// </summary>
public class ToggleUpsideDownCommand : BaseCommand
{
    public override ReadOnlySpan<byte> Prefix => [EscPosInterpreter.ESC, (byte)'{'];
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
            printer.SelectUpsideDownMode(false);
        else if (_n is 1 or 49)
            printer.SelectUpsideDownMode(true);
    }
}

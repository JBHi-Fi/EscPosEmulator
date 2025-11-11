using System;
using ReceiptPrinterEmulator.Emulator;

namespace ReceiptPrinterEmulator.EscPos.Commands.ESC;

/// <summary>
/// Generate pulse.
/// </summary>
public class GeneratePulseCommand : BaseCommand
{
    public override ReadOnlySpan<byte> Prefix => [EscPosInterpreter.ESC, (byte)'p'];
    public override bool HasArgs => true;

    private int i;
    private byte m;
    private byte t1;
    private byte t2;

    public override void Reset()
    {
        i = 0;
        m = 0;
        t1 = 0;
        t2 = 0;
    }

    public override bool InterpretNextChar(byte c)
    {
        switch (i++)
        {
            case 0:
                m = c;
                return true;
            case 1:
                t1 = c;
                return true;
            case 2:
                t2 = c;
                return false;
            default:
                return false;
        }
    }

    public override void Execute(ReceiptPrinter printer)
    {
    }
}

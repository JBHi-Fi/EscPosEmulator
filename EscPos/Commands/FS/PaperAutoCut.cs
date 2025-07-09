using System;
using ReceiptPrinterEmulator.Emulator;

namespace ReceiptPrinterEmulator.EscPos.Commands.FS;

/// <summary>
/// 2024.02.18 Leo
/// Paper Movement Commands
/// https://escpos.readthedocs.io/en/latest/paper_movement.html#enable-and-disable-auto-cut-1c-7d-60-phx
/// </summary>
public class PaperAutoCut : BaseCommand
{
    public override ReadOnlySpan<byte> Prefix => [EscPosInterpreter.ESC, (byte)'}'];
    public override bool HasArgs => true;

    private int _idx;
    private byte _n;
    private byte _m;

    public override void Reset()
    {
        _idx = 0;
        _n = _m = 0;
    }

    public override bool InterpretNextChar(byte c)
    {
        if (_idx == 0)
        {
            _idx++;
            _n = c;
            _m = 0;
            if (_n == 0x60)
                return true;
        }
        else if (_idx == 1)
        {
            _idx++;
            _m = c;
        }

        return false;
    }

    public override void Execute(ReceiptPrinter printer)
    {
        // Nothing to do
    }
}

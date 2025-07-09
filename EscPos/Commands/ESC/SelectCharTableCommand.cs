using System;
using ReceiptPrinterEmulator.Emulator;

namespace ReceiptPrinterEmulator.EscPos.Commands.ESC;

/// <summary>
/// Select character font
/// https://reference.epson-biz.com/modules/ref_escpos/index.php?content_id=27
/// </summary>
public class SelectCharTableCommand : BaseCommand
{
    public override ReadOnlySpan<byte> Prefix => [EscPosInterpreter.ESC, (byte)'t'];
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

    public override void Execute(ReceiptPrinter printer) { }
}

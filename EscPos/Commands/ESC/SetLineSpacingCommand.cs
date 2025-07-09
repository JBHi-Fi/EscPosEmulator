using System;
using ReceiptPrinterEmulator.Emulator;

namespace ReceiptPrinterEmulator.EscPos.Commands.ESC;

/// <summary>
/// Set line spacing
/// https://reference.epson-biz.com/modules/ref_escpos/index.php?content_id=20
/// </summary>
public class SetLineSpacingCommand : BaseCommand
{
    public override ReadOnlySpan<byte> Prefix => [EscPosInterpreter.ESC, (byte)'3'];
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
        printer.SetLineSpacing(_n);
    }
}

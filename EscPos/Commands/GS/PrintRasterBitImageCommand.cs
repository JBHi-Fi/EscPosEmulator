using System;
using ReceiptPrinterEmulator.Emulator;

namespace ReceiptPrinterEmulator.EscPos.Commands.GS;

public class PrintRasterBitImageCommand : BaseCommand
{
    public override ReadOnlySpan<byte> Prefix => [EscPosInterpreter.GS, (byte)'v', (byte)'0'];
    public override bool HasArgs => true;

    private int n = 0;
    private byte m = 0x00;
    private byte xL = 0x00;
    private byte xH = 0x00;
    private int width = 0;
    private byte yL = 0x00;
    private byte yH = 0x00;
    private int height = 0;
    private int length = 0;
    private byte[]? data = null;

    public override bool InterpretNextChar(byte c)
    {
        switch (n++)
        {
            case 0:
                m = c;
                return true;
            case 1:
                xL = c;
                return true;
            case 2:
                xH = c;
                width = (xH << 8) | xL;
                return true;
            case 3:
                yL = c;
                return true;
            case 4:
                yH = c;
                height = (yH << 8) | yL;
                length = width * height;
                width *= 8;
                data = new byte[length];
                return length > 0;
            default:
                data![n - 6] = c;
                return n - 5 < length;
        }
    }

    public override void Reset()
    {
        n = 0;
        m = 0x00;
        xL = 0x00;
        xH = 0x00;
        width = 0;
        yL = 0x00;
        yH = 0x00;
        height = 0;
        length = 0;
        data = null;
    }

    public override void Execute(ReceiptPrinter printer)
    {
        printer.PrintBitmap(Graphics.DecodeBitmap(data!, data!.Length, width, height));
    }
}

using System;
using ReceiptPrinterEmulator.Emulator;

namespace ReceiptPrinterEmulator.EscPos.Commands.GS;

/// <summary>
/// Graphics data command
/// </summary>
public class GraphicsDataCommand : DataCommand
{
    public override ReadOnlySpan<byte> Prefix => [EscPosInterpreter.GS, (byte)'(', (byte)'L'];

    public override void Execute(ReceiptPrinter printer)
    {
        if (fn == 2 || fn == 50)
        {
            var buffer = printer.GetPrintBuffer();
            if (buffer is not null)
                printer.PrintBitmap(buffer);
        }
        else if (fn == 112)
        {
            if (data?.Length >= 8)
            {
                bool multitone = data[0] == 52;
                byte scaleX = data[1];
                byte scaleY = data[2];
                byte color = data[3];
                int width = (data[5] << 8) | data[4];
                int height = (data[7] << 8) | data[6];
                int byteLength = (width + 7) / 8 * height;

                printer.SetPrintBuffer(
                    Graphics.DecodeBitmap(
                        data[8..],
                        byteLength,
                        width,
                        height,
                        scaleX,
                        scaleY,
                        color
                    )
                );
            }
        }
    }
}

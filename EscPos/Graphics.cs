using System;
using System.Drawing;
using System.Drawing.Imaging;
using ReceiptPrinterEmulator.Logging;

namespace ReceiptPrinterEmulator.EscPos;

public static class Graphics
{
    public static Bitmap DecodeBitmap(
        byte[] data,
        int dataLength,
        int width,
        int height,
        int scaleX = 1,
        int scaleY = 1,
        int color = 0
    )
    {
        var bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
        var values = ReadBytesByBits(data, dataLength, width, height);
        Logger.Info($"{data.Length} bytes read, {values.Length} bits decoded");

        BitmapData bitmapData = bmp.LockBits(
            new Rectangle(0, 0, width, height),
            ImageLockMode.WriteOnly,
            bmp.PixelFormat
        );
        IntPtr ptr = bitmapData.Scan0;
        for (int i = 0; i < values.Length; i++)
        {
            // TODO: Map color to actual RGB values
            byte value = values[i] == 0 ? (byte)255 : (byte)0;
            System.Runtime.InteropServices.Marshal.WriteByte(ptr, i * 3 + 0, value);
            System.Runtime.InteropServices.Marshal.WriteByte(ptr, i * 3 + 1, value);
            System.Runtime.InteropServices.Marshal.WriteByte(ptr, i * 3 + 2, value);
        }

        bmp.UnlockBits(bitmapData);

        if (scaleX != 1 || scaleY != 1)
        {
            // TODO: Implement scaling
        }

        return bmp;
    }

    private static byte[] ReadBytesByBits(byte[] data, int dataLength, int width, int height)
    {
        byte[] result = new byte[width * height];
        int padding = width % 8;
        if (padding == 0)
            padding = 8;

        int col = 0;
        int x = 0;
        byte b;
        for (int i = 0; i < dataLength; i++)
        {
            b = data[i];
            col += 8;
            if (col >= width)
            {
                for (int n = 0; n < padding; n++)
                {
                    result[x++] = (byte)((b >> (7 - n)) & 1);
                }
                col = 0;
            }
            else
            {
                for (int j = 0; j < 8; j++)
                {
                    result[x++] = (byte)((b >> (7 - j)) & 1);
                }
            }
        }

        return result;
    }
}

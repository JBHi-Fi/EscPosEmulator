using System;
using System.Drawing;
using ReceiptPrinterEmulator.Emulator.Abstraction;
using ReceiptPrinterEmulator.Emulator.Enums;
using ReceiptPrinterEmulator.Logging;
using ZXing.Windows.Compatibility;

namespace ReceiptPrinterEmulator.Emulator.Printables;

public class ReceiptBarcodeLine(
    PaperConfiguration paperConfiguration,
    BarcodeType type,
    BarcodeConfiguration barcodeConfiguration,
    string barcode
)
    : IReceiptPrintable
{
    public int GetPrintHeight() => barcodeConfiguration.Height;

    public void Render(Bitmap bitmap, Graphics g, int offsetX, int offsetY)
    {
        Logger.Info(
            $"Rendering {type} barcode [{barcode}] at offset ({offsetX}, {offsetY})"
        );

        var printWidth = paperConfiguration.GetPrintWidthInPixels();

        var barcodeWriter = new BarcodeWriter
        {
            Format = type switch
            {
                BarcodeType.UPC_A => ZXing.BarcodeFormat.UPC_A,
                BarcodeType.UPC_E => ZXing.BarcodeFormat.UPC_E,
                BarcodeType.JAN13 => ZXing.BarcodeFormat.EAN_13,
                BarcodeType.JAN8 => ZXing.BarcodeFormat.EAN_8,
                BarcodeType.CODE39 => ZXing.BarcodeFormat.CODE_39,
                BarcodeType.ITF => ZXing.BarcodeFormat.ITF,
                BarcodeType.CODABAR => ZXing.BarcodeFormat.CODABAR,
                BarcodeType.CODE93 => ZXing.BarcodeFormat.CODE_93,
                BarcodeType.CODE128 or BarcodeType.CODE128_AUTO => ZXing.BarcodeFormat.CODE_128,
                BarcodeType.GS1_128 => ZXing.BarcodeFormat.CODE_128, // GS1-128 is a subset of Code 128
                _ => throw new NotSupportedException($"Unsupported barcode type: {type}"),
            },
            Options = new ZXing.Common.EncodingOptions
            {
                Width = printWidth,
                Height = barcodeConfiguration.Height,
                PureBarcode = true,
                GS1Format = type == BarcodeType.GS1_128,
                NoPadding = false,
                Margin = 0,
            },
        };

        try
        {
            var image = barcodeWriter.Write(barcode);

            g.DrawImageUnscaled(image, offsetX, offsetY, image.Width, image.Height);
        }
        catch (Exception ex)
        {
            Logger.Exception(ex, $"Failed to render {type} barcode [{barcode}]");
        }
    }
}

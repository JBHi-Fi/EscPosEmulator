using System.Drawing;
using ReceiptPrinterEmulator.Emulator.Abstraction;
using ReceiptPrinterEmulator.Logging;

namespace ReceiptPrinterEmulator.Emulator.Printables;

public class ReceiptBitmapLine(PaperConfiguration paperConfiguration, Bitmap image)
    : IReceiptPrintable
{
    public int GetPrintHeight()
    {
        return image.Height;
    }

    public void Render(Bitmap bitmap, Graphics g, int offsetX, int offsetY)
    {
        Logger.Info(
            $"Rendering bitmap line at offset ({offsetX}, {offsetY}) with size ({image.Width}, {image.Height})"
        );

        var printWidth = paperConfiguration.GetPrintWidthInPixels();
        if (image.Width <= printWidth)
        {
            // Center the image horizontally if it fits within the print width
            offsetX += (printWidth - image.Width) / 2;
        }

        g.DrawImageUnscaled(image, offsetX, offsetY, image.Width, image.Height);
    }
}

using System;
using System.Collections.Generic;
using ReceiptPrinterEmulator.Emulator.Enums;

namespace ReceiptPrinterEmulator.Emulator;

public class PaperConfiguration(
    double DotsPerInch = 180,
    double PaperWidthMm = 80,
    double PrintWidthMm = 72
)
{
    private const double MillimetersPerInch = 25.4;
    public int DefaultLineSpacing => 10;
    public int DefaultTabSpacing => 8;

    public static PaperConfiguration Default => new();

    public Dictionary<PrinterFont, FontConfiguration> _printerFonts = new()
    {
        { PrinterFont.FontA, new FontConfiguration(PrinterFont.FontA, 12, 24, "MS Gothic") },
        { PrinterFont.FontB, new FontConfiguration(PrinterFont.FontB, 9, 17, "MS Gothic") },
        { PrinterFont.FontC, new FontConfiguration(PrinterFont.FontC, 24, 48, "MS Gothic") },
        { PrinterFont.FontD, new FontConfiguration(PrinterFont.FontD, 16, 24, "MS Gothic") },
    };

    public FontConfiguration GetFont(PrinterFont printerFont)
    {
        if (_printerFonts.ContainsKey(printerFont))
            return _printerFonts[printerFont];

        if (printerFont != PrinterFont.FontA)
            return GetFont(PrinterFont.FontA);

        throw new InvalidOperationException(
            $"Required font is missing from paper config: {printerFont}"
        );
    }

    public int GetPaperWidthInPixels() =>
        (int)Math.Ceiling(PaperWidthMm * DotsPerInch / MillimetersPerInch);

    public int GetPrintWidthInPixels() =>
        (int)Math.Ceiling(PrintWidthMm * DotsPerInch / MillimetersPerInch);

    public record FontConfiguration(
        PrinterFont PrinterFont,
        int CharacterWidth,
        int CharacterHeight,
        string RenderFont
    );
}

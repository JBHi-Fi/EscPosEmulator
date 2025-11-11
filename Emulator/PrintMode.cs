using ReceiptPrinterEmulator.Emulator.Enums;

namespace ReceiptPrinterEmulator.Emulator;

public record PrintMode(
    PrinterFont Font = PrinterFont.FontA,
    int CharWidthScale = 1,
    int CharHeightScale = 1,
    TextJustification Justification = TextJustification.Left,
    bool Emphasize = false,
    bool Italic = false,
    UnderlineMode Underline = UnderlineMode.Off,
    bool UpsideDown = false,
    bool Inverted = false
);

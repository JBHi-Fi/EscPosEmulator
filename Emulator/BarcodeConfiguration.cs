using ReceiptPrinterEmulator.Emulator.Enums;

namespace ReceiptPrinterEmulator.Emulator;

public sealed record BarcodeConfiguration(
    // 2 -> 6
    // Default is 3
    int Width = 3,
    // 1 -> 255
    // Default is 162
    int Height = 162,
    // 0 -> 3
    // Default is (0) NotPrinted
    HriPrintPosition HriPrintPosition = HriPrintPosition.NotPrinted,
    // 0 -> 3
    // Default is (0) FontA
    PrinterFont Font = PrinterFont.FontA
);

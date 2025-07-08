namespace ReceiptPrinterEmulator.Emulator;

public sealed record BarcodeConfiguration(
    // 1 -> 6
    // Default is 2
    int Width = 2,
    // 1 -> 255
    // Default is 100
    int Height = 100
);

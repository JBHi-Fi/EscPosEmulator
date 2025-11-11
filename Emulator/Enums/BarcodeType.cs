namespace ReceiptPrinterEmulator.Emulator.Enums;

public enum BarcodeType : byte
{
    UPC_A,
    UPC_E,
    JAN13,
    JAN8,
    CODE39,
    ITF,
    CODABAR,
    CODE93,
    CODE128,
    GS1_128,
    GS1_DATABAR_OMNIDIRECTIONAL,
    GS1_DATABAR_TRUNCATED,
    GS1_DATABAR_LIMITED,
    GS1_DATABAR_EXPANDED,
    CODE128_AUTO,
}

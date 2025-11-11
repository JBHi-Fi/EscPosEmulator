using System;
using ReceiptPrinterEmulator.Emulator;
using ReceiptPrinterEmulator.Logging;

namespace ReceiptPrinterEmulator.EscPos.Commands.GS;

/// <summary>
/// Graphics data command
/// </summary>
public class Barcode2dDataCmd : DataCommand
{
    public override ReadOnlySpan<byte> Prefix => [EscPosInterpreter.GS, (byte)'(', (byte)'k'];

    public override void Execute(ReceiptPrinter printer)
    {
        switch (m)
        {
            case 48: // PDF417
                PDF417(printer, fn, data ?? []);
                break;
            case 49: // QR Code
                QRCode(printer, fn, data ?? []);
                break;
            case 50: // MaxiCode
                MaxiCode(printer, fn, data ?? []);
                break;
            case 51: // 2D GS1 DataBar
                GS1DataBar(printer, fn, data ?? []);
                break;
            case 52: // Composite Symbology
                CompositeSymbology(printer, fn, data ?? []);
                break;
            case 53: // Aztec Code
                AztecCode(printer, fn, data ?? []);
                break;
            case 54: // Data Matrix
                DataMatrix(printer, fn, data ?? []);
                break;
            default:
                // Unsupported 2D barcode type
                Logger.Info($"Unsupported 2D barcode type: {m}");
                break;
        }
    }

    private static void PDF417(ReceiptPrinter printer, byte fn, byte[] data)
    {
        Logger.Info($"PDF417 2D barcode cmd fn {fn}");
    }

    private static void QRCode(ReceiptPrinter printer, byte fn, byte[] data)
    {
        Logger.Info($"QR Code 2D barcode cmd fn {fn}");
    }

    private static void MaxiCode(ReceiptPrinter printer, byte fn, byte[] data)
    {
        Logger.Info($"MaxiCode 2D barcode cmd fn {fn}");
    }

    private static void GS1DataBar(ReceiptPrinter printer, byte fn, byte[] data)
    {
        Logger.Info($"GS1 DataBar 2D barcode cmd fn {fn}");
    }

    private static void CompositeSymbology(ReceiptPrinter printer, byte fn, byte[] data)
    {
        Logger.Info($"Composite Symbology 2D barcode cmd fn {fn}");
    }

    private static void AztecCode(ReceiptPrinter printer, byte fn, byte[] data)
    {
        Logger.Info($"Aztec Code 2D barcode cmd fn {fn}");
    }

    private static void DataMatrix(ReceiptPrinter printer, byte fn, byte[] data)
    {
        Logger.Info($"Data Matrix 2D barcode cmd fn {fn}");
    }
}

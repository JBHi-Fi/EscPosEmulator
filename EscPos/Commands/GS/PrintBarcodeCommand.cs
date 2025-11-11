using System;
using System.Collections.Generic;
using ReceiptPrinterEmulator.Emulator;
using ReceiptPrinterEmulator.Emulator.Enums;
using ReceiptPrinterEmulator.Logging;

namespace ReceiptPrinterEmulator.EscPos.Commands.GS;

public class PrintBarcodeCommand : BaseCommand
{
    public override ReadOnlySpan<byte> Prefix => [EscPosInterpreter.GS, (byte)'k'];
    public override bool HasArgs => true;

    private int n = 0;
    private byte m = 0x00;
    private bool mode1 = true;
    private int n2 = 0x00;
    private List<byte>? data = null;

    public override bool InterpretNextChar(byte c)
    {
        switch (n++)
        {
            case 0:
                m = c;
                if (0 <= m && m <= 20)
                {
                    mode1 = true;
                }
                else if (65 <= m && m <= 90)
                {
                    mode1 = false;
                }
                else
                {
                    Logger.Info($"Invalid mode for PrintBarcodeCommand: {m}");
                    return false;
                }
                return true;
            case 1:
                if (mode1)
                {
                    data = [];
                    data.Add(c);
                }
                else
                {
                    n2 = c;
                    data = new List<byte>(n2);
                }
                return true;
            default:
                if (mode1)
                {
                    if (c == 0x00)
                    {
                        return false; // End of data for mode 1
                    }
                    else
                    {
                        data!.Add(c);
                        return true; // Continue collecting data for mode 1
                    }
                }
                else
                {
                    data!.Add(c);
                    return data!.Count < n2;
                }
        }
    }

    public override void Reset()
    {
        n = 0;
        m = 0x00;
        mode1 = true;
        n2 = 0x00;
        data = null;
    }

    public override void Execute(ReceiptPrinter printer)
    {
        printer.PrintBarcode(m switch
        {
            0 or 65 => BarcodeType.UPC_A,
            1 or 66 => BarcodeType.UPC_E,
            2 or 67 => BarcodeType.JAN13,
            3 or 68 => BarcodeType.JAN8,
            4 or 69 => BarcodeType.CODE39,
            5 or 70 => BarcodeType.ITF,
            6 or 71 => BarcodeType.CODABAR,
            72 => BarcodeType.CODE93,
            73 => BarcodeType.CODE128,
            74 => BarcodeType.GS1_128,
            75 => BarcodeType.GS1_DATABAR_OMNIDIRECTIONAL,
            76 => BarcodeType.GS1_DATABAR_TRUNCATED,
            77 => BarcodeType.GS1_DATABAR_LIMITED,
            78 => BarcodeType.GS1_DATABAR_EXPANDED,
            _ => BarcodeType.CODE128,
        }, data ?? []);
    }
}

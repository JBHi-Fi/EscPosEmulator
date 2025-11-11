using ReceiptPrinterEmulator.Emulator.Abstraction;
using ReceiptPrinterEmulator.Emulator.Enums;
using ReceiptPrinterEmulator.Emulator.Printables;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace ReceiptPrinterEmulator.Emulator;

public class Receipt(
    PaperConfiguration paperConfiguration,
    PrintMode printMode,
    BarcodeConfiguration barcodeConfiguration,
    int lineSpacing
)
{
    public readonly string Guid = System.Guid.NewGuid().ToString();

    private int PaperWidth => paperConfiguration.GetPaperWidthInPixels();
    private int PrintWidth => paperConfiguration.GetPrintWidthInPixels();
    private int PaperMargins => (PaperWidth - PrintWidth) / 2;

    private List<IReceiptPrintable> _renderLines = [];
    private ReceiptTextLine? _currentTextLine = null;
    private PrintMode _printMode = printMode;
    private BarcodeConfiguration _barcodeConfiguration = barcodeConfiguration;
    private int _tabSpacing = paperConfiguration.DefaultTabSpacing;

    public bool IsEmpty =>
        (_currentTextLine == null || _currentTextLine.IsEmpty) && _renderLines.Count == 0;

    public void ChangeFontConfiguration(PrintMode printMode)
    {
        // FinalizeTextLine(false);

        _printMode = printMode;
        _currentTextLine?.SetFont(paperConfiguration.GetFont(printMode.Font));
    }

    public void ChangeBarcodeConfiguration(BarcodeConfiguration barcodeConfiguration)
    {
        _barcodeConfiguration = barcodeConfiguration;
    }

    public void SetLineSpacing(int value)
    {
        lineSpacing = value;
    }

    public void SetTabSpacing(int value)
    {
        _tabSpacing = value;
    }

    private ReceiptTextLine CreateNewTextLine() => new(paperConfiguration, _printMode);

    public void PrintText(string text, PrintMode printMode)
    {
        if (_currentTextLine is null)
            _currentTextLine = CreateNewTextLine();

        for (var i = 0; i < text.Length; i++)
        {
            var canContinue = _currentTextLine.TryWriteChar(text[i], printMode);

            if (!canContinue)
            {
                FinalizeTextLine(true);

                _currentTextLine = CreateNewTextLine();
                canContinue = _currentTextLine.TryWriteChar(text[i], printMode);

                if (!canContinue)
                    throw new Exception("Logic error - line must be able to contain > 0 chars");
            }
        }
    }

    public void FinalizeTextLine(bool insertLineSpacing)
    {
        if (_currentTextLine == null || _currentTextLine.IsEmpty)
        {
            var font = paperConfiguration.GetFont(_printMode.Font);
            _renderLines.Add(
                new ReceiptEmptyLine(font.CharacterHeight * _printMode.CharHeightScale)
            );
        }
        else
        {
            if (!_currentTextLine.IsEmpty)
                _renderLines.Add(_currentTextLine);
            _currentTextLine = null;
        }

        if (insertLineSpacing)
        {
            _renderLines.Add(new ReceiptEmptyLine(lineSpacing));
        }
    }

    public void AdvanceToNewLine() => FinalizeTextLine(true);

    public void PrintBarcode(BarcodeType type, string barcode)
    {
        FinalizeTextLine(false);

        PrintMode barcodePrintMode = new()
        {
            Font = _barcodeConfiguration.Font,
            Justification = TextJustification.Center,
        };
        ReceiptTextLine barcodeHriLine = new(paperConfiguration, barcodePrintMode);
        bool hriCanContinue = true;
        for (var i = 0; i < barcode.Length && hriCanContinue; i++)
        {
            hriCanContinue = barcodeHriLine.TryWriteChar(barcode[i], barcodePrintMode);
        }

        if (_barcodeConfiguration.HriPrintPosition == HriPrintPosition.Above || _barcodeConfiguration.HriPrintPosition == HriPrintPosition.AboveAndBelow)
        {
            _renderLines.Add(barcodeHriLine);
            _renderLines.Add(new ReceiptEmptyLine(10));
        }
        _renderLines.Add(
            new ReceiptBarcodeLine(paperConfiguration, type, _barcodeConfiguration, barcode)
        );
        if (_barcodeConfiguration.HriPrintPosition == HriPrintPosition.Below || _barcodeConfiguration.HriPrintPosition == HriPrintPosition.AboveAndBelow)
        {
            _renderLines.Add(new ReceiptEmptyLine(10));
            _renderLines.Add(barcodeHriLine);
        }
    }

    public void PrintBitmap(Bitmap image)
    {
        FinalizeTextLine(false);

        _renderLines.Add(new ReceiptBitmapLine(paperConfiguration, image));
    }

    public int GetTotalPrintHeight() => _renderLines.Sum(line => line.GetPrintHeight());

    public int GetTotalPaperHeight() => GetTotalPrintHeight() + (PaperMargins * 2);

    public Bitmap Render(bool drawPartials = true)
    {
        var paperWidth = PaperWidth;
        var paperHeight = GetTotalPaperHeight();

        var bmp = new Bitmap(paperWidth, paperHeight);
        using var g = Graphics.FromImage(bmp);

        // Fill white background
        g.FillRectangle(Brushes.White, 0, 0, paperWidth, paperHeight);

        // Draw all rendered lines
        var offsetX = PaperMargins;
        var offsetY = PaperMargins;

        foreach (var line in _renderLines)
        {
            line.Render(bmp, g, offsetX, offsetY);
            offsetY += line.GetPrintHeight();
        }

        return bmp;
    }
}

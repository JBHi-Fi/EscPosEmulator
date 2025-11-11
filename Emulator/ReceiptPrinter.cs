using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using ReceiptPrinterEmulator.Emulator.Enums;
using ReceiptPrinterEmulator.EscPos;
using ReceiptPrinterEmulator.Logging;

namespace ReceiptPrinterEmulator.Emulator;

public class ReceiptPrinter
{
    private readonly PaperConfiguration _paperConfiguration;
    private readonly EscPosInterpreter _escPosInterpreter;

    private PrintMode _printMode;
    private BarcodeConfiguration _barcodeConfiguration;
    private int _lineSpacing;
    private int _tabSpacing;
    private Bitmap? _printBuffer = null;

    public Receipt CurrentReceipt { get; private set; }
    public List<Receipt> ReceiptStack { get; private set; }

    public event EventHandler<EventArgs> OnActivityEvent;

    public ReceiptPrinter(PaperConfiguration paperConfiguration)
    {
        _paperConfiguration = paperConfiguration;
        _escPosInterpreter = new(this);

        _printMode = new();
        _barcodeConfiguration = new();

        ReceiptStack = [];

        StartNewReceipt();
        PowerCycle();
    }

    #region ESC/POS

    public void FeedEscPos(ReadOnlySpan<byte> input)
    {
        if (input.Length > 10000)
        {
            File.WriteAllBytes("last_ticket.bin", input);
        }
        File.WriteAllBytes("last_escpos_receive.txt", input);

        try
        {
            Logger.Info($"Received {input.Length} bytes of ESC/POS data");
            _escPosInterpreter.Interpret(input);
        }
        catch (Exception ex)
        {
            Logger.Exception(ex, "ESC/POS Interpreter Error");
        }

        OnActivityEvent?.Invoke(this, EventArgs.Empty);
    }

    #endregion

    #region Receipt meta

    public void StartNewReceipt()
    {
        CurrentReceipt = new(_paperConfiguration, _printMode, _barcodeConfiguration, _lineSpacing);
        ReceiptStack.Add(CurrentReceipt);

        Logger.Info($"Starting new receipt (#{ReceiptStack.Count})");
    }

    #endregion

    #region Emulated

    public void PowerCycle()
    {
        Initialize();
    }

    #endregion

    #region Direct API

    public void Initialize()
    {
        _escPosInterpreter.ClearBuffers();

        SelectFont(PrinterFont.FontA);
        SelectJustification(TextJustification.Left);
        SelectCharacterSize(1, 1);
        SelectEmphasizeMode(false);
        SelectItalicMode(false);
        SelectUnderlineMode(UnderlineMode.Off);
        SetDefaultLineSpacing();
        SetDefaultTabSpacing();
    }

    public void PrintText(IReadOnlyList<byte> bytes)
    {
        if (bytes.Count == 0)
            return;

        var text = Encoding.ASCII.GetString([.. bytes]);

        Logger.Info($"Print: [{text}]");

        CurrentReceipt.PrintText(text, _printMode);
    }

    public void Cut(
        CutFunction cutFunction = CutFunction.Cut,
        CutShape cutShape = CutShape.Full,
        int n = 0
    )
    {
        Logger.Info($"Execute cut: {cutFunction}, {cutShape}, {n}");

        LineFeed();

        // TODO Support alternate cut modes

        StartNewReceipt();
    }

    /// <summary>
    /// Feeds one line, based on the current line spacing.
    /// </summary>
    /// <remarks>
    /// - The amount of paper fed per line is based on the value set using the line spacing command (ESC 2 or ESC 3).
    /// </remarks>
    public void LineFeed()
    {
        Logger.Info($"Line feed");
        CurrentReceipt.AdvanceToNewLine();
    }

    public void SelectFont(PrinterFont printerFont)
    {
        Logger.Info($"Select font: {printerFont}");

        _printMode = _printMode with { Font = printerFont };
        CurrentReceipt.ChangeFontConfiguration(_printMode);
    }

    public void SelectJustification(TextJustification justification)
    {
        Logger.Info($"Select justification: {justification}");

        _printMode = _printMode with { Justification = justification };
        CurrentReceipt.ChangeFontConfiguration(_printMode);
    }

    public void SelectCharacterSize(int width, int height)
    {
        Logger.Info($"Set character size scale: x{width} width, x{height} height");

        _printMode = _printMode with { CharWidthScale = width, CharHeightScale = height };
        CurrentReceipt.ChangeFontConfiguration(_printMode);
    }

    public void SelectEmphasizeMode(bool enable)
    {
        Logger.Info($"Set emphasize mode: {enable}");

        _printMode = _printMode with { Emphasize = enable };
        CurrentReceipt.ChangeFontConfiguration(_printMode);
    }

    public void SelectItalicMode(bool enable)
    {
        Logger.Info($"Set italic mode: {enable}");

        _printMode = _printMode with { Italic = enable };
        CurrentReceipt.ChangeFontConfiguration(_printMode);
    }

    public void SelectUnderlineMode(UnderlineMode mode)
    {
        Logger.Info($"Set underline mode: {mode}");

        _printMode = _printMode with { Underline = mode };
        CurrentReceipt.ChangeFontConfiguration(_printMode);
    }

    public void SelectUpsideDownMode(bool enable)
    {
        Logger.Info($"Set upside down mode: {enable}");

        _printMode = _printMode with { UpsideDown = enable };
        CurrentReceipt.ChangeFontConfiguration(_printMode);
    }

    public void SelectInvertedMode(bool enable)
    {
        Logger.Info($"Set inverted mode: {enable}");

        _printMode = _printMode with { Inverted = enable };
        CurrentReceipt.ChangeFontConfiguration(_printMode);
    }

    public void SetLineSpacing(int value)
    {
        Logger.Info($"Set line spacing: {value}");

        _lineSpacing = value;
        CurrentReceipt.SetLineSpacing(_lineSpacing);
    }

    public void SetTabSpacing(int value)
    {
        Logger.Info($"Set tab spacing: {value}");

        _tabSpacing = value;
        CurrentReceipt.SetTabSpacing(_tabSpacing);
    }

    public void SetDefaultLineSpacing() => SetLineSpacing(_paperConfiguration.DefaultLineSpacing);

    public void SetDefaultTabSpacing() => SetTabSpacing(_paperConfiguration.DefaultTabSpacing);

    public void SetBarcodeWidthMultiplier(int widthMultiplier)
    {
        Logger.Info($"Set barcode width multiplier: {widthMultiplier}");

        _barcodeConfiguration = _barcodeConfiguration with { Width = widthMultiplier };
        CurrentReceipt.ChangeBarcodeConfiguration(_barcodeConfiguration);
    }

    public void SetBarcodeHeight(int height)
    {
        Logger.Info($"Set barcode height: {height}");

        _barcodeConfiguration = _barcodeConfiguration with { Height = height };
        CurrentReceipt.ChangeBarcodeConfiguration(_barcodeConfiguration);
    }

    public void SetBarcodeHriPrintPosition(HriPrintPosition position)
    {
        Logger.Info($"Set barcode HRI print position: {position}");

        _barcodeConfiguration = _barcodeConfiguration with { HriPrintPosition = position };
        CurrentReceipt.ChangeBarcodeConfiguration(_barcodeConfiguration);
    }

    public void SetBarcodeHriFont(PrinterFont font)
    {
        Logger.Info($"Set barcode HRI font: {font}");

        _barcodeConfiguration = _barcodeConfiguration with { Font = font };
        CurrentReceipt.ChangeBarcodeConfiguration(_barcodeConfiguration);
    }

    public void PrintBarcode(BarcodeType type, string barcode)
    {
        Logger.Info($"Print barcode: {barcode} ({type})");

        CurrentReceipt.PrintBarcode(type, barcode);
    }

    public void PrintBitmap(Bitmap bitmap)
    {
        Logger.Info($"Print bitmap: {bitmap.Width}x{bitmap.Height}");

        CurrentReceipt.PrintBitmap(bitmap);
    }

    public void SetPrintBuffer(Bitmap bitmap)
    {
        Logger.Info($"Set print buffer: {bitmap.Width}x{bitmap.Height}");

        _printBuffer = bitmap;
    }

    public Bitmap? GetPrintBuffer()
    {
        Logger.Info($"Get print buffer: {_printBuffer?.Width}x{_printBuffer?.Height}");

        return _printBuffer;
    }

    #endregion

    #region Command API

    /// <summary>
    /// Prints the data in the print buffer and feeds one line, based on the current line spacing.
    /// </summary>
    public void PrintAndLineFeed(IReadOnlyList<byte> printBuffer)
    {
        PrintText(printBuffer);
        LineFeed();
    }

    public void PrintTab(int beforeTabLength)
    {
        int mod = beforeTabLength % _tabSpacing;
        if (mod == 0 && beforeTabLength > 0)
            return;

        string tabs = "";
        for (var i = mod; i < _tabSpacing; i++)
            tabs += " ";

        CurrentReceipt.PrintText(tabs, _printMode);
    }

    #endregion
}

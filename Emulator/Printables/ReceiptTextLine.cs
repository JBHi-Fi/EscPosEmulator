using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using ReceiptPrinterEmulator.Emulator.Abstraction;
using ReceiptPrinterEmulator.Emulator.Enums;

namespace ReceiptPrinterEmulator.Emulator.Printables;

public class ReceiptTextLine : IReceiptPrintable
{
    private PaperConfiguration.FontConfiguration _font;
    private readonly int _printWidth;

    private int _totalWidth;
    private readonly List<(string text, PrintMode mode)> _strings = [];

    public bool IsEmpty => _strings.Count == 0;

    public ReceiptTextLine(PaperConfiguration paperConfiguration, PrintMode printMode)
    {
        _font = paperConfiguration.GetFont(printMode.Font);
        _printWidth = paperConfiguration.GetPrintWidthInPixels();

        _totalWidth = 0;
    }

    public void SetFont(PaperConfiguration.FontConfiguration font)
    {
        _font = font;
    }

    public bool TryWriteChar(char c, PrintMode mode)
    {
        int charWidth = _font.CharacterWidth * mode.CharWidthScale;
        if ((_totalWidth + charWidth) >= _printWidth)
            return false;

        if (_strings.Count > 0 && mode.Equals(_strings[^1].mode))
        {
            // Append to last run
            var (text, lastMode) = _strings[^1];
            _strings[^1] = (text + c, lastMode);
        }
        else
        {
            // Start new run
            _strings.Add((c.ToString(), mode));
        }
        _totalWidth += charWidth;
        return true;
    }

    public int GetPrintHeight()
    {
        return _strings.Select(t => t.mode.CharHeightScale).Max() * _font.CharacterHeight;
    }

    public void Render(Bitmap bitmap, Graphics g, int offsetX, int offsetY)
    {
        // 1. Measure total line width for justification
        float totalWidth = 0;
        var runWidths = new List<float>();
        foreach (var (text, mode) in _strings)
        {
            int scaledWidth = text.Length * _font.CharacterWidth * mode.CharWidthScale;
            runWidths.Add(scaledWidth);
            totalWidth += scaledWidth;
        }

        // 2. Use the justification of the first run (ESC/POS line property)
        TextJustification justification =
            _strings.Count > 0 ? _strings[0].mode.Justification : TextJustification.Left;
        int x = offsetX;
        if (justification == TextJustification.Center)
            x += (int)((_printWidth - totalWidth) / 2);
        else if (justification == TextJustification.Right)
            x += (int)(_printWidth - totalWidth);

        // Find the tallest run in this line for baseline alignment
        int maxCharHeightScale = _strings.Select(t => t.mode.CharHeightScale).Max();

        foreach (var (text, mode) in _strings)
        {
            float baseCharHeight = _font.CharacterHeight * 0.75f;

            var fontStyle = FontStyle.Regular;
            if (mode.Emphasize)
                fontStyle |= FontStyle.Bold;
            if (mode.Italic)
                fontStyle |= FontStyle.Italic;

            using var font = new Font(_font.RenderFont, baseCharHeight, fontStyle);

            // Font metrics for baseline alignment
            var ascent = (float)font.FontFamily.GetCellAscent(font.Style);
            var descent = (float)font.FontFamily.GetCellDescent(font.Style);
            var scaleDiff = maxCharHeightScale - mode.CharHeightScale;
            float baselineOffset =
                _font.CharacterHeight * scaleDiff
                - scaleDiff * (_font.CharacterHeight * descent / (ascent + descent));

            var state = g.Save();

            // Align baseline of run to line baseline
            g.TranslateTransform(x, offsetY + baselineOffset);
            g.ScaleTransform(mode.CharWidthScale, mode.CharHeightScale);

            if (mode.Inverted)
                g.FillRectangle(
                    Brushes.Black,
                    0,
                    0,
                    _font.CharacterWidth * text.Length,
                    _font.CharacterHeight
                );

            for (int i = 0; i < text.Length; i++)
            {
                var c = text.Substring(i, 1);
                SizeF charSize = g.MeasureString(
                    c,
                    font,
                    _font.CharacterWidth * 2,
                    StringFormat.GenericTypographic
                );
                g.DrawString(
                    c,
                    font,
                    mode.Inverted ? Brushes.White : Brushes.Black,
                    i * _font.CharacterWidth - (_font.CharacterWidth - charSize.Width) / 2f,
                    0,
                    StringFormat.GenericTypographic
                );
            }

            // Underline (draw in scaled context)
            if (mode.Underline is UnderlineMode.OnOneDot or UnderlineMode.OnTwoDots)
            {
                var dotHeight = mode.Underline is UnderlineMode.OnTwoDots ? 2 : 1;
                g.DrawLine(
                    new Pen(Color.Black, dotHeight),
                    0,
                    _font.CharacterHeight,
                    _font.CharacterWidth * text.Length,
                    _font.CharacterHeight
                );
            }

            g.Restore(state);

            x += _font.CharacterWidth * text.Length * mode.CharWidthScale;
        }
    }
}

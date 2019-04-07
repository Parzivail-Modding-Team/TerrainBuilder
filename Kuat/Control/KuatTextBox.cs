using System;
using NanoVGDotNet.NanoVG;
using OpenTK;
using OpenTK.Input;

namespace Kuat.Control
{
    public class KuatTextBox : KuatControl
    {
        private int _selectionStart = 0;
        private int _selectionLength = 0;
        private DateTime _moveTime;

        /// <inheritdoc />
        public KuatTextBox(string name) : base(name)
        {
        }

        /// <inheritdoc />
        protected override void OnPaint(object sender, NvgContext e)
        {
            base.OnPaint(sender, e);

            e.BeginPath();
            e.RoundedRect(ClientLocation.X, ClientLocation.Y, Size.Width, Size.Height, 2);

            e.FillPaint(e.LinearGradient(ClientLocation.X, ClientLocation.Y, ClientLocation.X + Size.Width,
                    ClientLocation.Y, NanoVg.Rgba(0xFFD9DFE3), NanoVg.Rgba(0xFFBDC8D1)));
            e.Fill();

            e.StrokeColor(NanoVg.Rgba(0xFF192025));
            e.Stroke();

            e.FillColor(NanoVg.Rgba(ForeColor));
            e.FontFace(Font.Family);
            e.FontSize(Font.Size);
            e.TextAlign(NvgAlign.Left | NvgAlign.Top);
            e.Text(ClientLocation.X + 2, ClientLocation.Y + 2, Text);

            var width = e.TextBounds(0, 0, Text.Substring(0, _selectionStart), null);

            var blinkTime = KNativeMethods.GetCaretBlinkTime();
            if (HasFocus && ((DateTime.Now - _moveTime).TotalMilliseconds < blinkTime || KMath.GetTodayMs() % (2 * blinkTime) < blinkTime))
            {
                e.BeginPath();
                e.MoveTo(ClientLocation.X + 2 + width, ClientLocation.Y + 2);
                e.LineTo(ClientLocation.X + 2 + width, ClientLocation.Y + Size.Height - 4);

                e.StrokeColor(NanoVg.Rgba(0xFF192025));
                e.Stroke();
            }
        }

        /// <inheritdoc />
        protected override void OnKeyPress(object sender, KeyPressEventArgs e)
        {
            base.OnKeyPress(sender, e);
            if (_selectionLength == 0)
            {
                var textUntilCaret = Text.Substring(0, _selectionStart);
                var textAfterCaret = Text.Substring(_selectionStart);
                Text = textUntilCaret + e.KeyChar + textAfterCaret;
                _selectionStart++;
                _moveTime = DateTime.Now;
                ValidateSelection();
            }
        }

        private void ValidateSelection()
        {
            if (_selectionStart < 0)
                _selectionStart = 0;
            if (_selectionStart > Text.Length)
                _selectionStart = Text.Length;
            if (_selectionLength < 0)
                _selectionLength = 0;
            if (_selectionStart + _selectionLength > Text.Length)
                _selectionLength = Text.Length - _selectionStart;
        }

        /// <inheritdoc />
        protected override void OnKeyDown(object sender, KeyboardKeyEventArgs e)
        {
            base.OnKeyDown(sender, e);

            switch (e.Key)
            {
                case Key.BackSpace:
                    if (_selectionLength == 0)
                    {
                        var textUntilCaret = Text.Substring(0, _selectionStart);
                        if (textUntilCaret.Length != 0)
                        {
                            var textAfterCaret = Text.Substring(_selectionStart);
                            Text = textUntilCaret.Substring(0, textUntilCaret.Length - 1) + textAfterCaret;
                            _selectionStart--;
                            _moveTime = DateTime.Now;
                            ValidateSelection();
                        }
                    }
                    else
                        DeleteSelection();
                    break;
                case Key.Delete:
                    if (_selectionLength == 0)
                    {
                        var textAfterCaret = Text.Substring(_selectionStart);
                        if (textAfterCaret.Length != 0)
                        {
                            var textUntilCaret = Text.Substring(0, _selectionStart);
                            Text = textUntilCaret + textAfterCaret.Substring(1, textAfterCaret.Length - 1);
                            ValidateSelection();
                        }
                    }
                    else
                        DeleteSelection();
                    break;
                case Key.Left:
                    _selectionStart--;
                    _moveTime = DateTime.Now;
                    ValidateSelection();
                    break;
                case Key.Right:
                    _selectionStart++;
                    _moveTime = DateTime.Now;
                    ValidateSelection();
                    break;
            }
        }

        private void DeleteSelection()
        {
            var textUntilCaret = Text.Substring(0, _selectionStart);
            var textAfterSelection = Text.Substring(_selectionStart + _selectionLength);
            Text = textUntilCaret + textAfterSelection;
            ValidateSelection();
            _selectionLength = 0;
        }
    }
}
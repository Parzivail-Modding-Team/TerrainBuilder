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

        public int SelectionStart
        {
            get => _selectionStart;
            set
            {
                _selectionStart = value;
                ValidateSelection();
            }
        }

        public int SelectionLength
        {
            get => _selectionLength;
            set
            {
                _selectionLength = value;
                ValidateSelection();
            }
        }

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

            var width = e.TextBounds(0, 0, Text.Substring(0, SelectionStart), null);

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
        protected override void OnTextChanged(object sender, EventArgs e)
        {
            ValidateSelection();
            base.OnTextChanged(sender, e);
        }

        /// <inheritdoc />
        protected override void OnKeyPress(object sender, KeyPressEventArgs e)
        {
            base.OnKeyPress(sender, e);
            if (SelectionLength == 0)
            {
                var textUntilCaret = Text.Substring(0, SelectionStart);
                var textAfterCaret = Text.Substring(SelectionStart);
                Text = textUntilCaret + e.KeyChar + textAfterCaret;
                SelectionStart++;
            }
        }

        /// <inheritdoc />
        protected internal override void OnFocus(object sender, EventArgs e)
        {
            base.OnFocus(sender, e);
            ValidateSelection();
        }

        private void ValidateSelection()
        {
            _moveTime = DateTime.Now;
            if (SelectionStart < 0)
                SelectionStart = 0;
            if (SelectionStart > Text.Length)
                SelectionStart = Text.Length;
            if (SelectionLength < 0)
                SelectionLength = 0;
            if (SelectionStart + SelectionLength > Text.Length)
                SelectionLength = Text.Length - SelectionStart;
        }

        /// <inheritdoc />
        protected override void OnKeyDown(object sender, KeyboardKeyEventArgs e)
        {
            base.OnKeyDown(sender, e);

            switch (e.Key)
            {
                case Key.BackSpace:
                    if (SelectionLength == 0)
                    {
                        var textUntilCaret = Text.Substring(0, SelectionStart);
                        if (textUntilCaret.Length != 0)
                        {
                            var textAfterCaret = Text.Substring(SelectionStart);
                            SelectionStart--;
                            Text = textUntilCaret.Substring(0, textUntilCaret.Length - 1) + textAfterCaret;
                        }
                    }
                    else
                        DeleteSelection();
                    break;
                case Key.Delete:
                    if (SelectionLength == 0)
                    {
                        var textAfterCaret = Text.Substring(SelectionStart);
                        if (textAfterCaret.Length != 0)
                        {
                            var textUntilCaret = Text.Substring(0, SelectionStart);
                            Text = textUntilCaret + textAfterCaret.Substring(1, textAfterCaret.Length - 1);
                        }
                    }
                    else
                        DeleteSelection();
                    break;
                case Key.Left:
                    SelectionStart--;
                    break;
                case Key.Right:
                    SelectionStart++;
                    break;
            }
        }

        private void DeleteSelection()
        {
            var textUntilCaret = Text.Substring(0, SelectionStart);
            var textAfterSelection = Text.Substring(SelectionStart + SelectionLength);
            Text = textUntilCaret + textAfterSelection;
            ValidateSelection();
            SelectionLength = 0;
        }
    }
}
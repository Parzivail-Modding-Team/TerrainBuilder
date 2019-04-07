using System;
using System.Text.RegularExpressions;
using NanoVGDotNet.NanoVG;
using OpenTK;
using OpenTK.Input;

namespace Kuat.Control
{
    public class KuatTextBox : KuatControl
    {
        private int _selectionStart = 0;
        private int _selectionLength = 0;
        private int _clientRenderOffsetX = 0;

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

            var selectionWidth = (int)e.TextBounds(0, 0, Text.Substring(0, SelectionStart), null);

            if (HasFocus)
            {
                if (_clientRenderOffsetX + selectionWidth > Size.Width - 5)
                {
                    _clientRenderOffsetX = -selectionWidth + 10;
                }
                else if (_clientRenderOffsetX + selectionWidth < 5)
                {
                    _clientRenderOffsetX = Math.Min(Size.Width - selectionWidth - 10, 0);
                }
            }
            else
                _clientRenderOffsetX = 0;

            e.Scissor(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);
            e.Translate(_clientRenderOffsetX, 0);
            e.FillColor(NanoVg.Rgba(ForeColor));
            e.FontFace(Font.Family);
            e.FontSize(Font.Size);
            e.TextAlign(NvgAlign.Left | NvgAlign.Top);
            e.Text(ClientLocation.X + 2, ClientLocation.Y + 2, Text);

            var blinkTime = KNativeMethods.GetCaretBlinkTime();
            if (HasFocus && ((DateTime.Now - _moveTime).TotalMilliseconds < blinkTime || KMath.GetTodayMs() % (2 * blinkTime) < blinkTime))
            {
                e.BeginPath();
                e.MoveTo(ClientLocation.X + 2 + selectionWidth, ClientLocation.Y + 2);
                e.LineTo(ClientLocation.X + 2 + selectionWidth, ClientLocation.Y + Size.Height - 4);

                e.StrokeColor(NanoVg.Rgba(0xFF192025));
                e.Stroke();
            }
            e.ResetScissor();
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
                Text = Text.Insert(SelectionStart, e.KeyChar.ToString());
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
            if (SelectionStart + SelectionLength < 0)
                SelectionLength = -SelectionStart;
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
                        Text = Text.Select(SelectionStart, -1, SelectionMode.Outside);
                        if (SelectionStart != Text.Length)
                            SelectionStart--;
                    }
                    else
                        DeleteSelection();
                    break;
                case Key.Delete:
                    if (SelectionLength == 0)
                        Text = Text.Select(SelectionStart, 1, SelectionMode.Outside);
                    else
                        DeleteSelection();
                    break;
                case Key.Left:
                    if (e.Control)
                    {
                        var lastToken = Text.Substring(0, _selectionStart).LastIndexOf(" ", StringComparison.InvariantCulture);
                        if (lastToken != -1)
                            SelectionStart = (lastToken + 1);
                    }
                    else
                    {
                        SelectionStart--;
                        SelectionLength = 0;
                    }

                    break;
                case Key.Right:
                    if (e.Control)
                    {
                    }
                    else
                    {
                        SelectionStart++;
                        SelectionLength = 0;
                    }

                    break;
            }
        }

        private void DeleteSelection()
        {
            Text = Text.Select(SelectionStart, SelectionLength, SelectionMode.Outside);
            if (SelectionLength < 0)
                SelectionStart += SelectionLength;
            SelectionLength = 0;
        }
    }
}
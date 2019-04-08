using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using NanoVGDotNet.NanoVG;
using OpenTK.Input;
using KeyPressEventArgs = OpenTK.KeyPressEventArgs;

namespace Kuat.Control
{
    public class KuatTextBox : KuatControl
    {
        private int _selectionStart = 0;
        private int _selectionLength = 0;
        private int _clientRenderOffsetX = 0;
        private bool _freezeSelectionValidation;
        private DateTime _moveTime;
        private Point _deferMouseCaretPos;

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

            e.FontFace(Font.Family);
            e.FontSize(Font.Size);
            e.TextAlign(NvgAlign.Left | NvgAlign.Top);
            var selectionWidth = (int)e.TextBounds(0, 0, Text.Substring(0, SelectionStart), null);

            if (_deferMouseCaretPos != Point.Empty)
            {
                var prevQueryX = 0;
                for (var i = 0; i <= Text.Length; i++)
                {
                    var queryX = ClientLocation.X + _clientRenderOffsetX + (int)e.TextBounds(0, 0, Text.Substring(0, i), null);
                    var prevDist = _deferMouseCaretPos.X - prevQueryX;
                    var hereDist = _deferMouseCaretPos.X - queryX;

                    if (hereDist <= 0 && prevDist > 0)
                    {
                        SelectionStart = i;
                        break;
                    }

                    prevQueryX = queryX;
                }
                _deferMouseCaretPos = Point.Empty;
            }

            if (HasFocus)
            {
                if (_clientRenderOffsetX + selectionWidth > Size.Width - 5)
                {
                    _clientRenderOffsetX = -selectionWidth + Size.Width / 2;
                }
                else if (_clientRenderOffsetX + selectionWidth < 5)
                {
                    _clientRenderOffsetX = Math.Min(Size.Width - selectionWidth - Size.Width / 2, 0);
                }
            }
            else
                _clientRenderOffsetX = 0;

            e.Scissor(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);
            e.Translate(_clientRenderOffsetX, 0);
            e.FillColor(NanoVg.Rgba(ForeColor));
            if (SelectionLength == 0)
                e.Text(ClientLocation.X + 2, ClientLocation.Y + 2, Text);
            else
            {
                e.Text(ClientLocation.X + 2, ClientLocation.Y + 2, Text.Select(SelectionStart, SelectionLength, SelectionMode.Before));
                var w = e.TextBounds(0, 0, Text.Select(SelectionStart, SelectionLength, SelectionMode.Before), null);
                e.FillColor(NanoVg.Rgba(Color.Blue));
                var selectedWidth = e.TextBounds(0, 0, Text.Select(SelectionStart, SelectionLength), null);
                e.BeginPath();
                e.Rect(ClientLocation.X + 2 + w, ClientRectangle.Y + 1, selectedWidth, Size.Height - 4);
                e.Fill();
                e.FillColor(NanoVg.Rgba(Color.White));
                e.Text(ClientLocation.X + 2 + w, ClientLocation.Y + 2, Text.Select(SelectionStart, SelectionLength));
                w += selectedWidth;
                e.FillColor(NanoVg.Rgba(ForeColor));
                e.Text(ClientLocation.X + 2 + w, ClientLocation.Y + 2, Text.Select(SelectionStart, SelectionLength, SelectionMode.After));
            }

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
        protected override void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseDown(sender, e);
            SelectionStart = Text.Length;
            _deferMouseCaretPos = e.Position;
            SelectionLength = 0;
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
            InsertAtCursor(e.KeyChar.ToString());
        }

        private void InsertAtCursor(string s)
        {
            if (SelectionLength != 0)
                DeleteSelection();
            Text = Text.Insert(SelectionStart, s);
            SelectionStart += s.Length;
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
            if (_freezeSelectionValidation)
                return;
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
                case Key.A:
                    if (e.Control)
                    {
                        SelectionStart = Text.Length;
                        SelectionLength = -Text.Length;
                    }
                    break;
                case Key.X:
                    if (e.Control) Clipboard.SetText(DeleteSelection());
                    break;
                case Key.C:
                    if (e.Control) Clipboard.SetText(Text.Select(SelectionStart, SelectionLength));
                    break;
                case Key.V:
                    if (e.Control)
                    {
                        if (!Clipboard.ContainsText())
                            break;

                        InsertAtCursor(Clipboard.GetText());
                    }
                    break;
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
                        // TODO
                    }
                    else
                    {
                        _freezeSelectionValidation = true;

                        if (SelectionStart != 0)
                        {
                            if (e.Shift)
                                SelectionLength++;
                            else
                                SelectionLength = 0;
                            SelectionStart--;
                        }
                        else if (!e.Shift)
                            SelectionLength = 0;

                        _freezeSelectionValidation = false;
                        ValidateSelection();
                    }

                    break;
                case Key.Right:
                    if (e.Control)
                    {
                        // TODO
                    }
                    else
                    {
                        _freezeSelectionValidation = true;

                        if (SelectionStart != Text.Length)
                        {
                            SelectionStart++;
                            if (e.Shift)
                                SelectionLength--;
                            else
                                SelectionLength = 0;
                        }
                        else if (!e.Shift)
                            SelectionLength = 0;

                        _freezeSelectionValidation = false;
                        ValidateSelection();
                    }

                    break;
                case Key.Home:
                    SelectionStart = 0;
                    if (!e.Shift)
                        SelectionLength = 0;
                    break;
                case Key.End:
                    SelectionStart = Text.Length;
                    if (!e.Shift)
                        SelectionLength = 0;
                    break;
            }
        }

        private string DeleteSelection()
        {
            var deleted = Text.Select(SelectionStart, SelectionLength);
            Text = Text.Select(SelectionStart, SelectionLength, SelectionMode.Outside);
            if (SelectionLength < 0)
                SelectionStart += SelectionLength + 1;
            SelectionLength = 0;
            return deleted;
        }
    }
}
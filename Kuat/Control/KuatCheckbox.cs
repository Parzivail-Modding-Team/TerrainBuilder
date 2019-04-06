using System;
using System.Drawing;
using NanoVGDotNet.NanoVG;

namespace Kuat.Control
{
    public class KuatCheckbox : KuatControl
    {
        public bool Checked { get; set; }

        /// <inheritdoc />
        public KuatCheckbox(string name) : base(name)
        {
            Size = new Size(14, 14);
        }

        public event EventHandler<EventArgs> CheckedChanged;

        protected void OnCheckedChanged(object sender, EventArgs e)
        {
            CheckedChanged?.Invoke(sender, e);
        }

        /// <inheritdoc />
        protected override void OnPaint(object sender, NvgContext e)
        {
            base.OnPaint(sender, e);

            e.BeginPath();
            e.RoundedRect(ClientLocation.X, ClientLocation.Y, Size.Width, Size.Height, 2);

            e.FillColor(Checked ? NanoVg.Rgba(ActiveColor) : NanoVg.Rgba(0xFF475054));
            e.Fill();

            e.StrokeColor(NanoVg.Rgba(0xFF192025));
            e.Stroke();

            e.FillColor(NanoVg.Rgba(ForeColor));
            e.FontFace(Font.Family);
            e.FontSize(Font.Size);
            e.TextAlign(NvgAlign.Left | NvgAlign.Middle);
            e.Text(ClientLocation.X + Size.Width + 4, ClientLocation.Y + Size.Height / 2f, Text);
        }

        /// <inheritdoc />
        protected override void OnClick(object sender, MouseClickEventArgs e)
        {
            base.OnClick(sender, e);
            Checked = !Checked;
            OnCheckedChanged(sender, e);
        }
    }
}
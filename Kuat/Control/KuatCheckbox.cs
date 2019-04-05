using NanoVGDotNet.NanoVG;

namespace Kuat.Control
{
    public class KuatCheckbox : KuatControl
    {
        public bool Checked { get; set; }

        /// <inheritdoc />
        public KuatCheckbox(string name) : base(name)
        {
        }

        /// <inheritdoc />
        protected override void OnPaint(object sender, NvgContext e)
        {
            base.OnPaint(sender, e);

            e.BeginPath();
            e.RoundedRect(Location.X, Location.Y, 14, 14, 2);

            e.FillColor(Checked ? NanoVg.Rgba(ActiveColor) :
                Hover ? NanoVg.Rgba(HoverColor) : NanoVg.Rgba(BackColor));
            e.Fill();

            e.StrokeColor(NanoVg.Rgba(43, 51, 55, 255));
            e.Stroke();

            e.FillColor(NanoVg.Rgba(ForeColor));
            e.FontFace(Font.Family);
            e.FontSize(Font.Size);
            e.TextAlign(NvgAlign.Left | NvgAlign.Middle);
            e.Text(Location.X + 18, Location.Y + 7.5f, Text);
        }

        /// <inheritdoc />
        protected override void OnClick(object sender, MouseClickEventArgs e)
        {
            base.OnClick(sender, e);
            Checked = !Checked;
        }
    }
}
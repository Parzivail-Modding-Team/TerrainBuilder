using NanoVGDotNet.NanoVG;

namespace Kuat.Control
{
    public class KuatButton : KuatControl
    {
        /// <inheritdoc />
        public KuatButton(string name) : base(name)
        {
        }

        /// <inheritdoc />
        protected override void OnPaint(object sender, NvgContext e)
        {
            base.OnPaint(sender, e);

            e.BeginPath();
            e.RoundedRect(Location.X, Location.Y, Size.Width, Size.Height, 2);

            e.FillColor(
                Active ? NanoVg.Rgba(ActiveColor) : Hover ? NanoVg.Rgba(HoverColor) : NanoVg.Rgba(BackColor));
            e.Fill();

            e.StrokeColor(NanoVg.Rgba(43, 51, 55, 255));
            e.Stroke();

            e.FillColor(NanoVg.Rgba(ForeColor));
            e.FontFace(Font.Family);
            e.FontSize(Font.Size);
            e.TextAlign(NvgAlign.Center | NvgAlign.Middle);
            e.Text(Location.X + Size.Width / 2, Location.Y + Size.Height / 2, Text);
        }
    }
}
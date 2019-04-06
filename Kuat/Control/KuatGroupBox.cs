using NanoVGDotNet.NanoVG;

namespace Kuat.Control
{
    public class KuatGroupBox : KuatControl
    {
        /// <inheritdoc />
        public KuatGroupBox(string name) : base(name)
        {
        }

        /// <inheritdoc />
        protected override void OnPaint(object sender, NvgContext e)
        {
            base.OnPaint(sender, e);

            e.BeginPath();
            e.RoundedRect(ClientLocation.X, ClientLocation.Y, Size.Width, Size.Height, 2);

            e.StrokeColor(NanoVg.Rgba(43, 51, 55, 255));
            e.Stroke();

            e.FillColor(NanoVg.Rgba(ForeColor));
            e.FontFace(Font.Family);
            e.FontSize(Font.Size);
            e.TextAlign(NvgAlign.Baseline | NvgAlign.Left);
            e.Text(ClientLocation.X, ClientLocation.Y - 3, Text);
        }
    }
}
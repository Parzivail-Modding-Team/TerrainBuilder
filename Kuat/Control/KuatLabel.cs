using NanoVGDotNet.NanoVG;

namespace Kuat.Control
{
    public class KuatLabel : KuatControl
    {
        /// <inheritdoc />
        public KuatLabel(string name) : base(name)
        {
        }

        /// <inheritdoc />
        protected override void OnPaint(object sender, NvgContext e)
        {
            base.OnPaint(sender, e);

            e.FillColor(NanoVg.Rgba(ForeColor));
            e.FontFace(Font.Family);
            e.FontSize(Font.Size);
            e.TextAlign(NvgAlign.Left | NvgAlign.Top);
            e.Text(ClientLocation.X, ClientLocation.Y, Text);
        }
    }
}
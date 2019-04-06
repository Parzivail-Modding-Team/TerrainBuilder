using System.Drawing;
using NanoVGDotNet.NanoVG;

namespace Kuat.Control
{
    public class KuatIndicator : KuatControl
    {
        public bool Lit { get; set; }

        /// <inheritdoc />
        public KuatIndicator(string name) : base(name)
        {
            Size = new Size(8, 8);
        }

        /// <inheritdoc />
        protected override void OnPaint(object sender, NvgContext e)
        {
            base.OnPaint(sender, e);

            e.BeginPath();
            e.Circle(ClientLocation.X + Size.Width / 2f, ClientLocation.Y + Size.Height / 2f, Size.Width / 2f);

            e.FillColor(Lit ? NanoVg.Rgba(ActiveColor) : NanoVg.Rgba(0xFF475054));
            e.Fill();

            e.StrokeColor(NanoVg.Rgba(0xFF192025));
            e.Stroke();
        }
    }
}
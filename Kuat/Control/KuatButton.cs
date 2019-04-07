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
            e.RoundedRect(ClientLocation.X, ClientLocation.Y, Size.Width, Size.Height, 2);

            if (Active)
                e.FillPaint(e.LinearGradient(ClientLocation.X, ClientLocation.Y, ClientLocation.X,
                    ClientLocation.Y + Size.Height, NanoVg.Rgba(0xFF323B40), NanoVg.Rgba(0xFF323B40)));
            else
                e.FillPaint(e.LinearGradient(ClientLocation.X, ClientLocation.Y, ClientLocation.X,
                    ClientLocation.Y + Size.Height, NanoVg.Rgba(0xFF50595E), NanoVg.Rgba(0xFF2B3337)));
            e.Fill();

            e.StrokeColor(NanoVg.Rgba(0xFF192025));
            e.Stroke();

            e.BeginPath();
            e.RoundedRect(ClientLocation.X + 1, ClientLocation.Y + 1, Size.Width - 2, Size.Height - 2, 1);

            if (Active)
                e.StrokeColor(NanoVg.Rgba(0xFF293034));
            else
                e.StrokePaint(e.LinearGradient(ClientLocation.X, ClientLocation.Y, ClientLocation.X,
                    ClientLocation.Y + Size.Height, NanoVg.Rgba(0xFF6E757B), NanoVg.Rgba(0xFF323B40)));
            e.Stroke();

            e.FillColor(NanoVg.Rgba(ForeColor));
            e.FontFace(Font.Family);
            e.FontSize(Font.Size);
            e.TextAlign(NvgAlign.Center | NvgAlign.Middle);
            e.Text(ClientLocation.X + Size.Width / 2, ClientLocation.Y + Size.Height / 2, Text);
        }
    }
}
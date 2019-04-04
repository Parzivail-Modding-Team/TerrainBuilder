using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NanoVGDotNet.NanoVG;
using OpenTK.Input;

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

            e.FillColor(NanoVg.Rgba(BackColor));
            e.StrokeColor(NanoVg.Rgba(ForeColor));

            e.BeginPath();
			e.RoundedRect(Location.X, Location.Y, Size.Width, Size.Height, 4);
            e.Fill();
            e.BeginPath();
			e.RoundedRect(Location.X, Location.Y, Size.Width, Size.Height, 4);
            e.Stroke();

            e.FillColor(NanoVg.Rgba(ForeColor));
            e.FontFace(Font.Family);
            e.FontSize(Font.Size);
            e.TextAlign(NvgAlign.Center | NvgAlign.Middle);
            e.Text(Location.X + Size.Width / 2, Location.Y + Size.Height / 2, Text);
        } 
    }
}

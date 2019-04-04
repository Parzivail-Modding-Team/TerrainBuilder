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

            e.BeginPath();
            e.Rect(Location.X, Location.Y, Size.Width, Size.Height);
            e.Fill();

            e.FillColor(NanoVg.Rgba(ForeColor));
            e.FontFace(Font.Family);
            e.FontSize(Font.Size);
            e.TextAlign(NvgAlign.Center | NvgAlign.Middle);
            e.Text(Location.X + Size.Width / 2, Location.Y + Size.Height / 2, Text);
        }

        /// <inheritdoc />
        protected override void OnMouseEnter(object sender, MouseMoveEventArgs e)
        {
            base.OnMouseEnter(sender, e);

            BackColor = Color.Blue;
        }

        /// <inheritdoc />
        protected override void OnMouseLeave(object sender, MouseMoveEventArgs e)
        {
            base.OnMouseLeave(sender, e);

            BackColor = Color.White;
        }
    }
}

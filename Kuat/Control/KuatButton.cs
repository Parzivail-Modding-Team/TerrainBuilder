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
        public KuatButton()
        {
            BackColor = Color.White;
        }

        /// <inheritdoc />
        protected override void OnPaint(object sender, NvgContext e)
        {
            base.OnPaint(sender, e);

            e.FillColor(NanoVg.Rgba(BackColor));

            e.BeginPath();
            e.Rect(Location.X, Location.Y, Size.Width, Size.Height);
            e.Fill();
        }

        /// <inheritdoc />
        protected override void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseDown(sender, e);

            Console.WriteLine("Down");
        }

        /// <inheritdoc />
        protected override void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseUp(sender, e);

            Console.WriteLine("Up");
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

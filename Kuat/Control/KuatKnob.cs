using System;
using System.Drawing;
using NanoVGDotNet.NanoVG;
using OpenTK.Input;

namespace Kuat.Control
{
    public class KuatKnob : KuatControl
    {
        private float _maximum = 50;
        private float _minimum = -50;
        private bool _moving;
        private float _value;

        public float Minimum
        {
            get => _minimum;
            set
            {
                _minimum = value;
                CheckValuesAndInvalidate();
            }
        }

        public float Maximum
        {
            get => _maximum;
            set
            {
                _maximum = value;
                CheckValuesAndInvalidate();
            }
        }

        public float Value
        {
            get => _value;
            set
            {
                _value = value;
                CheckValuesAndInvalidate();
            }
        }

        /// <inheritdoc />
        public KuatKnob(string name) : base(name)
        {
            Size = new Size(32, 32);
        }

        public event EventHandler<EventArgs> ValueChanged;

        private void CheckValuesAndInvalidate()
        {
            if (_value > _maximum)
                _value = _maximum;
            if (_value < _minimum)
                _value = _minimum;

            OnValueChanged(this, EventArgs.Empty);
        }

        protected void OnValueChanged(object sender, EventArgs e)
        {
            ValueChanged?.Invoke(sender, e);
        }

        /// <inheritdoc />
        protected override void OnPaint(object sender, NvgContext e)
        {
            base.OnPaint(sender, e);

            var location = ClientLocation;
            var center = ClientLocation + new Size(Size.Width / 2, Size.Height / 2);

            var angle = KMath.Remap(Value, Minimum, Maximum, -120, 120);

            // draw inset background circle
            e.BeginPath();
            e.Circle(center.X, center.Y, Size.Width / 2f + 5);
            e.FillColor(NanoVg.Rgba(0xFF192025));
            e.Fill();

            // draw fill arc
            if (Value != Minimum)
            {
                e.BeginPath();
                e.Arc(center.X, center.Y, Size.Width / 2f + 4, -120 / 180f * KMath.PI - KMath.PI / 2,
                    angle / 180f * KMath.PI - KMath.PI / 2, NvgWinding.Clockwise);
                e.LineTo(center.X, center.Y);
                e.FillColor(NanoVg.Rgba(ForeColor));
                e.Fill();
            }

            // draw knob face
            e.BeginPath();
            e.Circle(center.X, center.Y, Size.Width / 2f);
            e.FillPaint(NanoVg.LinearGradient(e, location.X, location.Y, location.X, location.Y + Size.Height,
                NanoVg.Rgba(0xFF50595E), NanoVg.Rgba(0xFF2B3337)));
            e.Fill();

            // draw dot
            var posDot = center.OffsetByUnitVector(angle / 180f * KMath.PI - KMath.PI / 2, Size.Width / 2f - 4);
            e.BeginPath();
            e.Circle(posDot.X, posDot.Y, 1.5f);
            e.FillColor(NanoVg.Rgba(ForeColor));
            e.Fill();

            // draw outer rim
            e.BeginPath();
            e.Circle(center.X, center.Y, Size.Width / 2f + 5);
            e.StrokeColor(NanoVg.Rgba(0xFF192025));
            e.Stroke();

            // draw inner dark rim
            e.BeginPath();
            e.Circle(center.X, center.Y, Size.Width / 2f);
            e.StrokeColor(NanoVg.Rgba(0xFF192025));
            e.Stroke();

            // draw inner rim
            e.BeginPath();
            e.Circle(center.X, center.Y, Size.Width / 2f - 1);
            e.StrokePaint(NanoVg.LinearGradient(e, location.X, location.Y, location.X, location.Y + Size.Height,
                NanoVg.Rgba(0xFF6E757B), NanoVg.Rgba(0xFF323B40)));
            e.Stroke();

            e.FontFace(Font.Family);
            e.FontSize(Font.Size);
            e.TextAlign(NvgAlign.Center | NvgAlign.Middle);
            e.Text(ClientLocation.X + Size.Width / 2, ClientLocation.Y + Size.Height / 2, Text);
        }

        /// <inheritdoc />
        protected override void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseDown(sender, e);
            _moving = true;
        }

        /// <inheritdoc />
        protected override void OnMouseMoveAnywhere(object sender, MouseMoveEventArgs e)
        {
            if (_moving) Value += KMath.Remap(e.XDelta, 0, 1, 0, (Maximum - Minimum) / Math.Max(Size.Width * 3, 50));
        }

        /// <inheritdoc />
        protected override void OnMouseUpAnywhere(object sender, MouseButtonEventArgs e)
        {
            _moving = false;
        }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TerrainGen
{
    public partial class TwoDimensionalTrackbar : UserControl
    {
        private float _valueX = 50;
        private float _valueY = 50;
        private float _minX;
        private float _maxX = 100;
        private float _minY;
        private float _maxY = 100;

        [Description("Lower X Bound"), Category("Behavior"), DefaultValue(0), Browsable(true)]
        public float MinX
        {
            get => _minX;
            set
            {
                _minX = value;
                CheckValuesAndInvalidate();
            }
        }

        [Description("Upper X Bound"), Category("Behavior"), DefaultValue(100), Browsable(true)]
        public float MaxX
        {
            get => _maxX;
            set
            {
                _maxX = value;
                CheckValuesAndInvalidate();
            }
        }

        [Description("Lower Y Bound"), Category("Behavior"), DefaultValue(0), Browsable(true)]
        public float MinY
        {
            get => _minY;
            set
            {
                _minY = value;
                CheckValuesAndInvalidate();
            }
        }

        [Description("Upper Y Bound"), Category("Behavior"), DefaultValue(100), Browsable(true)]
        public float MaxY
        {
            get => _maxY;
            set
            {
                _maxY = value;
                CheckValuesAndInvalidate();
            }
        }

        [Description("X Value"), Category("Behavior"), DefaultValue(50), Browsable(true)]
        public float ValueX
        {
            get => _valueX;
            set
            {
                _valueX = value;
                CheckValuesAndInvalidate();
            }
        }

        [Description("Y Value"), Category("Behavior"), DefaultValue(50), Browsable(true)]
        public float ValueY
        {
            get => _valueY;
            set
            {
                _valueY = value;
                CheckValuesAndInvalidate();
            }
        }
        
        [Category("Action"), Description("Fires when the value is changed."), Browsable(true)]
        public event EventHandler ValueChanged;

        public TwoDimensionalTrackbar()
        {
            InitializeComponent();
        }

        private void CheckValuesAndInvalidate()
        {
            if (_valueX > _maxX)
                _valueX = _maxX;
            if (_valueX < _minX)
                _valueX = _minX;

            if (_valueY > _maxY)
                _valueY = _maxY;
            if (_valueY < _minY)
                _valueY = _minY;

            OnValueChanged();
            Invalidate(ClientRectangle);
        }

        /// <inheritdoc />
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

			var p = new Pen(ForeColor);
			var b = new SolidBrush(ForeColor);

            var posX = (ValueX - MinX) / (MaxX - MinX) * ClientRectangle.Width;
            e.Graphics.DrawLine(p, posX, ClientRectangle.Top, posX, ClientRectangle.Bottom);

            var posY = (ValueY - MinY) / (MaxY - MinY) * ClientRectangle.Height;
            e.Graphics.DrawLine(p, ClientRectangle.Left, posY, ClientRectangle.Right, posY);

            e.Graphics.FillEllipse(b, posX - 3, posY - 3, 6, 6);
        }

        /// <inheritdoc />
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if ((e.Button & MouseButtons.Left) == 0) return;
            SetValueFromMouse(e);
        }

        /// <inheritdoc />
        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if ((e.Button & MouseButtons.Left) == 0) return;
            SetValueFromMouse(e);
        }

        private void SetValueFromMouse(MouseEventArgs e)
        {
            ValueX = e.X / (float)ClientRectangle.Width * (MaxX - MinX) + MinX;
            ValueY = e.Y / (float)ClientRectangle.Height * (MaxY - MinY) + MinY;
            OnValueChanged();
        }

        protected virtual void OnValueChanged()
        {
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}

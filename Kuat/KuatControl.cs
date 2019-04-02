using System;
using System.Drawing;
using NanoVGDotNet.NanoVG;
using OpenTK;
using OpenTK.Input;

namespace Kuat
{
    public class KuatControl
    {
	    public EventHandler<MouseButtonEventArgs> Click;
	    public EventHandler<MouseButtonEventArgs> DoubleClick;
		public EventHandler<MouseButtonEventArgs> MouseDown;
		public EventHandler<MouseButtonEventArgs> MouseUp;
		public EventHandler<MouseWheelEventArgs> MouseWheel;
		public EventHandler<MouseEventArgs> MouseEnter;
	    public EventHandler<MouseEventArgs> MouseLeave;
	    public EventHandler<MouseEventArgs> MouseMove;
	    public EventHandler<EventArgs> Paint;
	    public EventHandler<EventArgs> TextChanged;
	    public EventHandler<KeyboardKeyEventArgs> KeyDown;
	    public EventHandler<KeyboardKeyEventArgs> KeyUp;
	    public EventHandler<KeyPressEventArgs> KeyPress;

        public Point Location { get; set; }
        public Size Size { get; set; }
        public string Name { get; set; }
        public string Text { get; set; }
        public Color ForeColor { get; set; }
        public Color BackColor { get; set; }
        public int ZIndex { get; set; }
        public int TabIndex { get; set; }
        public bool TabStop { get; set; }

		protected virtual void OnClick(object sender, MouseButtonEventArgs e)
		{
			Click?.Invoke(sender, e);
		}

		protected virtual void OnDoubleClick(object sender, MouseButtonEventArgs e)
		{
			DoubleClick?.Invoke(sender, e);
		}

		protected virtual void OnMouseDown(object sender, MouseButtonEventArgs e)
		{
			MouseDown?.Invoke(sender, e);
		}

		protected virtual void OnMouseUp(object sender, MouseButtonEventArgs e)
		{
			MouseUp?.Invoke(sender, e);
		}

		protected virtual void OnMouseWheel(object sender, MouseWheelEventArgs e)
		{
			MouseWheel?.Invoke(sender, e);
		}

		protected virtual void OnMouseEnter(object sender, MouseEventArgs e)
		{
			MouseEnter?.Invoke(sender, e);
		}

		protected virtual void OnMouseLeave(object sender, MouseEventArgs e)
		{
			MouseLeave?.Invoke(sender, e);
		}

		protected virtual void OnMouseMove(object sender, MouseEventArgs e)
		{
			MouseMove?.Invoke(sender, e);
		}

		protected virtual void OnPaint(object sender, EventArgs e)
		{
			Paint?.Invoke(sender, e);
		}

		protected virtual void OnTextChanged(object sender, EventArgs e)
		{
			TextChanged?.Invoke(sender, e);
		}

		protected virtual void OnKeyDown(object sender, KeyboardKeyEventArgs e)
		{
			KeyDown?.Invoke(sender, e);
		}

		protected virtual void OnKeyUp(object sender, KeyboardKeyEventArgs e)
		{
			KeyUp?.Invoke(sender, e);
		}

		protected virtual void OnKeyPress(object sender, KeyPressEventArgs e)
		{
			KeyPress?.Invoke(sender, e);
		}

	}
}
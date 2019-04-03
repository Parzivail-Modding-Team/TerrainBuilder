using System;
using System.Diagnostics.Contracts;
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
		public EventHandler<MouseMoveEventArgs> MouseEnter;
		public EventHandler<MouseMoveEventArgs> MouseLeave;
		public EventHandler<MouseMoveEventArgs> MouseMove;
		public EventHandler<NvgContext> Paint;
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
		public bool CanFocus { get; set; }
		public bool HasFocus { get; set; }
		public bool CanTabOut { get; set; }
        public bool Invalid { get; private set; }

		public Rectangle ClientRectangle => new Rectangle(Location, Size);

        public void Invalidate()
        {
            Invalid = true;
        }

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

		protected virtual void OnMouseEnter(object sender, MouseMoveEventArgs e)
		{
			MouseEnter?.Invoke(sender, e);
		}

		protected virtual void OnMouseLeave(object sender, MouseMoveEventArgs e)
		{
			MouseLeave?.Invoke(sender, e);
		}

		protected virtual void OnMouseMove(object sender, MouseMoveEventArgs e)
		{
			MouseMove?.Invoke(sender, e);
		}

		protected virtual void OnPaint(object sender, NvgContext e)
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

		internal void ProcessKeyboardEvents(object sender, KeyboardKeyEventArgs args)
		{
            Contract.Requires(args != null);
            if (args.Keyboard.IsKeyDown(args.Key))
                OnKeyDown(sender, args);
            else
                OnKeyUp(sender, args);
		}

		internal void ProcessKeyboardEvents(object sender, KeyPressEventArgs args)
		{
            OnKeyPress(sender, args);
		}

	    internal EventStage ProcessMouseEvents(object sender, MouseEventArgs args)
	    {
            var mousePos = args.Position;
	        switch (args)
	        {
	            case MouseButtonEventArgs buttonEventArgs:
                    if (ClientRectangle.Contains(mousePos))
                    {
                        if (buttonEventArgs.IsPressed)
                            OnMouseDown(sender, buttonEventArgs);
                        else
                            OnMouseUp(sender, buttonEventArgs);
                        return EventStage.Handled;
                    }
                    break;
	            case MouseMoveEventArgs moveEventArgs:
	                var prevMousePos = mousePos - new Size(moveEventArgs.XDelta, moveEventArgs.YDelta);
	                if (ClientRectangle.Contains(mousePos))
	                {
	                    if (ClientRectangle.Contains(prevMousePos))
	                        OnMouseMove(sender, moveEventArgs);
	                    else
	                        OnMouseEnter(sender, moveEventArgs);
	                }
	                else if (ClientRectangle.Contains(prevMousePos))
	                    OnMouseLeave(sender, moveEventArgs);
                    break;
	            case MouseWheelEventArgs wheelEventArgs:
                    if (HasFocus)
                        OnMouseWheel(sender, wheelEventArgs);
	                break;
	        }

            return EventStage.Pass;
        }

        public void Render(object sender, NvgContext context)
        {
            OnPaint(sender, context);
        }
    }
}
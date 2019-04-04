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
		public EventHandler<MouseClickEventArgs> Click;
		public EventHandler<MouseClickEventArgs> DoubleClick;
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

        private string _text;

        /// <summary>
        /// The location of the control
        /// </summary>
		public Point Location { get; set; }
        /// <summary>
        /// The size of the control
        /// </summary>
		public Size Size { get; set; }

        /// <summary>
        /// The name of the font loaded NanoVG
        /// </summary>
        public KuatFont Font { get; set; } = new KuatFont();
        /// <summary>
        /// The identifying name of the control
        /// </summary>
		public string Name { get; set; }
        /// <summary>
        /// The text associated with the control
        /// </summary>
        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                OnTextChanged(this, EventArgs.Empty);
            }
        }
        /// <summary>
        /// The foreground color of the control
        /// </summary>
        public Color ForeColor { get; set; } = Color.Black;
        /// <summary>
        /// The background color of the control
        /// </summary>
        public Color BackColor { get; set; } = Color.White;
        /// <summary>
        /// The z-index of the control
        /// </summary>
		public int ZIndex { get; set; }
        /// <summary>
        /// The tab index of the control
        /// </summary>
		public int TabIndex { get; set; }
        /// <summary>
        /// True if the control can be focused on by tabbing to it
        /// </summary>
		public bool TabStop { get; set; }
        /// <summary>
        /// True if the control can receive any focus
        /// </summary>
        public bool CanFocus { get; set; } = true;
        /// <summary>
        /// True if the control is currently focused
        /// </summary>
		public bool HasFocus { get; set; }
        /// <summary>
        /// True if a tab character inputted while the control is focused will blur this control and advance along the tab focus list
        /// </summary>
		public bool CanTabOut { get; set; } = true;
        /// <summary>
        /// True if the control is invalid and requires a re-render
        /// </summary>
        public bool Invalid { get; private set; }

        /// <summary>
        /// The rectangle that defines the render and picking bounds of the control
        /// </summary>
		public Rectangle ClientRectangle => new Rectangle(Location, Size);

        public KuatControl(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Invalidates the control
        /// </summary>
        public void Invalidate()
        {
            Invalid = true;
        }

        /// <summary>
        /// Raises the <see cref="Click"/> event
        /// </summary>
        /// <param name="sender">The object which initiates the event</param>
        /// <param name="e">The context of the event</param>
		protected virtual void OnClick(object sender, MouseClickEventArgs e)
		{
			Click?.Invoke(sender, e);
		}
        
        /// <summary>
        /// Raises the <see cref="DoubleClick"/> event
        /// </summary>
        /// <param name="sender">The object which initiates the event</param>
        /// <param name="e">The context of the event</param>
		protected virtual void OnDoubleClick(object sender, MouseClickEventArgs e)
		{
			DoubleClick?.Invoke(sender, e);
		}
        
        /// <summary>
        /// Raises the <see cref="MouseDown"/> event
        /// </summary>
        /// <param name="sender">The object which initiates the event</param>
        /// <param name="e">The context of the event</param>
		protected virtual void OnMouseDown(object sender, MouseButtonEventArgs e)
		{
			MouseDown?.Invoke(sender, e);
		}
        
        /// <summary>
        /// Raises the <see cref="MouseUp"/> event
        /// </summary>
        /// <param name="sender">The object which initiates the event</param>
        /// <param name="e">The context of the event</param>
		protected virtual void OnMouseUp(object sender, MouseButtonEventArgs e)
		{
			MouseUp?.Invoke(sender, e);
		}
        
        /// <summary>
        /// Raises the <see cref="MouseWheel"/> event
        /// </summary>
        /// <param name="sender">The object which initiates the event</param>
        /// <param name="e">The context of the event</param>
		protected virtual void OnMouseWheel(object sender, MouseWheelEventArgs e)
		{
			MouseWheel?.Invoke(sender, e);
		}
        
        /// <summary>
        /// Raises the <see cref="MouseEnter"/> event
        /// </summary>
        /// <param name="sender">The object which initiates the event</param>
        /// <param name="e">The context of the event</param>
		protected virtual void OnMouseEnter(object sender, MouseMoveEventArgs e)
		{
			MouseEnter?.Invoke(sender, e);
		}
        
        /// <summary>
        /// Raises the <see cref="MouseLeave"/> event
        /// </summary>
        /// <param name="sender">The object which initiates the event</param>
        /// <param name="e">The context of the event</param>
		protected virtual void OnMouseLeave(object sender, MouseMoveEventArgs e)
		{
			MouseLeave?.Invoke(sender, e);
		}
        
        /// <summary>
        /// Raises the <see cref="MouseMove"/> event
        /// </summary>
        /// <param name="sender">The object which initiates the event</param>
        /// <param name="e">The context of the event</param>
		protected virtual void OnMouseMove(object sender, MouseMoveEventArgs e)
		{
			MouseMove?.Invoke(sender, e);
		}
        
        /// <summary>
        /// Raises the <see cref="Paint"/> event
        /// </summary>
        /// <param name="sender">The object which initiates the event</param>
        /// <param name="e">The context of the event</param>
		protected virtual void OnPaint(object sender, NvgContext e)
		{
			Paint?.Invoke(sender, e);
		}
        
        /// <summary>
        /// Raises the <see cref="TextChanged"/> event
        /// </summary>
        /// <param name="sender">The object which initiates the event</param>
        /// <param name="e">The context of the event</param>
		protected virtual void OnTextChanged(object sender, EventArgs e)
		{
			TextChanged?.Invoke(sender, e);
		}
        
        /// <summary>
        /// Raises the <see cref="KeyDown"/> event
        /// </summary>
        /// <param name="sender">The object which initiates the event</param>
        /// <param name="e">The context of the event</param>
		protected virtual void OnKeyDown(object sender, KeyboardKeyEventArgs e)
		{
			KeyDown?.Invoke(sender, e);
		}
        
        /// <summary>
        /// Raises the <see cref="KeyUp"/> event
        /// </summary>
        /// <param name="sender">The object which initiates the event</param>
        /// <param name="e">The context of the event</param>
		protected virtual void OnKeyUp(object sender, KeyboardKeyEventArgs e)
		{
			KeyUp?.Invoke(sender, e);
		}
        
        /// <summary>
        /// Raises the <see cref="KeyPress"/> event
        /// </summary>
        /// <param name="sender">The object which initiates the event</param>
        /// <param name="e">The context of the event</param>
		protected virtual void OnKeyPress(object sender, KeyPressEventArgs e) 
		{
			KeyPress?.Invoke(sender, e);
		}
        
        /// <summary>
        /// Processed the keyboard state change events
        /// </summary>
        /// <param name="sender">The object which initiates the events</param>
        /// <param name="args">The context of the events</param>
		internal void ProcessKeyboardEvent(object sender, KeyboardKeyEventArgs args)
		{
            if (args.Keyboard.IsKeyDown(args.Key))
                OnKeyDown(sender, args);
            else
                OnKeyUp(sender, args);
		}
        
        /// <summary>
        /// Processed the keyboard key press events
        /// </summary>
        /// <param name="sender">The object which initiates the events</param>
        /// <param name="args">The context of the events</param>
		internal void ProcessKeyboardEvent(object sender, KeyPressEventArgs args)
		{
            OnKeyPress(sender, args);
		}
        
        /// <summary>
        /// Processed the mouse events
        /// </summary>
        /// <param name="sender">The object which initiates the events</param>
        /// <param name="args">The context of the events</param>
	    internal EventStage ProcessMouseEvent(object sender, MouseEventArgs args)
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
                case MouseClickEventArgs clickEventArgs:
                    if (clickEventArgs.DoubleClick)
                        OnDoubleClick(sender, clickEventArgs);
                    OnClick(sender, clickEventArgs);
                    break;
	            case MouseWheelEventArgs wheelEventArgs:
                    if (HasFocus)
                        OnMouseWheel(sender, wheelEventArgs);
	                break;
	        }

            return EventStage.Pass;
        }
        
        /// <summary>
        /// Processed the render event
        /// </summary>
        /// <param name="sender">The object which initiates the event</param>
        /// <param name="context">The graphics context of the render call</param>
        public void ProcessRenderEvent(object sender, NvgContext context)
        {
            OnPaint(sender, context);
        }
    }
}
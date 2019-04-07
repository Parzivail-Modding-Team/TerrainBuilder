using System;
using System.Drawing;
using System.Linq;
using NanoVGDotNet.NanoVG;
using OpenTK;
using OpenTK.Input;

namespace Kuat.Control
{
    public class KuatControl
    {
        private KuatFont _font;
        private bool _invalid;
        private string _text = string.Empty;

        public EventHandler<MouseClickEventArgs> Click;
        public EventHandler<MouseClickEventArgs> DoubleClick;
        public EventHandler<KeyboardKeyEventArgs> KeyDown;
        public EventHandler<KeyPressEventArgs> KeyPress;
        public EventHandler<KeyboardKeyEventArgs> KeyUp;
        public EventHandler<MouseButtonEventArgs> MouseDown;
        public EventHandler<MouseMoveEventArgs> MouseEnter;
        public EventHandler<MouseMoveEventArgs> MouseLeave;
        public EventHandler<MouseMoveEventArgs> MouseMove;
        public EventHandler<MouseButtonEventArgs> MouseUp;
        public EventHandler<MouseWheelEventArgs> MouseWheel;
        public EventHandler<NvgContext> Paint;
        public EventHandler<EventArgs> TextChanged;
        public EventHandler<EventArgs> Focus;
        public EventHandler<EventArgs> Blur;

        /// <summary>
        ///     True if the control is being hovered
        /// </summary>
        internal bool Hover { get; set; }

        /// <summary>
        ///     True if the control is active (i.e. being clicked on)
        /// </summary>
        internal bool Active { get; set; }

        /// <summary>
        ///     The location of the control relative to its parent
        /// </summary>
        public Point Location { get; set; }

        /// <summary>
        ///     The render location of the control
        /// </summary>
        public Point ClientLocation =>
            Location + (Parent == null ? Size.Empty : (Size) Parent.ClientLocation);

        /// <summary>
        ///     The size of the control
        /// </summary>
        public Size Size { get; set; }

        /// <summary>
        ///     The control which owns this control
        /// </summary>
        public KuatControl Parent { get; internal set; }

        /// <summary>
        ///     The name of the font loaded NanoVG
        /// </summary>
        public KuatFont Font
        {
            get => _font == null && Parent?.Font != null ? Parent.Font : _font;
            set => _font = value;
        }

        /// <summary>
        ///     The identifying name of the control
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     The text associated with the control
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
        ///     The foreground color of the control
        /// </summary>
        public Color ForeColor { get; set; } = Color.White;

        /// <summary>
        ///     The active color of the control
        /// </summary>
        public Color ActiveColor { get; set; } = Color.FromArgb(255, 124, 232, 255);

        /// <summary>
        ///     The z-index of the control
        /// </summary>
        public int ZIndex { get; set; }

        /// <summary>
        ///     The tab index of the control
        /// </summary>
        public int TabIndex { get; set; }

        /// <summary>
        ///     True if the control can be focused on by tabbing to it
        /// </summary>
        public bool TabStop { get; set; }

        /// <summary>
        ///     True if the control can receive any focus
        /// </summary>
        public bool CanFocus { get; set; } = true;

        /// <summary>
        ///     True if the control is currently focused
        /// </summary>
        public bool HasFocus { get; set; }

        /// <summary>
        ///     True if a tab character inputted while the control is focused will blur this control and advance along the tab
        ///     focus list
        /// </summary>
        public bool CanTabOut { get; set; } = true;

        /// <summary>
        ///     True if the control is invalid and requires a re-render
        /// </summary>
        public bool Invalid
        {
            get => _invalid || Controls.Any(control => control.Invalid);
            private set => _invalid = value;
        }

        /// <summary>
        ///     The list of children controls contained within the control
        /// </summary>
        public KuatControlCollection Controls { get; }

        /// <summary>
        ///     The rectangle that defines the render and picking bounds of the control
        /// </summary>
        public Rectangle ClientRectangle => new Rectangle(ClientLocation, Size);

        public KuatControl(string name)
        {
            Name = name;
            Controls = new KuatControlCollection(this);
        }

        /// <summary>
        ///     Invalidates the control
        /// </summary>
        public void Invalidate()
        {
            Invalid = true;
        }

        /// <summary>
        ///     Raises the <see cref="Click" /> event
        /// </summary>
        /// <param name="sender">The object which initiates the event</param>
        /// <param name="e">The context of the event</param>
        protected virtual void OnClick(object sender, MouseClickEventArgs e)
        {
            Click?.Invoke(sender, e);
        }

        /// <summary>
        ///     Raises the <see cref="DoubleClick" /> event
        /// </summary>
        /// <param name="sender">The object which initiates the event</param>
        /// <param name="e">The context of the event</param>
        protected virtual void OnDoubleClick(object sender, MouseClickEventArgs e)
        {
            DoubleClick?.Invoke(sender, e);
        }

        /// <summary>
        ///     Raises the <see cref="MouseDown" /> event
        /// </summary>
        /// <param name="sender">The object which initiates the event</param>
        /// <param name="e">The context of the event</param>
        protected virtual void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            MouseDown?.Invoke(sender, e);
            Active = true;
        }

        /// <summary>
        ///     Raises the <see cref="MouseUp" /> event
        /// </summary>
        /// <param name="sender">The object which initiates the event</param>
        /// <param name="e">The context of the event</param>
        protected virtual void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            MouseUp?.Invoke(sender, e);
            Active = false;
        }

        /// <summary>
        ///     Called when the mouse moves anywhere on screen. Useful for detecting when drag operations end. Does not raise an
        ///     event.
        /// </summary>
        /// <param name="sender">The object which initiates the event</param>
        /// <param name="e">The context of the event</param>
        protected virtual void OnMouseUpAnywhere(object sender, MouseButtonEventArgs e)
        {
        }

        /// <summary>
        ///     Raises the <see cref="MouseWheel" /> event
        /// </summary>
        /// <param name="sender">The object which initiates the event</param>
        /// <param name="e">The context of the event</param>
        protected virtual void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            MouseWheel?.Invoke(sender, e);
        }

        /// <summary>
        ///     Raises the <see cref="MouseEnter" /> event
        /// </summary>
        /// <param name="sender">The object which initiates the event</param>
        /// <param name="e">The context of the event</param>
        protected virtual void OnMouseEnter(object sender, MouseMoveEventArgs e)
        {
            MouseEnter?.Invoke(sender, e);
            Hover = true;
            if (e.Mouse.IsAnyButtonDown)
                Active = true;
        }

        /// <summary>
        ///     Raises the <see cref="MouseLeave" /> event
        /// </summary>
        /// <param name="sender">The object which initiates the event</param>
        /// <param name="e">The context of the event</param>
        protected virtual void OnMouseLeave(object sender, MouseMoveEventArgs e)
        {
            MouseLeave?.Invoke(sender, e);
            Hover = false;
            Active = false;
        }

        /// <summary>
        ///     Raises the <see cref="MouseMove" /> event
        /// </summary>
        /// <param name="sender">The object which initiates the event</param>
        /// <param name="e">The context of the event</param>
        protected virtual void OnMouseMove(object sender, MouseMoveEventArgs e)
        {
            MouseMove?.Invoke(sender, e);
        }

        /// <summary>
        ///     Called when the mouse moves anywhere on screen. Useful for detecting drag operation changes. Does not raise an
        ///     event.
        /// </summary>
        /// <param name="sender">The object which initiates the event</param>
        /// <param name="e">The context of the event</param>
        protected virtual void OnMouseMoveAnywhere(object sender, MouseMoveEventArgs e)
        {
        }

        /// <summary>
        ///     Raises the <see cref="Paint" /> event
        /// </summary>
        /// <param name="sender">The object which initiates the event</param>
        /// <param name="e">The context of the event</param>
        protected virtual void OnPaint(object sender, NvgContext e)
        {
            Paint?.Invoke(sender, e);
        }

        /// <summary>
        ///     Raises the <see cref="TextChanged" /> event
        /// </summary>
        /// <param name="sender">The object which initiates the event</param>
        /// <param name="e">The context of the event</param>
        protected virtual void OnTextChanged(object sender, EventArgs e)
        {
            TextChanged?.Invoke(sender, e);
        }

        /// <summary>
        ///     Raises the <see cref="KeyDown" /> event
        /// </summary>
        /// <param name="sender">The object which initiates the event</param>
        /// <param name="e">The context of the event</param>
        protected virtual void OnKeyDown(object sender, KeyboardKeyEventArgs e)
        {
            KeyDown?.Invoke(sender, e);
        }

        /// <summary>
        ///     Raises the <see cref="KeyUp" /> event
        /// </summary>
        /// <param name="sender">The object which initiates the event</param>
        /// <param name="e">The context of the event</param>
        protected virtual void OnKeyUp(object sender, KeyboardKeyEventArgs e)
        {
            KeyUp?.Invoke(sender, e);
        }

        /// <summary>
        ///     Raises the <see cref="KeyPress" /> event
        /// </summary>
        /// <param name="sender">The object which initiates the event</param>
        /// <param name="e">The context of the event</param>
        protected virtual void OnKeyPress(object sender, KeyPressEventArgs e)
        {
            KeyPress?.Invoke(sender, e);
        }

        /// <summary>
        ///     Raises the <see cref="Focus" /> event
        /// </summary>
        /// <param name="sender">The object which initiates the event</param>
        /// <param name="e">The context of the event</param>
        protected internal virtual void OnFocus(object sender, EventArgs e)
        {
            Focus?.Invoke(sender, e);
        }

        /// <summary>
        ///     Raises the <see cref="Blur" /> event
        /// </summary>
        /// <param name="sender">The object which initiates the event</param>
        /// <param name="e">The context of the event</param>
        protected internal virtual void OnBlur(object sender, EventArgs e)
        {
            Focus?.Invoke(sender, e);
        }

        /// <summary>
        ///     Processes the keyboard state change events
        /// </summary>
        /// <param name="sender">The object which initiates the events</param>
        /// <param name="args">The context of the events</param>
        internal void ProcessKeyboardEvent(object sender, KeyboardKeyEventArgs args)
        {
            foreach (var child in Controls) child.ProcessKeyboardEvent(sender, args);

            if (args.Keyboard.IsKeyDown(args.Key))
                OnKeyDown(sender, args);
            else
                OnKeyUp(sender, args);
        }

        /// <summary>
        ///     Processes the keyboard key press events
        /// </summary>
        /// <param name="sender">The object which initiates the events</param>
        /// <param name="args">The context of the events</param>
        internal void ProcessKeyboardEvent(object sender, KeyPressEventArgs args)
        {
            foreach (var child in Controls) child.ProcessKeyboardEvent(sender, args);

            OnKeyPress(sender, args);
        }

        /// <summary>
        ///     Processes the mouse events
        /// </summary>
        /// <param name="sender">The object which initiates the events</param>
        /// <param name="args">The context of the events</param>
        internal EventStage ProcessMouseEvent(object sender, MouseEventArgs args)
        {
            foreach (var child in Controls)
            {
                var childStage = child.ProcessMouseEvent(sender, args);
                if (childStage == EventStage.Handled)
                    return EventStage.Handled;
            }

            var mousePos = args.Position;
            switch (args)
            {
                case MouseButtonEventArgs buttonEventArgs:
                    if (!buttonEventArgs.IsPressed)
                        OnMouseUpAnywhere(sender, buttonEventArgs);

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
                    OnMouseMoveAnywhere(sender, moveEventArgs);

                    var prevMousePos = mousePos - new Size(moveEventArgs.XDelta, moveEventArgs.YDelta);
                    if (ClientRectangle.Contains(mousePos))
                    {
                        if (!ClientRectangle.Contains(prevMousePos))
                            OnMouseEnter(sender, moveEventArgs);
                        OnMouseMove(sender, moveEventArgs);
                    }
                    else if (ClientRectangle.Contains(prevMousePos))
                    {
                        OnMouseLeave(sender, moveEventArgs);
                    }

                    break;
                case MouseClickEventArgs clickEventArgs:
                    if (ClientRectangle.Contains(mousePos))
                    {
                        if (clickEventArgs.DoubleClick)
                            OnDoubleClick(sender, clickEventArgs);
                        OnClick(sender, clickEventArgs);
                        return EventStage.Handled;
                    }

                    break;
                case MouseWheelEventArgs wheelEventArgs:
                    if (ClientRectangle.Contains(mousePos) && HasFocus)
                    {
                        OnMouseWheel(sender, wheelEventArgs);
                        return EventStage.Handled;
                    }

                    break;
            }

            return EventStage.Pass;
        }

        /// <summary>
        ///     Processes the render event
        /// </summary>
        /// <param name="sender">The object which initiates the event</param>
        /// <param name="context">The graphics context of the render call</param>
        internal void ProcessRenderEvent(object sender, NvgContext context)
        {
            OnPaint(sender, context);

            foreach (var child in Controls) child.ProcessRenderEvent(sender, context);
        }
    }
}
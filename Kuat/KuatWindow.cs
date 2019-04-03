using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Linq;
using NanoVGDotNet.NanoVG;
using OpenTK;
using OpenTK.Input;

namespace Kuat
{
    public class KuatWindow
    {
        private int _tabIndex;
        private KuatControl _focusedControl;

        public ConcurrentBag<KuatControl> Controls { get; }
        public bool Invalid => Controls.Any(control => control.Invalid);

        public KuatWindow(INativeWindow window)
        {
            Controls = new ConcurrentBag<KuatControl>();
            SubscribeEvents(window);
        }

        private void SubscribeEvents(INativeWindow window)
        {
            window.MouseMove += WindowOnMouseEvent;
            window.MouseWheel += WindowOnMouseEvent;
            window.MouseDown += WindowOnMouseEvent;
            window.MouseUp += WindowOnMouseEvent;
            window.KeyDown += WindowOnKeyboardEvent;
            window.KeyUp += WindowOnKeyboardEvent;
            window.KeyPress += WindowOnKeyboardEvent;
        }

        public void Paint(object sender, NvgContext context)
        {
            foreach (var control in Controls) control.Render(sender, context);
        }

        private void WindowOnKeyboardEvent(object sender, KeyPressEventArgs args)
        {
            if (args.KeyChar == '\t' && (_focusedControl == null || _focusedControl.CanTabOut))
            {
                _tabIndex = (_tabIndex + 1) % Controls.Count;
                var tabbedControl = Controls.First(control => control.TabStop && control.TabIndex == _tabIndex);
                SetFocus(tabbedControl);
            }

            _focusedControl?.ProcessKeyboardEvents(sender, args);
        }

        private void WindowOnKeyboardEvent(object sender, KeyboardKeyEventArgs args)
        {
            _focusedControl?.ProcessKeyboardEvents(sender, args);
        }

        private void WindowOnMouseEvent(object sender, MouseEventArgs args)
        {
            var isFocusingEvent = args is MouseButtonEventArgs;
            var focused = false;
            foreach (var control in Controls.OrderByDescending(control => control.ZIndex))
            {
                var stage = control.ProcessMouseEvents(sender, args);
                if (stage != EventStage.Handled || !control.CanFocus) continue;
                SetFocus(control);
                focused = true;
                break;
            }
            if (!focused && isFocusingEvent)
                SetFocus(null);
        }

        private void SetFocus(KuatControl needle)
        {
            _focusedControl = needle;
            foreach (var control in Controls)
                control.HasFocus = control == needle;
        }
    }
}

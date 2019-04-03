using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Linq;
using OpenTK;
using OpenTK.Input;

namespace Kuat
{
	public class KuatWindow
	{
		private int _tabIndex = 0;
		private KuatControl _focusedControl = null;

		public ConcurrentBag<KuatControl> Controls { get; }
		
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

		private void WindowOnKeyboardEvent(object sender, KeyPressEventArgs args)
		{
			if (args.KeyChar == '\t' && _focusedControl?.CanTabOut)
			{
				_tabIndex = (_tabIndex + 1) % Controls.Count;
				var tabbedControl = Controls.First(control => control.TabStop && control.TabIndex == _tabIndex);
				SetFocus(tabbedControl);
			}

			foreach (var control in Controls)
			{
				if (!control.HasFocus) continue;
				control.ProcessKeyboardEvents(sender, args);
			}
		}

		private void WindowOnKeyboardEvent(object sender, KeyboardKeyEventArgs args)
		{
			foreach (var control in Controls)
			{
				if (!control.HasFocus) continue;
				control.ProcessKeyboardEvents(sender, args);
			}
		}

		private void WindowOnMouseEvent(object sender, MouseEventArgs args)
		{
		    var isFocusingEvent = args is MouseButtonEventArgs;
            var focused = false;
			foreach (var control in Controls)
			{
                control.ProcessMouseEvents(sender, args);
			    if (!isFocusingEvent || !control.CanFocus || !control.ClientRectangle.Contains(args.Position)) continue;
			    SetFocus(control);
			    focused = true;
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

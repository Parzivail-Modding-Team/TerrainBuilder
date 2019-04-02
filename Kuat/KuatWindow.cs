using System;
using System.Collections.Concurrent;
using System.Drawing;
using OpenTK;
using OpenTK.Input;

namespace Kuat
{
	public class KuatWindow
	{
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
			foreach (var control in Controls)
				control.HasFocus = control == needle;
		}
	}
}

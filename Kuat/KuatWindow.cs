using System;
using System.Collections.Concurrent;
using System.Drawing;
using OpenTK;
using OpenTK.Input;

namespace Kuat
{
	public class KuatWindow
	{
		private Point _prevMousePos;
		private Point _mousePos;

		public ConcurrentBag<KuatControl> Controls { get; }

		public KuatWindow(INativeWindow window)
		{
			Controls = new ConcurrentBag<KuatControl>();
			SubscribeEvents(window);
		}

		private void SubscribeEvents(INativeWindow window)
		{
			window.MouseMove += WindowOnMouseMove;
			window.MouseWheel += WindowOnMouseWheel;
			window.MouseDown += WindowOnMouseDown;
		}

		private void WindowOnMouseDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
		{
			foreach (var control in Controls)
			{
				if (control.ClientRectangle.Contains(mouseButtonEventArgs.Position))
				{
					control.MouseDown.Invoke(sender, mouseButtonEventArgs);
					SetFocus(control);
				}
			}
		}

		private void WindowOnMouseWheel(object sender, MouseWheelEventArgs mouseWheelEventArgs)
		{
			foreach (var control in Controls)
			{
				if (control.HasFocus)
					control.MouseWheel.Invoke(sender, mouseWheelEventArgs);
			}
		}

		private void WindowOnMouseMove(object sender, MouseMoveEventArgs mouseMoveEventArgs)
		{
			_prevMousePos = _mousePos;
			_mousePos = mouseMoveEventArgs.Position;
			foreach (var control in Controls)
			{
				if (control.ClientRectangle.Contains(_mousePos))
				{
					if (control.ClientRectangle.Contains(_prevMousePos))
						control.MouseMove.Invoke(sender, mouseMoveEventArgs);
					else
						control.MouseEnter.Invoke(sender, mouseMoveEventArgs);
				}
				else if (control.ClientRectangle.Contains(_prevMousePos))
					control.MouseLeave.Invoke(sender, mouseMoveEventArgs);
			}
		}

		private void SetFocus(KuatControl needle)
		{
			foreach (var control in Controls)
				control.HasFocus = control == needle;
		}
	}
}

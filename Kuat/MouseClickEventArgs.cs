using OpenTK.Input;

namespace Kuat
{
    public class MouseClickEventArgs : MouseEventArgs
    {
        public bool DoubleClick { get; }

        public MouseClickEventArgs(MouseEventArgs args, bool doubleClick) : base(args)
        {
            DoubleClick = doubleClick;
        }
    }
}
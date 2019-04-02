using NanoVGDotNet.NanoVG;

namespace Kuat
{
	public class PaintEventArgs
	{
		public NvgContext Graphics { get; private set; }

		public PaintEventArgs(NvgContext graphics)
		{
			Graphics = graphics;
		}
	}
}
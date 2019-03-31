using System;

namespace NanoVGDotNet.NanoVG
{
    [Flags]
    public enum NvgCreateFlags
    {
        None = 0,
        // Flag indicating if geometry based anti-aliasing is used (may not be needed when using MSAA).
        AntiAlias = 1,
        // Flag indicating if strokes should be drawn using stencil buffer. The rendering will be a little
        // slower, but path overlaps (i.e. self-intersecting or sharp turns) will be drawn just once.
        StencilStrokes = 2,
        // Flag indicating that additional debug checks are done.
        Debug = 4
    }
}
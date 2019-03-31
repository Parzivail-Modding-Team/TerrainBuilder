using System;

namespace NanoVGDotNet.NanoVG
{
    [Flags]
    public enum NvgAlign
    {
        // Default, align text horizontally to left.
        Left = 1,
        // Align text horizontally to center.
        Center = 2,
        // Align text horizontally to right.
        Right = 4,
        // Align text vertically to top.
        Top = 8,
        // Align text vertically to middle.
        Middle = 16,
        // Align text vertically to bottom.
        Bottom = 32,
        // Default, align text vertically to baseline.
        Baseline = 64
    }
}
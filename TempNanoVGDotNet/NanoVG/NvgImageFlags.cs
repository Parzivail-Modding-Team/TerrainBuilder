using System;

namespace NanoVGDotNet.NanoVG
{
    [Flags]
    public enum NvgImageFlags
    {
        // Generate mipmaps during creation of the image.
        GenerateMipmaps = 1,
        // Repeat image in X direction.
        RepeatX = 2,
        // Repeat image in Y direction.
        RepeatY = 4,
        // Flips (inverses) image in Y direction when rendered.
        FlipY = 8,
        // Image data has premultiplied alpha.
        Premultiplied = 16
    }
}
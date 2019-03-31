using System;

namespace NanoVGDotNet.NanoVG
{
    [Flags]
    public enum NvgBlendFactor
    {
        Zero = 1,
        One = 2,
        SrcColor = 4,
        OneMinusSrcColor = 8,
        DstColor = 16,
        OneMinusDstColor = 32,
        SrcAlpha = 64,
        OneMinusSrcAlpha = 128,
        DstAlpha = 256,
        OneMinusDstAlpha = 512,
        SrcAlphaSaturate = 1024
    }
}
namespace NanoVGDotNet.NanoVG
{
    public struct NvgGlyphPosition
    {
        public int Str;
        // Position of the glyph in the input string.
        public float X;
        // The x-coordinate of the logical glyph position.
        public float Minx, Maxx;
        // The bounds of the glyph shape.
    }
}
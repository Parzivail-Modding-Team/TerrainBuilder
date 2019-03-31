namespace NanoVGDotNet.NanoVG
{
    public struct NvgTextRow
    {
        public int Start;
        // Pointer to the input text where the row starts.
        public int End;
        // Pointer to the input text where the row ends (one past the last character).
        public int Next;
        // Pointer to the beginning of the next row.
        public float Width;
        // Logical width of the row.
        public float MinX, MaxX;
        // Actual bounds of the row. Logical with and bounds can differ because of kerning and some parts over extending.
    }
}
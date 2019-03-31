namespace NanoVGDotNet.NanoVG
{
    public delegate void RenderFillHandler(object uptr, ref NvgPaint paint, ref NvgScissor scissor, float fringe, float[] bounds, NvgPath[] paths, int npaths);
}
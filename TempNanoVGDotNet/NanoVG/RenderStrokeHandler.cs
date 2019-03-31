namespace NanoVGDotNet.NanoVG
{
    public delegate void RenderStrokeHandler(object uptr, ref NvgPaint paint, ref NvgScissor scissor, float fringe, float strokeWidth, NvgPath[] paths, int npaths);
}
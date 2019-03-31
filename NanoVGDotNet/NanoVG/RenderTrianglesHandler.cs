namespace NanoVGDotNet.NanoVG
{
    public delegate void RenderTrianglesHandler(object uptr, ref NvgPaint paint, ref NvgScissor scissor,
        NvgVertex[] verts, int nverts);
}
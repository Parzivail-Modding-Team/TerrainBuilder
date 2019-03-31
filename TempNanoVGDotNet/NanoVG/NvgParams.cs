namespace NanoVGDotNet.NanoVG
{
    public struct NvgParams
    {
        public object UserPtr;
        public int EdgeAntiAlias;
        public RenderCreateHandler RenderCreate;

        public RenderCreateTextureHandlerByte RenderCreateTextureByte;
        public RenderCreateTextureHandlerBmp RenderCreateTextureBmp;
        public RenderViewportHandler RenderViewport;
        public RenderFlushHandler RenderFlush;
        public RenderFillHandler RenderFill;
        public RenderStrokeHandler RenderStroke;
        public RenderTrianglesHandler RenderTriangles;
        public RenderUpdateTextureHandler RenderUpdateTexture;
        public RenderGetTextureSizeHandler RenderGetTextureSize;
        public RenderDeleteTexture RenderDeleteTexture;
        public RenderCancel RenderCancel;
        public RenderDelete RenderDelete;
    }
}
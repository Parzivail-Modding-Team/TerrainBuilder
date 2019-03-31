#define NANOVG_GL3_IMPLEMENTATION
#define NANOVG_GL_USE_STATE_FILTER

#if NANOVG_GL2_IMPLEMENTATION
#define NANOVG_GL2
#define NANOVG_GL_IMPLEMENTATION
#elif NANOVG_GL3_IMPLEMENTATION
#define NANOVG_GL3
#define NANOVG_GL_IMPLEMENTATION
//#define NANOVG_GL_USE_UNIFORMBUFFER
#endif

using OpenTK.Graphics.OpenGL;

namespace NanoVGDotNet.NanoVG
{
    public class GlNvgContext
    {
        public GlNvgShader Shader;
        public GlNvgTexture[] Textures;
        // [2]
        public float[] View;
        public int Ntextures;
        public int Ctextures;
        public int TextureId;
        public uint VertBuf;
#if NANOVG_GL3
        public uint VertArr;
#endif
#if NANOVG_GL_USE_UNIFORMBUFFER
		public uint FragBuf;
#endif
        public int FragSize;
        public int Flags;

        // Per frame buffers
        public GlNvgCall[] Calls;
        public int Ccalls;
        public int Ncalls;
        public GlNvgPath[] Paths;
        public int Cpaths;
        public int Npaths;
        public NvgVertex[] Verts;
        public int Cverts;
        public int Nverts;
        public GlNvgFragUniforms[] Uniforms;
        public int Cuniforms;
        public int Nuniforms;

        // cached state
#if NANOVG_GL_USE_STATE_FILTER
        public uint BoundTexture;
        public uint StencilMask;
        public StencilFunction StencilFunc;
        public int StencilFuncRef;
        public uint StencilFuncMask;
#endif

        public GlNvgContext()
        {
            View = new float[2];
        }
    }
}
using System;
using OpenTK.Graphics.OpenGL;

namespace TerrainGen.Graphics
{
    internal class Framebuffer
    {
        private readonly int _samples;
        public int FboId { get; set; }
        public int TextureId { get; set; }
        public int DepthId { get; set; }

        public Framebuffer(int samples)
        {
            _samples = samples;
            FboId = GL.GenFramebuffer();
            TextureId = GL.GenTexture();
            DepthId = GL.GenRenderbuffer();
        }

        public void Init(int width, int height)
        {
            Use();

            GL.BindTexture(TextureTarget.Texture2DMultisample, TextureId);
            GL.TexImage2DMultisample(TextureTargetMultisample.Texture2DMultisample, _samples, PixelInternalFormat.Rgba, width, height, true);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2DMultisample, TextureId, 0);

//            GL.BindTexture(TextureTarget.Texture2DMultisample, DepthId);
//            GL.TexImage2DMultisample(TextureTargetMultisample.Texture2DMultisample, _samples,
//                PixelInternalFormat.DepthComponent, width, height, true);
//            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment,
//                TextureTarget.Texture2DMultisample, DepthId, 0);

            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, DepthId);
            GL.RenderbufferStorageMultisample(RenderbufferTarget.Renderbuffer, _samples, RenderbufferStorage.Depth24Stencil8, width, height);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, DepthId);

            var status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (status != FramebufferErrorCode.FramebufferComplete)
                throw new ApplicationException($"Framebuffer status expected to be FramebufferComplete, instead was {status}");

            GL.BindTexture(TextureTarget.Texture2DMultisample, 0);
            Release();
        }

        public void Use()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FboId);
        }

        public void Release()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }
    }
}
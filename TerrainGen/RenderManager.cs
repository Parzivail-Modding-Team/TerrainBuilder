using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NanoVGDotNet.FontStash;
using NanoVGDotNet.NanoVG;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using TerrainGen.Generator;
using TerrainGen.Graphics;
using TerrainGen.Job;
using TerrainGen.Shader;
using TerrainGen.Util;
using TerrainGenCore;

namespace TerrainGen
{
    public class RenderManager
    {
        private readonly ConcurrentQueue<IJob> _fgJobs;
        private readonly ConcurrentQueue<IJob> _bgJobs;
        private readonly GameWindow _window;
        private readonly CsTerrainGenerator _generator;
        private readonly EventWaitHandle _workerHandle;

        private readonly NvgContext _nvg;
        private readonly PerfGraph _perfGraphFps;
        private readonly Framebuffer _framebuffer;
        private readonly Framebuffer _framebufferUi;
        private readonly int _texRandom;
        private readonly ShaderProgram _shaderScreen;
        private int _screenVao;
        private readonly ShaderProgram _shaderModel;
        private readonly ShaderUniform _uTint = new ShaderUniform("tint");
        private readonly ShaderUniform _uLightPos = new ShaderUniform("lightPos");
        private readonly ShaderUniform _uMatModel = new ShaderUniform("m");
        private readonly ShaderUniform _uMatView = new ShaderUniform("v");
        private readonly ShaderUniform _uMatProjection = new ShaderUniform("p");
        private readonly ShaderUniform _uWidth = new ShaderUniform("width");
        private readonly ShaderUniform _uHeight = new ShaderUniform("height");
        private readonly ShaderUniform _uTexColor = new ShaderUniform("screenColor");
        private readonly ShaderUniform _uTexUi = new ShaderUniform("screenUi");
        private readonly ShaderUniform _uSamples = new ShaderUniform("samples");
        private readonly ShaderUniform _uSamplesUi = new ShaderUniform("samplesUi");

        private Thread _worker;

        public Chunk[] Chunks { get; private set; }
        public Vector3 TintColor { get; set; }
        public Vector3 LightPosition { get; set; }
        public int SideLength { get; set; }

        public RenderManager(GameWindow window, CsTerrainGenerator generator)
        {
            _window = window;
            _generator = generator;

            _fgJobs = new ConcurrentQueue<IJob>();
            _bgJobs = new ConcurrentQueue<IJob>();

            _nvg = GlNanoVg.CreateGl(NvgCreateFlags.StencilStrokes);

            var rSans = _nvg.CreateFont("sans", EmbeddedFiles.ibmplexmono);
            if (rSans == -1)
                throw new ApplicationException("Unable to load UI font");

            _perfGraphFps = new PerfGraph(GraphRenderStyle.Fps, null, 40);

            _framebuffer = new Framebuffer(8);
            _framebuffer.Init(window.Width, window.Height);
            _framebufferUi = new Framebuffer(1);
            _framebufferUi.Init(window.Width, window.Height);
            _texRandom = LoadGlTexture(EmbeddedFiles.random);
            _shaderModel = new ShaderProgram(EmbeddedFiles.fs_model, EmbeddedFiles.vs_model);
            _shaderModel.Init();
            _shaderScreen = new ShaderProgram(EmbeddedFiles.fs_screen, EmbeddedFiles.vs_screen);
            _shaderScreen.Init();
            _workerHandle = new EventWaitHandle(false, EventResetMode.ManualReset);

            CreateChunks();
            CreateScreenVao();
        }

        private void CreateScreenVao()
        {
            float[] quadVertices = {
                // positions   // texCoords
                -1.0f,  1.0f,  0.0f, 1.0f,
                -1.0f, -1.0f,  0.0f, 0.0f,
                1.0f, -1.0f,  1.0f, 0.0f,

                -1.0f,  1.0f,  0.0f, 1.0f,
                1.0f, -1.0f,  1.0f, 0.0f,
                1.0f,  1.0f,  1.0f, 1.0f
            };

            _screenVao = GL.GenVertexArray();
            var screenVbo = GL.GenBuffer();
            GL.BindVertexArray(_screenVao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, screenVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, quadVertices.Length * sizeof(float), quadVertices, BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
            GL.BufferData(BufferTarget.ArrayBuffer, quadVertices.Length * sizeof(float), quadVertices, BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

        public void CreateChunks()
        {
            Chunks = new Chunk[SideLength * SideLength];
            for (var i = 0; i < SideLength; i++)
                for (var j = 0; j < SideLength; j++)
                {
                    Chunks[i * SideLength + j] = new Chunk(i, j);
                }
        }

        public void EnqueueJob(IJob job)
        {
            if (job.CanExecuteInBackground())
            {
                _bgJobs.Enqueue(job);
                _workerHandle.Set();
            }
            else
                _fgJobs.Enqueue(job);
        }

        public void Update()
        {
            if (_worker is null)
            {
                _worker = new Thread(() =>
                {
                    while (true)
                    {
                        while (!_bgJobs.IsEmpty)
                        {
                            if (!_bgJobs.TryDequeue(out var job))
                            {
                                Lumberjack.Warn("Unable to dequeue BG job");
                                return;
                            }
                            job.Execute(this);
                        }

                        _workerHandle.Reset();
                        _workerHandle.WaitOne();
                    }
                });
                _worker.Start();
            }

            while (!_fgJobs.IsEmpty)
            {
                if (!_fgJobs.TryDequeue(out var job))
                {
                    Lumberjack.Warn("Unable to dequeue FG job");
                    return;
                }
                job.Execute(this);
                Application.DoEvents();
            }

            _perfGraphFps.UpdateGraph((float)_window.RenderTime);
        }

        public void Render(Matrix4 model, Matrix4 view, Matrix4 projection)
        {
            _framebuffer.Use();
            GL.PushMatrix();

            GL.Clear(ClearBufferMask.ColorBufferBit |
                     ClearBufferMask.DepthBufferBit |
                     ClearBufferMask.StencilBufferBit);

            GL.Color3(Color.White);

            // Set up uniforms
            _uTint.Value = TintColor;
            _uLightPos.Value = LightPosition;
            _uMatModel.Value = model;
            _uMatView.Value = view;
            _uMatProjection.Value = projection;
	        _uSamples.Value = _framebuffer.Samples;

			// Engage shader, render, disengage
			_shaderModel.Use(_uTint, _uLightPos, _uMatModel, _uMatView, _uMatProjection, _uSamples);

            foreach (var chunk in Chunks)
                chunk?.Draw();

            _shaderScreen.Release();

            // Render the ocean
            GL.Color3(Color.MediumBlue);

            var waterLevel = _generator.GetWaterLevel();

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref projection);
            GL.MatrixMode(MatrixMode.Modelview);
            var mat = view * model;
            GL.LoadMatrix(ref mat);

            GL.Begin(PrimitiveType.Quads);
            GL.Normal3(Vector3.UnitY);
            GL.Vertex3(-1, waterLevel - 0.4, -1);
            GL.Vertex3(SideLength * 16 - 1, waterLevel - 0.4, -1);
            GL.Vertex3(SideLength * 16 - 1, waterLevel - 0.4, SideLength * 16 - 1);
            GL.Vertex3(-1, waterLevel - 0.4, SideLength * 16 - 1);
            GL.End();

            GL.PopMatrix();
            _framebuffer.Release();

            _framebufferUi.Use();
            GL.ClearColor(0, 0, 0, 0);
            GL.Clear(ClearBufferMask.ColorBufferBit |
                     ClearBufferMask.DepthBufferBit |
                     ClearBufferMask.StencilBufferBit);

            _nvg.BeginFrame(_window.Width, _window.Height, 1);
            _nvg.Save();

            _nvg.FillColor(NanoVg.Rgba(255, 255, 255, 255));
            _nvg.FontFace("sans");
            _nvg.FontSize(18);
            _nvg.TextAlign(NvgAlign.Top | NvgAlign.Left);

            _perfGraphFps.RenderGraph(_nvg, 4, 4);

            _nvg.Restore();
            _nvg.EndFrame();
            _framebufferUi.Release();

            _uWidth.Value = _window.Width;
            _uHeight.Value = _window.Height;
            _uTexColor.Value = 0;
            _uTexUi.Value = 1;
	        _uSamples.Value = _framebuffer.Samples;
	        _uSamplesUi.Value = _framebufferUi.Samples;

			_shaderScreen.Use(_uWidth, _uHeight, _uTexColor, _uTexUi, _uSamples, _uSamplesUi);
            DrawFullscreenQuad(_framebuffer.TextureId, _framebufferUi.TextureId);
            _shaderScreen.Release();
        }

        private void DrawFullscreenQuad(int colorTexture, int extTexture1 = -1)
        {
            GL.BindVertexArray(_screenVao);
            GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Texture2D);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2DMultisample, colorTexture);
            if (extTexture1 != -1)
            {
                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2DMultisample, extTexture1);
            }
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
            GL.Disable(EnableCap.Texture2D);
            GL.Enable(EnableCap.DepthTest);
        }

        public void Rebuild()
        {
            CancelJobs();
            EnqueueJob(new JobRebuildChunks(_generator));
        }

        public void CancelJobs()
        {
            var fgJobs = new List<IJob>();
            var bgJobs = new List<IJob>();

            while (!_fgJobs.IsEmpty)
            {
                if (!_fgJobs.TryDequeue(out var j))
                    continue;
                fgJobs.Add(j);
            }

            while (!_bgJobs.IsEmpty)
            {
                if (!_bgJobs.TryDequeue(out var j))
                    continue;
                bgJobs.Add(j);
            }

            foreach (var job in fgJobs)
                _fgJobs.Enqueue(job);

            foreach (var job in bgJobs)
                _bgJobs.Enqueue(job);
        }

        public void SetSeed(long seed)
        {
            ProcNoise.SetSeed(seed);
        }

        public void Kill()
        {
            _worker.Abort();
        }

        public void OnResize()
        {
            _framebuffer.Init(_window.Width, _window.Height);
            _framebufferUi.Init(_window.Width, _window.Height);
        }

        private static int LoadGlTexture(Bitmap bitmap)
        {
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

            GL.GenTextures(1, out int tex);
            GL.BindTexture(TextureTarget.Texture2D, tex);

            var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
            bitmap.UnlockBits(data);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            return tex;
        }

	    public void SetSamples(int samples)
	    {
		    _framebuffer.Samples = samples;
	    }
    }
}

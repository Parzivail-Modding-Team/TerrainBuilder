using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using TerrainGen.Generator;
using TerrainGen.Job;
using TerrainGen.Shader;
using TerrainGen.Util;

namespace TerrainGen
{
    public class MainWindow : GameWindow
    {
        private bool _shouldDie;
        private KeyboardState _keyboard;
        private RenderController _terrainLayerList;
        private ScriptWatcher _scriptWatcher;
        private CsTerrainGenerator _terrainGenerator;
        private RenderManager _renderManager;

        private float _zoom = 1;
        private float _prevZoom = 1;
        private Vector2 _translation = new Vector2(0, -25);
        private Vector2 _prevTranslation = new Vector2(0, -25);
        private Vector2 _rotation = new Vector2(160, 45);
        private Vector2 _prevRotation = new Vector2(160, 45);

        private double _updateTimeAccumulator = 0;

        public MainWindow() : base(960, 540, new OpenTK.Graphics.GraphicsMode(32, 24, 0, 8))
        {
            Load += OnLoad;
            Closing += OnClose;
            Resize += OnResize;
            UpdateFrame += OnUpdate;
            RenderFrame += OnRender;
            MouseWheel += OnMouseWheel;
            MouseMove += OnMouseMove;
        }

        private void OnLoad(object sender, EventArgs e)
        {
            Title = $"{EmbeddedFiles.AppName} | {EmbeddedFiles.Title_Unsaved}";
            Icon = EmbeddedFiles.logo;

            // Set up lighting
            GL.ShadeModel(ShadingModel.Smooth);
            GL.Enable(EnableCap.ColorMaterial);

            // Set up caps
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.RescaleNormal);
            GL.Disable(EnableCap.Texture2D);
            GL.Disable(EnableCap.CullFace);

            // Set up blending
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            // Set background color
            GL.ClearColor(Color.FromArgb(255, 13, 13, 13));

            // Init keyboard to ensure first frame won't NPE
            _keyboard = Keyboard.GetState();

            _terrainGenerator = new CsTerrainGenerator();
            _renderManager = new RenderManager(this, _terrainGenerator);

            _terrainLayerList = new RenderController(this);
            _terrainLayerList.Show();

            _scriptWatcher = new ScriptWatcher();
            _scriptWatcher.FileChanged += OnScriptChanged;

            Lumberjack.Info(EmbeddedFiles.Info_WindowLoaded);
        }

        private void OnClose(object sender, CancelEventArgs e)
        {
            if (!_shouldDie)
                _terrainLayerList?.Close();
            _renderManager.Kill();
        }

        private void OnResize(object sender, EventArgs e)
        {
            GL.Viewport(ClientRectangle);
            _renderManager.OnResize();
        }

        private void OnUpdate(object sender, FrameEventArgs e)
        {
            if (_shouldDie)
                Exit();

            // Grab the new keyboard state
            _keyboard = Keyboard.GetState();

            // Compute input-based rotations
            var delta = (float)e.Time;
            var amount = _keyboard[Key.LShift] || _keyboard[Key.RShift] ? 45 : 90;

            _prevZoom = _zoom;
            _prevTranslation = new Vector2(_translation.X, _translation.Y);
            _prevRotation = new Vector2(_rotation.X, _rotation.Y);

            if (Focused)
            {
                if (_keyboard[Key.Left])
                    _rotation.Y += amount * delta;
                if (_keyboard[Key.Right])
                    _rotation.Y -= amount * delta;
                if (_keyboard[Key.Up])
                    _rotation.X += amount * delta;
                if (_keyboard[Key.Down])
                    _rotation.X -= amount * delta;
                if (_keyboard[Key.R])
                {
                    _rotation.Y = 45;
                    _rotation.X = 160;
                    _translation = new Vector2(0, -25);
                }
            }

            _renderManager.Update();

            _updateTimeAccumulator = 0;
        }

        private void OnRender(object sender, FrameEventArgs e)
        {
            // Reset the view
            GL.Clear(ClearBufferMask.ColorBufferBit |
                     ClearBufferMask.DepthBufferBit |
                     ClearBufferMask.StencilBufferBit);

            _updateTimeAccumulator += e.Time;
            var partialTicks = _updateTimeAccumulator / TargetUpdatePeriod;

            // Reload the projection matrix
            var aspectRatio = Width / (float)Height;
            var zoom = (float)(_prevZoom + (_zoom - _prevZoom) * partialTicks);
            var scale = new Vector3(4 * (1 / zoom), -4 * (1 / zoom), 4 * (1 / zoom));
            var rotX = _prevRotation.X + (_rotation.X - _prevRotation.X) * partialTicks;
            var rotY = _prevRotation.Y + (_rotation.Y - _prevRotation.Y) * partialTicks;
            var transX = _prevTranslation.X + (_translation.X - _prevTranslation.X) * partialTicks;
            var transY = _prevTranslation.Y + (_translation.Y - _prevTranslation.Y) * partialTicks;

            var mProjection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspectRatio, 1, 1024);
            var mModel = Matrix4.LookAt(0, 128, 256, 0, 0, 0, 0, 1, 0);
            var mTranslate = Matrix4.CreateTranslation((float)transX, (float)transY, 0);
            var mScale = Matrix4.CreateScale(scale);
            var mRotX = Matrix4.CreateRotationX((float)(rotX / 180 * Math.PI));
            var mRotY = Matrix4.CreateRotationY((float)(rotY / 180 * Math.PI));
            var mLocalTranslate = Matrix4.CreateTranslation(-_renderManager.SideLength * 8 + 1, 0, -_renderManager.SideLength * 8 + 1);
            var mView = mLocalTranslate * mRotY * mRotX * mScale * mTranslate;

            _renderManager.Render(mModel, mView, mProjection);

            // Swap the graphics buffer
            SwapBuffers();

            var err = GL.GetError();
            if (err != ErrorCode.NoError)
                Lumberjack.Error(err.ToString());
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            _zoom -= e.DeltaPrecise / 4f;

            if (_zoom < 0.5f)
                _zoom = 0.5f;
            if (_zoom > 35)
                _zoom = 35;
        }

        private void OnMouseMove(object sender, MouseMoveEventArgs e)
        {
            if (!e.Mouse.IsButtonDown(MouseButton.Left)) return;

            if (_keyboard[Key.ShiftLeft])
            {
                _translation.X += e.XDelta / 2f;
                _translation.Y -= e.YDelta / 2f;
            }
            else
            {
                _rotation.X -= e.YDelta / 2f;
                _rotation.Y -= e.XDelta / 2f;
            }
        }

        public void Kill()
        {
            _shouldDie = true;
        }

        private void OnScriptChanged(object sender, ScriptChangedEventArgs e)
        {
            Lumberjack.Info(string.Format(EmbeddedFiles.Info_FileReloaded, e.Filename));
            var success = _terrainGenerator.LoadScript(e.ScriptCode);
            if (!success)
                return;

            _renderManager.Rebuild();
        }

        public void WatchTerrainScript(string filename)
        {
            _scriptWatcher.WatchTerrainScript(filename);
        }

        public void EnqueueJob(IJob job)
        {
            _renderManager.EnqueueJob(job);
        }

        public void RebuildChunks()
        {
            _renderManager.Rebuild();
        }

        public void CancelJobs()
        {
            _renderManager.CancelJobs();
        }
    }
}

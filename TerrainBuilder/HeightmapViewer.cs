using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using TerrainBuilder.RenderUtil;

namespace TerrainBuilder
{
    public partial class HeightmapViewer : Form
    {
        public CsTerrainGenerator ScriptedTerrainGenerator = new CsTerrainGenerator();
        private readonly ScriptWatcher _scriptWatcher = new ScriptWatcher();
        private readonly BackgroundWorker _backgroundRenderer = new BackgroundWorker();

        public Dictionary<int, Color> Colors = new Dictionary<int, Color>();

        private readonly Random _random = new Random();

        public HeightmapViewer()
        {
            InitializeComponent();
        }

        private void bCreate_Click(object sender, EventArgs e)
        {
            var sfd = new SaveFileDialog { Filter = "C# Files|*.cs" };

            if (sfd.ShowDialog() == DialogResult.Cancel) return;

            File.WriteAllText(sfd.FileName, EmbeddedFiles.terrain);
            Process.Start(sfd.FileName);
            WatchTerrainScript(sfd.FileName);
        }

        private void bOpen_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog { Filter = "C# Files|*.cs" };

            if (ofd.ShowDialog() == DialogResult.Cancel) return;

            WatchTerrainScript(ofd.FileName);
        }

        private void bRandomize_Click(object sender, EventArgs e)
        {
            nudSeed.Value = _random.Next();
        }

        private void nudSeed_ValueChanged(object sender, EventArgs e)
        {
            ScriptedTerrainGenerator.SetSeed((long)nudSeed.Value);
            ReRenderNoiseImage();
        }

        private void nudSideLength_ValueChanged(object sender, EventArgs e)
        {
            ReRenderNoiseImage();
        }

        private void WatchTerrainScript(string filename)
        {
            _scriptWatcher.WatchTerrainScript(filename);
        }

        private void ReRenderNoiseImage()
        {
            if (Colors.Count == 0)
                return;

            try
            {
                var bmp = new Bitmap((int)nudSideLength.Value, (int)nudSideLength.Value);

                // If there's an ongoing render, cancel it
                if (IsRendering())
                    CancelRender();

                // Enable the render statusbar
                Invoke((MethodInvoker)delegate
               {
                   bCancelRender.Enabled = true;
                   bCancelRender.Visible = true;

                   pbRenderStatus.Visible = true;
               });

                // Fire up the render
                _backgroundRenderer.RunWorkerAsync(bmp);
            }
            catch (Exception ex)
            {
                Lumberjack.Error(ex.Message);
            }
        }

        public bool IsRendering()
        {
            return _backgroundRenderer.IsBusy;
        }

        public void CancelRender()
        {
            Lumberjack.Warn(EmbeddedFiles.Info_CancellingPreviousRenderOp);
            _backgroundRenderer.CancelAsync();

            while (IsRendering())
                Application.DoEvents();
        }

        private void DoBackgroundRenderProgress(object sender, ProgressChangedEventArgs progressChangedEventArgs)
        {
            // Render thread says something
            Invoke((MethodInvoker)delegate
            {
                // Invoke changes on form thread
                pbRenderStatus.Value = progressChangedEventArgs.ProgressPercentage;
                if (progressChangedEventArgs.UserState is string s)
                    lRenderStatus.Text = s;
            });
        }

        private void DoBackgroundRenderComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            // Render done, reset statusbar 
            Invoke((MethodInvoker)delegate
            {
                bCancelRender.Visible = false;
                bCancelRender.Enabled = false;

                pbRenderStatus.Visible = false;
                pbRenderStatus.Value = 0;

                lRenderStatus.Text = EmbeddedFiles.Status_Ready;
            });

            // If the render was manually cancelled, go no further
            if (_scriptWatcher.GetScriptId() == 0 || e.Cancelled)
                return;

            // Take the render result and upload it to the VBO
            pbHeightmap.Image = (Bitmap)e.Result;
            GC.Collect();

            // Wait for render thread to exit
            while (IsRendering())
                Application.DoEvents();
        }

        private void DoBackgroundRender(object sender, DoWorkEventArgs e)
        {
            try
            {
                // Make sure the render requirements are met
                if (_scriptWatcher.GetScriptId() == 0)
                {
                    Lumberjack.Warn("Can't render, no terrain loaded.");
                    return;
                }

                // Grab worker and report progress
                var worker = (BackgroundWorker)sender;
                var bitmap = (Bitmap)e.Argument;
                var s = (int)(nudSideLength.Value / 2);
                worker.ReportProgress(0, EmbeddedFiles.Status_GenHeightmap);

                var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                var data = bitmap.LockBits(rect, ImageLockMode.ReadWrite, bitmap.PixelFormat);
                var depth = Image.GetPixelFormatSize(data.PixelFormat) / 8; //bytes per pixel

                var buffer = new byte[data.Width * data.Height * depth];
                Marshal.Copy(data.Scan0, buffer, 0, buffer.Length);

                var w = bitmap.Width;
                var h = bitmap.Height;
                var done = 0;
                var r = Parallel.For(0, bitmap.Width, (x, state) =>
                {
                    for (var y = 0; y < h; y++)
                    {
                        // Cancel if requested
                        if (worker.CancellationPending)
                        {
                            e.Cancel = true;
                            state.Stop();
                        }

                        var n = ScriptedTerrainGenerator.GetValue(x - s, y - s);
                        var v = (int) n;
                        if (v > 255)
                            v = 255;
                        if (v < 0)
                            v = 0;

                        var offset = (y * w + x) * depth;
                        buffer[offset] = (byte) v;
                        buffer[offset + 1] = (byte) v;
                        buffer[offset + 2] = (byte) v;
                        buffer[offset + 3] = 255;
                    }

                    done++;
                    worker.ReportProgress((int) (done / (float) w * 100));
                });

                Marshal.Copy(buffer, 0, data.Scan0, buffer.Length);
                bitmap.UnlockBits(data);

                // Send the result back to the worker
                e.Result = bitmap;
            }
            catch (Exception ex)
            {
                Lumberjack.Error(ex.Message);
                e.Result = (Bitmap)e.Argument;
            }
        }

        private void HeightmapViewer_Load(object sender, EventArgs e)
        {
            nudSeed.Minimum = 0;
            nudSeed.Maximum = int.MaxValue;
            nudSeed.Value = _random.Next();

            Text = EmbeddedFiles.AppName;
            Icon = EmbeddedFiles.logo;

            for (var i = 0; i < 256; i++)
                Colors.Add(i, Color.FromArgb(i, i, i));

            _scriptWatcher.FileChanged += ScriptWatcherOnFileChanged;

            // Wire up background worker
            _backgroundRenderer.WorkerReportsProgress = true;
            _backgroundRenderer.WorkerSupportsCancellation = true;
            _backgroundRenderer.DoWork += DoBackgroundRender;
            _backgroundRenderer.ProgressChanged += DoBackgroundRenderProgress;
            _backgroundRenderer.RunWorkerCompleted += DoBackgroundRenderComplete;
        }

        private void ScriptWatcherOnFileChanged(object sender, ScriptChangedEventArgs e)
        {
            Lumberjack.Info(string.Format(EmbeddedFiles.Info_FileReloaded, e.Filename));
            if (ScriptedTerrainGenerator.LoadScript(e.ScriptCode))
                ReRenderNoiseImage();
        }

        private void bCancelRender_ButtonClick(object sender, EventArgs e)
        {
            CancelRender();
        }
    }
}

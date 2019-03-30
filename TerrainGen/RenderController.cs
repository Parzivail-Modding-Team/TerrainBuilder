using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using OpenTK;
using TerrainGen.Job;

namespace TerrainGen
{
    public partial class RenderController : Form
    {
        private readonly MainWindow _parent;
        private readonly Random _random = new Random();

        public RenderController(MainWindow parent)
        {
            _parent = parent;
            InitializeComponent();
        }

        private void RenderController_Load(object sender, EventArgs e)
        {
            nudSeed.Maximum = long.MaxValue;
            nudSeed.Value = _random.Next();
            _parent.EnqueueJob(new JobSetSeed((long)nudSeed.Value));
            _parent.EnqueueJob(new JobSetSideLength((int)nudSideLength.Value));
            _parent.EnqueueJob(new JobSetLightPosition(new Vector3(tdtLightPos.ValueX * 2200, 1000, tdtLightPos.ValueY * 2200)));
            SetTintColor(Color.LimeGreen);

            nudSeed.MouseWheel += delegate(object o, MouseEventArgs args) {
                ((HandledMouseEventArgs)args).Handled = true;
            };
            nudSideLength.MouseWheel += delegate(object o, MouseEventArgs args) {
                ((HandledMouseEventArgs)args).Handled = true;
            };

            tdtLightPos.ValueChanged += TdtLightPosOnValueChanged;
        }

        private void TdtLightPosOnValueChanged(object sender, EventArgs eventArgs)
        {
            _parent.EnqueueJob(new JobSetLightPosition(new Vector3(tdtLightPos.ValueX * 2200, 1000, tdtLightPos.ValueY * 2200)));
        }

        private void SetTintColor(Color c)
        {
            pbTerrainColor.BackColor = c;
            _parent.EnqueueJob(new JobSetTintColor(new Vector3(c.R / 255f, c.G / 255f, c.B / 255f)));
        }

        private void bRandomize_Click(object sender, EventArgs e)
        {
            nudSeed.Value = _random.Next();
        }

        private void TerrainLayerList_FormClosing(object sender, FormClosingEventArgs e)
        {
            _parent.Kill();
        }

        private void nudSideLength_ValueChanged(object sender, EventArgs e)
        {
            _parent.EnqueueJob(new JobSetSideLength((int) nudSideLength.Value));
            _parent.RebuildChunks();
        }

        private void nudSeed_ValueChanged(object sender, EventArgs e)
        {
            _parent.EnqueueJob(new JobSetSeed((long)nudSeed.Value));
            _parent.RebuildChunks();
        }

        private void bCreateTerrain_Click(object sender, EventArgs e)
        {
            var sfd = new SaveFileDialog { Filter = "C# Files|*.cs" };

            if (sfd.ShowDialog() == DialogResult.Cancel) return;

            File.WriteAllText(sfd.FileName, EmbeddedFiles.terrain);
            Process.Start(sfd.FileName);
            _parent.WatchTerrainScript(sfd.FileName);
            _parent.Title = $"{EmbeddedFiles.AppName} | {sfd.FileName}";
        }

        private void bOpenTerrain_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog { Filter = "C# Files|*.cs" };

            if (ofd.ShowDialog() == DialogResult.Cancel) return;

            _parent.WatchTerrainScript(ofd.FileName);
            _parent.Title = $"{EmbeddedFiles.AppName} | {ofd.FileName}";
        }

        private void pbMinColor_Click(object sender, EventArgs e)
        {
            if (colorPicker.ShowDialog() != DialogResult.OK)
                return;
            SetTintColor(colorPicker.Color);
        }
    }
}

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
            _parent.EnqueueJob(new JobSetSeed((long)nudSeed.Value));
            _parent.EnqueueJob(new JobSetSideLength((int)nudSideLength.Value));
            SetTintColor(Color.LimeGreen);
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

        private void bCancelGen_Click(object sender, EventArgs e)
        {
        }

        private void bManuallyGenerate_Click(object sender, EventArgs e)
        {
        }

        private void cbPauseGen_CheckedChanged(object sender, EventArgs e)
        {
        }

        private void pbMinColor_Click(object sender, EventArgs e)
        {
            if (colorPicker.ShowDialog() != DialogResult.OK)
                return;
            SetTintColor(colorPicker.Color);
        }

        private void heightmapViewerToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void cbVoxels_CheckedChanged(object sender, EventArgs e)
        {
        }
    }
}

using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace TerrainGen
{
    public partial class RenderController : Form
    {
        private readonly MainWindow _parent;

        public RenderController(MainWindow parent)
        {
            _parent = parent;
            InitializeComponent();
        }

        private void RenderController_Load(object sender, EventArgs e)
        {
        }

        private void bRandomize_Click(object sender, EventArgs e)
        {
        }

        private void TerrainLayerList_FormClosing(object sender, FormClosingEventArgs e)
        {
            _parent.Kill();
        }

        private void nudSideLength_ValueChanged(object sender, EventArgs e)
        {
        }

        private void nudSeed_ValueChanged(object sender, EventArgs e)
        {
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
        }

        private void heightmapViewerToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void cbVoxels_CheckedChanged(object sender, EventArgs e)
        {
        }
    }
}

namespace TerrainGen
{
    partial class RenderController
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RenderController));
            this.nudSeed = new System.Windows.Forms.NumericUpDown();
            this.bRandomize = new System.Windows.Forms.Button();
            this.lSeed = new System.Windows.Forms.Label();
            this.nudSideLength = new System.Windows.Forms.NumericUpDown();
            this.lSideLen = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.pbTerrainColor = new System.Windows.Forms.PictureBox();
            this.lTerrainColor = new System.Windows.Forms.Label();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.bCreate = new System.Windows.Forms.ToolStripButton();
            this.bOpen = new System.Windows.Forms.ToolStripButton();
            this.colorPicker = new System.Windows.Forms.ColorDialog();
            ((System.ComponentModel.ISupportInitialize)(this.nudSeed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSideLength)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbTerrainColor)).BeginInit();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // nudSeed
            // 
            this.nudSeed.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.nudSeed.Location = new System.Drawing.Point(125, 22);
            this.nudSeed.Name = "nudSeed";
            this.nudSeed.Size = new System.Drawing.Size(317, 20);
            this.nudSeed.TabIndex = 2;
            this.nudSeed.ValueChanged += new System.EventHandler(this.nudSeed_ValueChanged);
            // 
            // bRandomize
            // 
            this.bRandomize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bRandomize.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.bRandomize.Image = global::TerrainGen.EmbeddedFiles.arrow_switch;
            this.bRandomize.Location = new System.Drawing.Point(96, 20);
            this.bRandomize.Name = "bRandomize";
            this.bRandomize.Size = new System.Drawing.Size(23, 23);
            this.bRandomize.TabIndex = 3;
            this.bRandomize.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.bRandomize.UseVisualStyleBackColor = true;
            this.bRandomize.Click += new System.EventHandler(this.bRandomize_Click);
            // 
            // lSeed
            // 
            this.lSeed.AutoSize = true;
            this.lSeed.Location = new System.Drawing.Point(6, 24);
            this.lSeed.Name = "lSeed";
            this.lSeed.Size = new System.Drawing.Size(32, 13);
            this.lSeed.TabIndex = 4;
            this.lSeed.Text = "Seed";
            // 
            // nudSideLength
            // 
            this.nudSideLength.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.nudSideLength.Location = new System.Drawing.Point(125, 48);
            this.nudSideLength.Maximum = new decimal(new int[] {
            128,
            0,
            0,
            0});
            this.nudSideLength.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudSideLength.Name = "nudSideLength";
            this.nudSideLength.Size = new System.Drawing.Size(317, 20);
            this.nudSideLength.TabIndex = 5;
            this.nudSideLength.Value = new decimal(new int[] {
            8,
            0,
            0,
            0});
            this.nudSideLength.ValueChanged += new System.EventHandler(this.nudSideLength_ValueChanged);
            // 
            // lSideLen
            // 
            this.lSideLen.AutoSize = true;
            this.lSideLen.Location = new System.Drawing.Point(6, 50);
            this.lSideLen.Name = "lSideLen";
            this.lSideLen.Size = new System.Drawing.Size(108, 13);
            this.lSideLen.TabIndex = 6;
            this.lSideLen.Text = "Side Length (chunks)";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.bRandomize);
            this.groupBox1.Controls.Add(this.nudSeed);
            this.groupBox1.Controls.Add(this.lSeed);
            this.groupBox1.Controls.Add(this.nudSideLength);
            this.groupBox1.Controls.Add(this.lSideLen);
            this.groupBox1.Location = new System.Drawing.Point(12, 28);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(448, 80);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Terrain";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.pbTerrainColor);
            this.groupBox2.Controls.Add(this.lTerrainColor);
            this.groupBox2.Location = new System.Drawing.Point(12, 114);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(448, 287);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Render";
            // 
            // pbTerrainColor
            // 
            this.pbTerrainColor.BackColor = System.Drawing.SystemColors.Control;
            this.pbTerrainColor.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pbTerrainColor.Cursor = System.Windows.Forms.Cursors.Cross;
            this.pbTerrainColor.Location = new System.Drawing.Point(6, 33);
            this.pbTerrainColor.Name = "pbTerrainColor";
            this.pbTerrainColor.Size = new System.Drawing.Size(100, 50);
            this.pbTerrainColor.TabIndex = 28;
            this.pbTerrainColor.TabStop = false;
            this.pbTerrainColor.Click += new System.EventHandler(this.pbMinColor_Click);
            // 
            // lTerrainColor
            // 
            this.lTerrainColor.AutoSize = true;
            this.lTerrainColor.Location = new System.Drawing.Point(3, 17);
            this.lTerrainColor.Name = "lTerrainColor";
            this.lTerrainColor.Size = new System.Drawing.Size(67, 13);
            this.lTerrainColor.TabIndex = 26;
            this.lTerrainColor.Text = "Terrain Color";
            // 
            // toolStrip1
            // 
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.bCreate,
            this.bOpen});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(472, 25);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // bCreate
            // 
            this.bCreate.Image = global::TerrainGen.EmbeddedFiles.brick_edit;
            this.bCreate.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.bCreate.Name = "bCreate";
            this.bCreate.Size = new System.Drawing.Size(100, 22);
            this.bCreate.Text = "&Create Terrain";
            this.bCreate.Click += new System.EventHandler(this.bCreateTerrain_Click);
            // 
            // bOpen
            // 
            this.bOpen.Image = global::TerrainGen.EmbeddedFiles.folder_brick;
            this.bOpen.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.bOpen.Name = "bOpen";
            this.bOpen.Size = new System.Drawing.Size(95, 22);
            this.bOpen.Text = "&Open Terrain";
            this.bOpen.Click += new System.EventHandler(this.bOpenTerrain_Click);
            // 
            // colorPicker
            // 
            this.colorPicker.AnyColor = true;
            this.colorPicker.Color = System.Drawing.Color.LimeGreen;
            this.colorPicker.FullOpen = true;
            // 
            // RenderController
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(472, 413);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(488, 452);
            this.Name = "RenderController";
            this.Text = "RenderController";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TerrainLayerList_FormClosing);
            this.Load += new System.EventHandler(this.RenderController_Load);
            ((System.ComponentModel.ISupportInitialize)(this.nudSeed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSideLength)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbTerrainColor)).EndInit();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.NumericUpDown nudSeed;
        private System.Windows.Forms.Button bRandomize;
        private System.Windows.Forms.Label lSeed;
        private System.Windows.Forms.NumericUpDown nudSideLength;
        private System.Windows.Forms.Label lSideLen;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton bCreate;
        private System.Windows.Forms.ToolStripButton bOpen;
        private System.Windows.Forms.PictureBox pbTerrainColor;
        private System.Windows.Forms.Label lTerrainColor;
        private System.Windows.Forms.ColorDialog colorPicker;
    }
}
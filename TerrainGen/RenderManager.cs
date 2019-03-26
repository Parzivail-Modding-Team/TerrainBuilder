using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using TerrainGen.Generator;
using TerrainGen.Job;
using TerrainGen.Shader;

namespace TerrainGen
{
    public class RenderManager
    {
        private BlockingCollection<IJob> _fgJobs;
        private BlockingCollection<IJob> _bgJobs;
        private readonly CsTerrainGenerator _generator;
        private readonly ShaderProgram _shaderProgram;
        private readonly Uniform _tintUniform = new Uniform("tint");

        private Thread _worker;

        public Chunk[] Chunks { get; private set; }
        public Vector3 TintColor { get; set; }
        public int SideLength { get; set; }
        public long Seed { get; set; }

        public RenderManager(CsTerrainGenerator generator)
        {
            _fgJobs = new BlockingCollection<IJob>();
            _bgJobs = new BlockingCollection<IJob>();
            _generator = generator;
            _shaderProgram = new DefaultShaderProgram(EmbeddedFiles.default_fs);
            _shaderProgram.InitProgram();
            CreateChunks();
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
                _bgJobs.Add(job);
            else
                _fgJobs.Add(job);
        }

        public void UpdateJobs()
        {
            if (_worker is null)
            {
                _worker = new Thread(() =>
                {
                    while (true)
                    {
                        if (_bgJobs.Count > 0)
                            Parallel.ForEach(_bgJobs.GetConsumingEnumerable(), job =>
                            {
                                job.Execute(this);
                            });
                    }
                });
                _worker.Start();
            }

            if (_fgJobs.Count > 0)
                Parallel.ForEach(_fgJobs.GetConsumingEnumerable(), job =>
                {
                    job.Execute(this);
                });
        }

        public void Render()
        {
            // Set up uniforms
            _tintUniform.Value = TintColor;

            GL.Color3(Color.White);

            GL.PushMatrix();
            GL.Translate(-SideLength * 8, 0, -SideLength * 8);
            // Engage shader, render, disengage
            _shaderProgram.Use(_tintUniform);
            foreach (var chunk in Chunks)
                chunk?.Draw();
            GL.UseProgram(0);
            GL.PopMatrix();

            // Render the ocean
            GL.Color3(Color.MediumBlue);

            var waterLevel = _generator.GetWaterLevel();

            GL.Begin(PrimitiveType.Quads);
            GL.Normal3(Vector3.UnitY);
            GL.Vertex3(-SideLength * 8, waterLevel - 0.4, -SideLength * 8);
            GL.Vertex3(SideLength * 8, waterLevel - 0.4, -SideLength * 8);
            GL.Vertex3(SideLength * 8, waterLevel - 0.4, SideLength * 8);
            GL.Vertex3(-SideLength * 8, waterLevel - 0.4, SideLength * 8);
            GL.End();
        }

        public void Rebuild()
        {
            CancelJobs();
            EnqueueJob(new JobRebuildChunks(_generator));
        }

        private void CancelJobs()
        {
        }
    }
}

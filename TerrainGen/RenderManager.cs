using System;
using System.Collections;
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
        private readonly Queue<IJob> _jobs;
        private readonly CsTerrainGenerator _generator;
        private readonly ShaderProgram _shaderProgram;
        private readonly Uniform _tintUniform = new Uniform("tint");

        private Thread _worker;
        private Chunk[] _chunks;

        public Vector3 TintColor { get; set; }
        public int SideLength { get; set; }
        public long Seed { get; set; }

        public RenderManager(CsTerrainGenerator generator)
        {
            _jobs = new Queue<IJob>();
            _generator = generator;
            _shaderProgram = new DefaultShaderProgram(EmbeddedFiles.default_fs);
            _shaderProgram.InitProgram();
            CreateChunks();
        }

        private void CreateChunks()
        {
            _chunks = new Chunk[SideLength * SideLength];
            for (var i = 0; i < SideLength; i++)
            for (var j = 0; j < SideLength; j++)
            {
                _chunks[i * SideLength + j] = new Chunk(i, j);
            }
        }

        public void EnqueueJob(IJob job)
        {
            _jobs.Enqueue(job);
        }

        public void UpdateJobs()
        {
            if (_worker != null && _worker.IsAlive) return;

            var nextJob = _jobs.Dequeue();
            if (nextJob.CanExecuteInBackground())
            {
                _worker = new Thread(() => nextJob.Execute(this));
                _worker.Start();
            }
            else
            {
                _worker = null;
                nextJob.Execute(this);
            }
        }

        public void Render()
        {
            // Set up uniforms
            _tintUniform.Value = TintColor;

            GL.Color3(Color.White);

            // Engage shader, render, disengage
            _shaderProgram.Use(_tintUniform);
            foreach (var chunk in _chunks)
                chunk.Render();
            GL.UseProgram(0);

            // Render the ocean
            GL.Color3(Color.MediumBlue);

            var waterLevel = _generator.GetWaterLevel();

            GL.Begin(PrimitiveType.Quads);
            GL.Normal3(Vector3.UnitY);
            GL.Vertex3(-SideLength / 2f, waterLevel - 0.1, -SideLength / 2f);
            GL.Vertex3(SideLength / 2f, waterLevel - 0.1, -SideLength / 2f);
            GL.Vertex3(SideLength / 2f, waterLevel - 0.1, SideLength / 2f);
            GL.Vertex3(-SideLength / 2f, waterLevel - 0.1, SideLength / 2f);
            GL.End();
        }
    }
}

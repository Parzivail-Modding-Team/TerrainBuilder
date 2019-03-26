using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrainGen.Generator;

namespace TerrainGen.Job
{
    class JobRebuildChunks : IJob
    {
        private readonly CsTerrainGenerator _generator;

        public JobRebuildChunks(CsTerrainGenerator generator)
        {
            _generator = generator;
        }

        public void Execute(RenderManager renderManager)
        {
            if (renderManager.Chunks.Length != renderManager.SideLength * renderManager.SideLength)
                renderManager.CreateChunks();
            foreach (var chunk in renderManager.Chunks)
                renderManager.EnqueueJob(new JobPregenerateChunk(chunk, _generator));
        }

        public bool CanExecuteInBackground()
        {
            return true;
        }

        public bool IsCancellable()
        {
            return true;
        }
    }
}

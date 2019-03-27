using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrainGen.Generator;

namespace TerrainGen.Job
{
    class JobPregenerateChunks : IJob
    {
        private readonly CsTerrainGenerator _generator;

        public JobPregenerateChunks(CsTerrainGenerator generator)
        {
            _generator = generator;
        }

        public void Execute(RenderManager renderManager)
        {
            Parallel.ForEach(renderManager.Chunks, chunk =>
            {
                chunk.Generate(_generator);
                chunk.Prerender();
                renderManager.EnqueueJob(new JobRenderChunk(chunk));
            });
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

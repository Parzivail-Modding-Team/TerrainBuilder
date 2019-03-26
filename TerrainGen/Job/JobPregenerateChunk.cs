using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrainGen.Generator;

namespace TerrainGen.Job
{
    class JobPregenerateChunk : IJob
    {
        private readonly Chunk _chunk;
        private readonly CsTerrainGenerator _generator;

        public JobPregenerateChunk(Chunk chunk, CsTerrainGenerator generator)
        {
            _chunk = chunk;
            _generator = generator;
        }

        public void Execute(RenderManager renderManager)
        {
            _chunk.Generate(_generator);
            _chunk.Prerender();
            renderManager.EnqueueJob(new JobRenderChunk(_chunk));
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrainGen.Job
{
    class JobRenderChunk : IJob
    {
        private readonly Chunk _chunk;

        public JobRenderChunk(Chunk chunk)
        {
            _chunk = chunk;
        }

        public void Execute(RenderManager renderManager)
        {
            _chunk.Render();
        }

        public bool CanExecuteInBackground()
        {
            return false;
        }

        public bool IsCancellable()
        {
            return true;
        }
    }
}

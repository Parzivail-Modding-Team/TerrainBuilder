using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrainGen.Job
{
    class JobSetSeed : IJob
    {
        private readonly long _seed;

        public JobSetSeed(long seed)
        {
            _seed = seed;
        }

        /// <inheritdoc />
        public void Execute(RenderManager renderManager)
        {
            renderManager.Seed = _seed;
        }

        /// <inheritdoc />
        public bool CanExecuteInBackground()
        {
            return true;
        }
    }
}

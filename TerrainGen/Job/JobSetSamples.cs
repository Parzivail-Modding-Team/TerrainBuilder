using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrainGen.Job
{
    class JobSetSamples : IJob
    {
        private readonly int _samples;

        public JobSetSamples(int samples)
        {
            _samples = samples;
        }

        /// <inheritdoc />
        public void Execute(RenderManager renderManager)
        {
            renderManager.SetSamples(_samples);
        }

        /// <inheritdoc />
        public bool CanExecuteInBackground()
        {
            return false;
        }

        public bool IsCancellable()
        {
            return false;
        }
    }
}

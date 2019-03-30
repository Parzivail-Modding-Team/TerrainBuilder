using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace TerrainGen.Job
{
    class JobSetLightPosition : IJob
    {
        private readonly Vector3 _lightPosition;

        public JobSetLightPosition(Vector3 lightPosition)
        {
            _lightPosition = lightPosition;
        }

        public void Execute(RenderManager renderManager)
        {
            renderManager.LightPosition = _lightPosition;
        }

        public bool CanExecuteInBackground()
        {
            return true;
        }

        public bool IsCancellable()
        {
            return false;
        }
    }
}

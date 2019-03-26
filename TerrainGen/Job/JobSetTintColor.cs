using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace TerrainGen.Job
{
    class JobSetTintColor : IJob
    {
        private readonly Vector3 _tintColor;

        public JobSetTintColor(Vector3 tintColor)
        {
            _tintColor = tintColor;
        }

        public void Execute(RenderManager renderManager)
        {
            renderManager.TintColor = _tintColor;
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

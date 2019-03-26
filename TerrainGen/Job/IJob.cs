using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrainGen.Job
{
    public interface IJob
    {
        void Execute(RenderManager renderManager);
    }
}

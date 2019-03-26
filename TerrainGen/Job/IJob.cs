using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TerrainGen.Job
{
    public interface IJob
    {
        /// <summary>
        /// Executes the job with the specified render context
        /// </summary>
        /// <param name="renderManager"></param>
        void Execute(RenderManager renderManager);

        /// <summary>
        /// Returns true if the job doesn't need to be a part of the UI thread
        /// </summary>
        /// <returns></returns>
        bool CanExecuteInBackground();
    }
}

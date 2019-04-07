using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace Kuat
{
    internal static class KNativeMethods
    {
        /// <summary>
        ///     Gets the time, in milliseconds, that the user has configured to be the minimum time between caret blinks
        /// </summary>
        /// <returns>The time in milliseconds</returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern uint GetCaretBlinkTime();

        /// <summary>
        ///     Gets the time, in milliseconds, that the user has configured to be the minimum time between clicks to be considered
        ///     a double click
        /// </summary>
        /// <returns>The time in milliseconds</returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern int GetDoubleClickTime();
    }
}

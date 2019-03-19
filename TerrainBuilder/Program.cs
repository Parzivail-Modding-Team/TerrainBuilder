using System;
using System.Windows.Forms;
using OpenTK;

namespace TerrainBuilder
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            new WindowVisualize
            {
                VSync = VSyncMode.On
            }.Run(40);
        }
    }
}

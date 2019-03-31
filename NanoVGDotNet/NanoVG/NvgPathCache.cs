namespace NanoVGDotNet.NanoVG
{
    public class NvgPathCache
    {
        public NvgPoint[] Points;
        public int Npoints;
        public int Cpoints;
        public NvgPath[] Paths;
        public int Npaths;
        public int Cpaths;
        public NvgVertex[] Verts;
        public int Nverts;
        public int Cverts;
        //[4];
        public float[] Bounds;

        public NvgPathCache()
        {
            Bounds = new float[4];
        }
    }
}
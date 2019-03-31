namespace NanoVGDotNet.NanoVG
{
    public class GlNvgShader
    {
        public int Prog;
        public int Frag;
        public int Vert;
        //[GLNVG_MAX_LOCS];
        public int[] Loc;

        public GlNvgShader()
        {
            Loc = new int[(int)GlNvgUniformLoc.MaxLocs];
        }
    }
}
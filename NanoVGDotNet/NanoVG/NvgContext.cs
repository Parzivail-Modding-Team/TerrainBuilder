using NanoVGDotNet.FontStash;

namespace NanoVGDotNet.NanoVG
{
    public class NvgContext
    {
        public NvgParams Params;
        public float[] Commands;
        public int Ccommands;
        public int Ncommands;
        public float Commandx, Commandy;
        //[NVG_MAX_STATES];
        public NvgState[] States;
        public int Nstates;
        public NvgPathCache Cache;
        public float TessTol;
        public float DistTol;
        public float FringeWidth;
        public float DevicePxRatio;
        public FONScontext Fs;
        //[NVG_MAX_FONTIMAGES];
        public int[] FontImages;
        public int FontImageIdx;
        public int DrawCallCount;
        public int FillTriCount;
        public int StrokeTriCount;
        public int TextTriCount;

        public NvgContext()
        {
            States = new NvgState[NanoVg.NvgMaxStates];
            for (var cont = 0; cont < States.Length; cont++)
                States[cont] = new NvgState();
            FontImages = new int[NanoVg.NvgMaxFontimages];
        }
    }
}
using System;

namespace NanoVGDotNet.NanoVG
{
    public class NvgScissor
    {
        //[6];
        public float[] Xform;
        //[2];
        public float[] Extent;

        public NvgScissor()
        {
            Xform = new float[6];
            Extent = new float[2];
        }

        public NvgScissor Clone()
        {
            var newScissor = new NvgScissor();

            Array.Copy(Xform, newScissor.Xform, Xform.Length);
            Array.Copy(Extent, newScissor.Extent, Extent.Length);

            return newScissor;
        }
    }
}
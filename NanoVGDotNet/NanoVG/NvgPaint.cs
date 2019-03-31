using System;

namespace NanoVGDotNet.NanoVG
{
    public class NvgPaint
    {
        //[6];
        public float[] Xform;
        //[2];
        public float[] Extent;
        public float Radius;
        public float Feather;
        public NvgColor InnerColor;
        public NvgColor OuterColor;
        public int Image;

        public NvgPaint()
        {
            Xform = new float[6];
            Extent = new float[2];
        }

        public NvgPaint Clone()
        {
            var newPaint = new NvgPaint();

            Array.Copy(Xform, newPaint.Xform, Xform.Length);
            Array.Copy(Extent, newPaint.Extent, Extent.Length);
            newPaint.Radius = Radius;
            newPaint.Feather = Feather;
            newPaint.InnerColor = InnerColor;
            newPaint.OuterColor = OuterColor;
            newPaint.Image = Image;

            return newPaint;
        }
    }
}
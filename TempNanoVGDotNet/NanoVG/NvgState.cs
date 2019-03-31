using System;

namespace NanoVGDotNet.NanoVG
{
    public class NvgState
    {
        public NvgCompositeOperationState CompositeOperation;
        public NvgPaint Fill;
        public NvgPaint Stroke;
        public float StrokeWidth;
        public float MiterLimit;
        public int LineJoin;
        public int LineCap;
        public float Alpha;
        //[6];
        public float[] Xform;
        public NvgScissor Scissor;
        public float FontSize;
        public float LetterSpacing;
        public float LineHeight;
        public float FontBlur;
        public int TextAlign;
        public int FontId;

        public NvgState()
        {
            Xform = new float[6];
            Scissor = new NvgScissor();
            Fill = new NvgPaint();
            Stroke = new NvgPaint();
        }

        public NvgState Clone()
        {
            var newState = new NvgState();
            newState.CompositeOperation = CompositeOperation;
            newState.Fill = Fill.Clone();
            newState.Stroke = Stroke.Clone();
            newState.StrokeWidth = StrokeWidth;
            newState.MiterLimit = MiterLimit;
            newState.LineJoin = LineJoin;
            newState.LineCap = LineCap;
            newState.Alpha = Alpha;

            Array.Copy(Xform, newState.Xform, Xform.Length);

            newState.Scissor = Scissor.Clone();
            newState.FontSize = FontSize;
            newState.LetterSpacing = LetterSpacing;
            newState.LineHeight = LineHeight;
            newState.FontBlur = FontBlur;
            newState.TextAlign = TextAlign;
            newState.FontId = FontId;

            return newState;
        }
    }
}
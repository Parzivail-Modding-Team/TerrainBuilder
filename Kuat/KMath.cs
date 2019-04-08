using System;
using System.Drawing;

namespace Kuat
{
    internal static class KMath
    {
        public const float PI = (float) Math.PI;

        public static float Lerp(float x, float a, float b)
        {
            return x * b + (1 - x) * a;
        }

        public static float Remap(float x, float iMin, float iMax, float oMin, float oMax)
        {
            return (x - iMin) / (iMax - iMin) * (oMax - oMin) + oMin;
        }

        public static Point UnitVector(float angle, float radius)
        {
            return new Point((int) (Math.Cos(angle) * radius), (int) (Math.Sin(angle) * radius));
        }

        public static PointF OffsetByUnitVector(this Point origin, float angle, float radius)
        {
            return (PointF) origin + new SizeF((float) (Math.Cos(angle) * radius), (float) (Math.Sin(angle) * radius));
        }

        public static int GetUnixTime()
        {
            return (int) DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        }

        public static int GetTodayMs()
        {
            return (int) DateTime.Now.Subtract(DateTime.Today).TotalMilliseconds;
        }
    }
}
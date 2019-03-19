using System;

namespace TerrainGenCore
{
    public class ProcNoise
    {
        private static OpenSimplexNoise _noise = new OpenSimplexNoise();
        private static InfiniteWorleyNoise _worley = new InfiniteWorleyNoise();
        private static long _seed;

        public static double HashA(double x, double z)
        {
            return MathUtil.Fract(Math.Sin(MathUtil.Seed(x - 173.37, _seed) * 7441.35 - 4113.21 * Math.Cos(x * z) + MathUtil.Seed(z - 1743.7, _seed) * 1727.93 * 1291.27) * 2853.85 + MathUtil.OneOverGoldenRatio);
        }

        public static double HashB(double x, double z)
        {
            return MathUtil.Fract(Math.Cos(MathUtil.Seed(z - 143.37, _seed) * 4113.21 - 2853.85 * Math.Sin(x * z) + MathUtil.Seed(x - 743.37, _seed) * 1291.27 * 1727.93) * 4113.21 + MathUtil.OneOverGoldenRatio);
        }

        public static double OctaveNoise(double x, double z, int octaves)
        {
            var n = Noise(x, z) / 2;
            if (octaves <= 1)
                return n;
            return n + OctaveNoise((x + octaves * 100) * 2, (z + octaves * 100) * 2, octaves - 1) / 2;
        }

        public static double OctaveWorley(double x, double z, int octaves)
        {
            var n = Worley(x, z) / 2;
            if (octaves <= 1)
                return n;
            return n + OctaveWorley((x + octaves * 100) * 2, (z + octaves * 100) * 2, octaves - 1) / 2;
        }

        public static double OctaveInvWorley(double x, double z, int octaves)
        {
            var n = (1 - Worley(x, z)) / 2;
            if (octaves <= 1)
                return n;
            return n + OctaveInvWorley((x + octaves * 100) * 2, (z + octaves * 100) * 2, octaves - 1) / 2;
        }

        public static double Worley(double x, double z)
        {
            return _worley.Eval(x, z);
        }

        public static double RawWorley(double x, double z)
        {
            return _worley.Eval(x, z) * 2 - 1;
        }

        public static double Worley(double x, double y, double z)
        {
            return _worley.Eval(x, y, z);
        }

        public static double RawWorley(double x, double y, double z)
        {
            return _worley.Eval(x, y, z) * 2 - 1;
        }

        public static double NoiseDx(double x, double z)
        {
            var n = Noise(x, z);
            const double d = 0.001;
            return Noise(x + d, z) - n;
        }

        public static double NoiseDz(double x, double z)
        {
            var n = Noise(x, z);
            const double d = 0.001;
            return Noise(x, z + d) - n;
        }

        public static double Noise(double x, double z)
        {
            return (_noise.Eval(x, z) + 1) / 2;
        }

        public static double Noise(double x, double y, double z)
        {
            return (_noise.Eval(x, y, z) + 1) / 2;
        }

        public static double Noise(double x, double y, double z, double w)
        {
            return (_noise.Eval(x, y, z, w) + 1) / 2;
        }

        public static double RawNoise(double x, double z)
        {
            return _noise.Eval(x, z);
        }

        public static double RawNoise(double x, double y, double z)
        {
            return _noise.Eval(x, y, z);
        }

        public static double RawNoise(double x, double y, double z, double w)
        {
            return _noise.Eval(x, y, z, w);
        }

        public static void SetSeed(long seed)
        {
            _seed = seed;
            _noise = new OpenSimplexNoise(seed);
            _worley = new InfiniteWorleyNoise(seed);
        }
    }
}

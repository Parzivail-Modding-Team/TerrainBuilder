using System.Runtime.InteropServices;

namespace TerrainGen
{
    [StructLayout(LayoutKind.Explicit, Size = 3)]
    public struct SmallVertex
    {
        public const int Size = 3;
        public static readonly SmallVertex UnitY = new SmallVertex(0, sbyte.MaxValue, 0);
        public static readonly SmallVertex UnitX = new SmallVertex(sbyte.MaxValue, 0, 0);
        public static readonly SmallVertex UnitNX = new SmallVertex(sbyte.MinValue, 0, 0);
        public static readonly SmallVertex UnitZ = new SmallVertex(0, 0, sbyte.MaxValue);
        public static readonly SmallVertex UnitNZ = new SmallVertex(0, 0, sbyte.MinValue);

        [FieldOffset(0)] public sbyte X;
        [FieldOffset(1)] public sbyte Y;
        [FieldOffset(2)] public sbyte Z;

        public SmallVertex(sbyte x, sbyte y, sbyte z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}
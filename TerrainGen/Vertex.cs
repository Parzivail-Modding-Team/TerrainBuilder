using System.Runtime.InteropServices;

namespace TerrainGen
{
    [StructLayout(LayoutKind.Explicit, Size = 6)]
    public struct Vertex
    {
        public const int Size = 6;
        [FieldOffset(0)] public short X;
        [FieldOffset(2)] public short Y;
        [FieldOffset(4)] public short Z;

        public Vertex(float x, float y, float z)
        {
            X = (short)x;
            Y = (short)y;
            Z = (short)z;
        }
    }
}
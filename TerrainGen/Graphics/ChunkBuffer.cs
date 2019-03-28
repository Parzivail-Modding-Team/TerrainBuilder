namespace TerrainGen.Graphics
{
    public class ChunkBuffer
    {
        public object LockHandle;
        public Vertex[] VertexBuffer;
        public SmallVertex[] NormalBuffer;
        public int[] ColorBuffer;
        public short[] IndexBuffer;
        public short Length;

        public ChunkBuffer()
        {
            LockHandle = new object();
            VertexBuffer = new Vertex[5120];
            NormalBuffer = new SmallVertex[5120];
            ColorBuffer = new int[5120];
            IndexBuffer = new short[5120];
            Length = 0;
        }

        public void Reset()
        {
            lock (LockHandle)
            {
                Length = 0;
            }
        }

        public short Append(Vertex pos, SmallVertex normal, int color)
        {
            lock (LockHandle)
            {
                VertexBuffer[Length] = pos;
                NormalBuffer[Length] = normal;
                ColorBuffer[Length] = color;
                IndexBuffer[Length] = Length;
                Length++;
                return (short) (Length - 1);
            }
        }

        public void Append(short elementIdx)
        {
            lock (LockHandle)
            {
                IndexBuffer[Length] = elementIdx;
                Length++;
            }
        }
    }
}

namespace TerrainGen.Graphics
{
    public class ChunkBuffer
    {
        public object LockHandle = new object();
        public Vertex[] VertexBuffer;
        public SmallVertex[] NormalBuffer;
        public int[] ColorBuffer;
        public short[] IndexBuffer;

        public short Length { get; private set; }

        public ChunkBuffer()
        {
            VertexBuffer = new Vertex[5120];
            NormalBuffer = new SmallVertex[5120];
            ColorBuffer = new int[5120];
            IndexBuffer = new short[5120];
        }

        public void Reset()
        {
            lock (LockHandle)
            {
                Length = 0;
            }
        }

        public void Append(Vertex pos, SmallVertex normal, int color)
        {
            lock (LockHandle)
            {
                VertexBuffer[Length] = pos;
                NormalBuffer[Length] = normal;
                ColorBuffer[Length] = color;
                IndexBuffer[Length] = Length;
                Length++;
            }
        }
    }
}

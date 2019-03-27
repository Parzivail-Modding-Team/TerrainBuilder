using System.Collections.Generic;
using OpenTK;

namespace TerrainGen.Buffer
{
    public class VertexBufferInitializer
    {
        public List<Vector3> Vertices { get; private set; }
        public List<Vector3> Normals { get; private set; }
        public List<int> Indices { get; private set; }
        public List<int> Colors { get; private set; }

        public VertexBufferInitializer(List<Vector3> vertices, List<Vector3> normals, List<int> colors, List<int> indices)
        {
            Vertices = vertices;
            Normals = normals;
            Colors = colors;
            Indices = indices;
        }

        public VertexBufferInitializer()
        {
            Vertices = new List<Vector3>();
            Normals = new List<Vector3>();
            Colors = new List<int>();
            Indices = new List<int>();
        }

        public void AddVertex(Vector3 pos)
        {
            AddVertex(pos, Vector3.Zero);
        }

        public void AddVertex(Vector3 pos, Vector3 normal)
        {
            AddVertex(pos, normal, 0xFFFFFF);
        }

        public void AddVertex(Vector3 pos, Vector3 normal, int color)
        {
            AddVertex(pos, normal, color, Indices.Count);
        }

        public void AddVertex(Vector3 pos, Vector3 normal, int color, int index)
        {
            Vertices.Add(pos);
            Normals.Add(normal);
            Colors.Add(color);
            Indices.Add(index);
        }

        public void Reset()
        {
            Vertices.Clear();
            Vertices.TrimExcess();
            Normals.Clear();
            Normals.TrimExcess();
            Colors.Clear();
            Colors.TrimExcess();
            Indices.Clear();
            Indices.TrimExcess();
        }
    }
}
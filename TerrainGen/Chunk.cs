using OpenTK;
using TerrainGen.Generator;
using TerrainGen.Graphics;

namespace TerrainGen
{
    public class Chunk
    {
        public int X { get; }
        public int Z { get; }

        private readonly byte[,] _heightmap = new byte[18,18];
        private readonly VertexBuffer _vbo = new VertexBuffer();
        private readonly ChunkBuffer _vbi = new ChunkBuffer();

        public Chunk(int x, int z)
        {
            X = x;
            Z = z;
        }

        /// <summary>
        /// Generates chunk heightmap
        /// </summary>
        /// <param name="generator"></param>
        public void Generate(CsTerrainGenerator generator)
        {
            for (var i = -1; i < 17; i++)
            for (var j = -1; j < 17; j++)
                _heightmap[i + 1, j + 1] = (byte) generator.GetValue(X * 16 + i, Z * 16 + j);
        }

        /// <summary>
        /// Initializes the VBO but does not modify graphics state
        /// </summary>
        public void Prerender()
        {
            const int color = 0xFFFFFF;
            const int occludedColor = 0xC0C0C0;
            _vbi.Reset();

            for (var x = X * 16; x < X * 16 + 16; x++)
            for (var z = Z * 16; z < Z * 16 + 16; z++)
            {
                var nx = x - X * 16 + 1;
                var nz = z - Z * 16 + 1;

                // Heightmap value here
                var valueHere = (float)_heightmap[nx, nz];

                // Neighboring positions
                var valuePosX = _heightmap[nx + 1, nz];
                var valueNegX = _heightmap[nx - 1, nz];
                var valuePosZ = _heightmap[nx, nz + 1];
                var valueNegZ = _heightmap[nx, nz - 1];

                var valuePosXPosZ = _heightmap[nx + 1, nz + 1];
                var valueNegXPosZ = _heightmap[nx - 1, nz + 1];
                var valuePosXNegZ = _heightmap[nx + 1, nz - 1];
                var valueNegXNegZ = _heightmap[nx - 1, nz - 1];

                // Comparisons used in cheap AO
                var isPosXHigher = valuePosX > valueHere;
                var isNegXHigher = valueNegX > valueHere;
                var isPosZHigher = valuePosZ > valueHere;
                var isNegZHigher = valueNegZ > valueHere;

                var isPosXPosZHigher = valuePosXPosZ > valueHere;
                var isNegXPosZHigher = valueNegXPosZ > valueHere;
                var isPosXNegZHigher = valuePosXNegZ > valueHere;
                var isNegXNegZHigher = valueNegXNegZ > valueHere;

                // Always draw a top face for a block
                _vbi.Append(
                    new Vertex(x, valueHere, z),
                    SmallVertex.UnitY,
                    isPosXHigher || isPosZHigher || isPosXPosZHigher ? occludedColor : color
                );

                _vbi.Append(
                    new Vertex(x - 1, valueHere, z),
                    SmallVertex.UnitY,
                    isNegXHigher || isPosZHigher || isNegXPosZHigher ? occludedColor : color
                );

                _vbi.Append(
                    new Vertex(x - 1, valueHere, z - 1),
                    SmallVertex.UnitY,
                    isNegXHigher || isNegZHigher || isNegXNegZHigher ? occludedColor : color
                );

                _vbi.Append(
                    new Vertex(x, valueHere, z - 1),
                    SmallVertex.UnitY,
                    isPosXHigher || isNegZHigher || isPosXNegZHigher ? occludedColor : color
                );

                // Try and draw the PosZ face
                if (valuePosZ < valueHere)
                {
                    _vbi.Append(
                        new Vertex(x, valueHere, z),
                        SmallVertex.UnitZ,
                        color
                    );

                    _vbi.Append(
                        new Vertex(x, valuePosZ, z),
                        SmallVertex.UnitZ,
                        occludedColor
                    );

                    _vbi.Append(
                        new Vertex(x - 1, valuePosZ, z),
                        SmallVertex.UnitZ,
                        occludedColor
                    );

                    _vbi.Append(
                        new Vertex(x - 1, valueHere, z),
                        SmallVertex.UnitZ,
                        color
                    );
                }

                // Try and draw the NegZ face
                if (valueNegZ < valueHere)
                {
                    _vbi.Append(
                        new Vertex(x, valueHere, z - 1),
                        SmallVertex.UnitNZ,
                        color
                    );

                    _vbi.Append(
                        new Vertex(x, valueNegZ, z - 1),
                        SmallVertex.UnitNZ,
                        occludedColor
                    );

                    _vbi.Append(
                        new Vertex(x - 1, valueNegZ, z - 1),
                        SmallVertex.UnitNZ,
                        occludedColor
                    );

                    _vbi.Append(
                        new Vertex(x - 1, valueHere, z - 1),
                        SmallVertex.UnitNZ,
                        color
                    );
                }

                // Try and draw the PosX face
                if (valuePosX < valueHere)
                {
                    _vbi.Append(
                        new Vertex(x, valueHere, z),
                        SmallVertex.UnitX,
                        color
                    );

                    _vbi.Append(
                        new Vertex(x, valuePosX, z),
                        SmallVertex.UnitX,
                        occludedColor
                    );

                    _vbi.Append(
                        new Vertex(x, valuePosX, z - 1),
                        SmallVertex.UnitX,
                        occludedColor
                    );

                    _vbi.Append(
                        new Vertex(x, valueHere, z - 1),
                        SmallVertex.UnitX,
                        color
                    );
                }

                // Try and draw the NegX face
                if (valueNegX < valueHere)
                {
                    _vbi.Append(
                        new Vertex(x - 1, valueHere, z),
                        SmallVertex.UnitNX,
                        color
                    );

                    _vbi.Append(
                        new Vertex(x - 1, valueNegX, z),
                        SmallVertex.UnitNX,
                        occludedColor
                    );

                    _vbi.Append(
                        new Vertex(x - 1, valueNegX, z - 1),
                        SmallVertex.UnitNX,
                        occludedColor
                    );

                    _vbi.Append(
                        new Vertex(x - 1, valueHere, z - 1),
                        SmallVertex.UnitNX,
                        color
                    );
                }
            }
        }

        /// <summary>
        /// Uploads the VBO to OpenGL
        /// </summary>
        public void Render()
        {
            _vbo.InitializeVbo(_vbi);
            _vbi.Reset();
        }

        /// <summary>
        /// Renders the VBO
        /// </summary>
        public void Draw()
        {
            if (!_vbo.Initialized)
                return;

            _vbo.Render();
        }
    }
}
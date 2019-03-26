using OpenTK;
using TerrainGen.Buffer;
using TerrainGen.Generator;

namespace TerrainGen
{
    internal class Chunk
    {
        public int X { get; }
        public int Z { get; }

        private readonly byte[,] _heightmap = new byte[18,18];
        private readonly SimpleVertexBuffer _vbo = new SimpleVertexBuffer();

        private VertexBufferInitializer _vbi;

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
            var occludedColor = 0xC0C0C0;
            _vbi = new VertexBufferInitializer();

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

                var isPosXLower = valuePosX < valueHere;
                var isNegXLower = valueNegX < valueHere;
                var isPosZLower = valuePosZ < valueHere;
                var isNegZLower = valueNegZ < valueHere;

                var isPosXPosZHigher = valuePosXPosZ > valueHere;
                var isNegXPosZHigher = valueNegXPosZ > valueHere;
                var isPosXNegZHigher = valuePosXNegZ > valueHere;
                var isNegXNegZHigher = valueNegXNegZ > valueHere;

                var isPosXPosZLower = valuePosXPosZ < valueHere;
                var isNegXPosZLower = valueNegXPosZ < valueHere;
                var isPosXNegZLower = valuePosXNegZ < valueHere;
                var isNegXNegZLower = valueNegXNegZ < valueHere;

                // Always draw a top face for a block
                _vbi.AddVertex(
                    new Vector3(x, valueHere, z),
                    Vector3.UnitY,
                    isPosXHigher || isPosZHigher || isPosXPosZHigher ? occludedColor : color
                );

                _vbi.AddVertex(
                    new Vector3(x - 1, valueHere, z),
                    Vector3.UnitY,
                    isNegXHigher || isPosZHigher || isNegXPosZHigher ? occludedColor : color
                );

                _vbi.AddVertex(
                    new Vector3(x - 1, valueHere, z - 1),
                    Vector3.UnitY,
                    isNegXHigher || isNegZHigher || isNegXNegZHigher ? occludedColor : color
                );

                _vbi.AddVertex(
                    new Vector3(x, valueHere, z - 1),
                    Vector3.UnitY,
                    isPosXHigher || isNegZHigher || isPosXNegZHigher ? occludedColor : color
                );

                // Try and draw the PosZ face
                if (valuePosZ < valueHere)
                {
                    _vbi.AddVertex(
                        new Vector3(x, valueHere, z),
                        Vector3.UnitZ,
                        color
                    );

                    _vbi.AddVertex(
                        new Vector3(x, valuePosZ, z),
                        Vector3.UnitZ,
                        isPosXLower || isPosZLower || isPosXPosZLower ? occludedColor : color
                    );

                    _vbi.AddVertex(
                        new Vector3(x - 1, valuePosZ, z),
                        Vector3.UnitZ,
                        isNegXLower || isPosZLower || isNegXPosZLower ? occludedColor : color
                    );

                    _vbi.AddVertex(
                        new Vector3(x - 1, valueHere, z),
                        Vector3.UnitZ,
                        color
                    );
                }

                // Try and draw the NegZ face
                if (valueNegZ < valueHere)
                {
                    _vbi.AddVertex(
                        new Vector3(x, valueHere, z - 1),
                        -Vector3.UnitZ,
                        color
                    );

                    _vbi.AddVertex(
                        new Vector3(x, valueNegZ, z - 1),
                        -Vector3.UnitZ,
                        isPosXLower || isNegZLower || isPosXNegZLower ? occludedColor : color
                    );

                    _vbi.AddVertex(
                        new Vector3(x - 1, valueNegZ, z - 1),
                        -Vector3.UnitZ,
                        isNegXLower || isNegZLower || isNegXNegZLower ? occludedColor : color
                    );

                    _vbi.AddVertex(
                        new Vector3(x - 1, valueHere, z - 1),
                        -Vector3.UnitZ,
                        color
                    );
                }

                // Try and draw the PosX face
                if (valuePosX < valueHere)
                {
                    _vbi.AddVertex(
                        new Vector3(x, valueHere, z),
                        Vector3.UnitX,
                        color
                    );

                    _vbi.AddVertex(
                        new Vector3(x, valuePosX, z),
                        Vector3.UnitX,
                        isPosXLower || isPosZLower || isPosXPosZLower ? occludedColor : color
                    );

                    _vbi.AddVertex(
                        new Vector3(x, valuePosX, z - 1),
                        Vector3.UnitX,
                        isPosXLower || isNegZLower || isPosXNegZLower ? occludedColor : color
                    );

                    _vbi.AddVertex(
                        new Vector3(x, valueHere, z - 1),
                        Vector3.UnitX,
                        color
                    );
                }

                // Try and draw the NegX face
                if (valueNegX < valueHere)
                {
                    _vbi.AddVertex(
                        new Vector3(x - 1, valueHere, z),
                        -Vector3.UnitX,
                        color
                    );

                    _vbi.AddVertex(
                        new Vector3(x - 1, valueNegX, z),
                        -Vector3.UnitX,
                        isNegXLower || isPosZLower || isNegXPosZLower ? occludedColor : color
                    );

                    _vbi.AddVertex(
                        new Vector3(x - 1, valueNegX, z - 1),
                        -Vector3.UnitX,
                        isNegXLower || isNegZLower || isNegXNegZLower ? occludedColor : color
                    );

                    _vbi.AddVertex(
                        new Vector3(x - 1, valueHere, z - 1),
                        -Vector3.UnitX,
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
        }

        /// <summary>
        /// Renders the VBO
        /// </summary>
        public void Draw()
        {
            _vbo.Render();
        }
    }
}
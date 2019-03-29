using OpenTK.Graphics.OpenGL;

namespace TerrainGen.Shader
{
    public class DefaultShaderProgram : ShaderProgram
    {
        private readonly string _fProg;
        private readonly string _vProg;

        public DefaultShaderProgram(string fProg, string vProg)
        {
            _fProg = fProg;
            _vProg = vProg;
        }

        protected override void Init()
        {
            LoadShader(_fProg, ShaderType.FragmentShader, PgmId, out FsId);
            LoadShader(_vProg, ShaderType.VertexShader, PgmId, out VsId);

            GL.LinkProgram(PgmId);
            Log(GL.GetProgramInfoLog(PgmId));
        }
    }
}
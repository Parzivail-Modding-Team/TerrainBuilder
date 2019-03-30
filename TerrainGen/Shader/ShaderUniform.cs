namespace TerrainGen.Shader
{
    public class ShaderUniform
    {
        public ShaderUniform(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
        public object Value { get; set; }

        public virtual object GetValue()
        {
            return Value;
        }
    }
}
using System;

namespace TerrainGen.Generator
{
    internal class RequiredMethodEntry
    {
        public string Name { get; }
        public Type ReturnType { get; }
        public Type[] ParameterTypes { get; }

        public RequiredMethodEntry(string name, Type returnType, params Type[] parameterTypes)
        {
            Name = name;
            ReturnType = returnType;
            ParameterTypes = parameterTypes;
        }
    }
}
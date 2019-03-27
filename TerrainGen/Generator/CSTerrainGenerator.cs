using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TerrainGen.Util;
using TerrainGenCore;

namespace TerrainGen.Generator
{
    public class CsTerrainGenerator
    {
        private object _generatorInstance;
        private MethodInfo _methodGetValue;
        private MethodInfo _methodGetWaterLevel;

        private static readonly List<RequiredMethodEntry> RequiredMethods = new List<RequiredMethodEntry>
        {
            new RequiredMethodEntry("GetTerrain", typeof(double), typeof(int), typeof(int)),
            new RequiredMethodEntry("GetWaterLevel", typeof(int))
        };

        public bool LoadScript(string code)
        {
            var codeDomProvider = CodeDomProvider.CreateProvider("CSharp");

            var compilerParameters = new CompilerParameters();
            compilerParameters.ReferencedAssemblies.Add("system.dll");
            compilerParameters.ReferencedAssemblies.Add("TerrainGenCore.dll");

            compilerParameters.GenerateExecutable = false;
            compilerParameters.GenerateInMemory = true;
            
            var compilerResults = codeDomProvider.CompileAssemblyFromSource(compilerParameters, code);
            
            if (compilerResults.Errors.HasErrors)
            {
                var errorMessage = "Compilation failed.\r\n";
                foreach (var error in compilerResults.Output)
                    errorMessage += error + "\r\n";

                Lumberjack.Error(errorMessage);
                return false;
            }
            
            var compiledAssembly = compilerResults.CompiledAssembly;

            try
            {
                var types = compiledAssembly.DefinedTypes;
                foreach (var type in types)
                {
                    if (type.FullName is null || !type.IsDefined(typeof(TerrainProviderAttribute))) continue;
                    
                    _generatorInstance = compiledAssembly.CreateInstance(type.FullName);
                    Lumberjack.Log($"Creating instance of {type.FullName}");
                    break;
                }

                if (_generatorInstance is null)
                {
                    Lumberjack.Log("No classes found in assembly with the TerrainProvider attribute");
                    return false;
                }

                var typeInfo = _generatorInstance.GetType();
                var methods = typeInfo.GetMethods();

                foreach (var requiredMethod in RequiredMethods)
                {
                    if (methods.All(method => method.Name != requiredMethod.Name))
                    {
                        Lumberjack.Error($"Required method {requiredMethod.Name} not found in instantiated class");
                        return false;
                    }

                    var foundMethod = typeInfo.GetMethod(requiredMethod.Name, requiredMethod.ParameterTypes);

                    if (foundMethod is null)
                    {
                        Lumberjack.Error($"Required method {requiredMethod.Name} does not have required signature {string.Join(", ", requiredMethod.ParameterTypes.Select(type => type.Name))}");
                        return false;
                    }

                    if (foundMethod.ReturnType != requiredMethod.ReturnType)
                    {
                        Lumberjack.Error($"Required method {requiredMethod.Name} does not have required return type {requiredMethod.ReturnType.Name}");
                        return false;
                    }
                }

                _methodGetValue = typeInfo.GetMethod("GetTerrain");
                _methodGetWaterLevel = typeInfo.GetMethod("GetWaterLevel");
            }
            catch (Exception e)
            {
                Lumberjack.Error(e.Message);
                return false;
            }

            return true;
        }

        public void SetSeed(long nudSeedValue)
        {
            ProcNoise.SetSeed(nudSeedValue);
        }

        public double GetValue(int x, int z)
        {
            if (_methodGetValue is null)
                return 0;

            var value = (double)_methodGetValue.Invoke(_generatorInstance, new object[] { x, z });

            if (value < 0 || double.IsNaN(value))
                value = 0;
            if (value > 255)
                value = 255;

            return value;
        }

        public int GetWaterLevel()
        {
            if (_methodGetWaterLevel is null)
                return 0;
            return (int)_methodGetWaterLevel.Invoke(_generatorInstance, null);
        }
    }
}

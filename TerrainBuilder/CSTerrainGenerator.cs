using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TerrainBuilder.RenderUtil;
using TerrainGenCore;

namespace TerrainBuilder
{
    public class CsTerrainGenerator
    {
        private object _generatorInstance;
        private MethodInfo _methodGetValue;
        private MethodInfo _methodGetTree;
        private MethodInfo _methodGetWaterLevel;

        public bool LoadScript(string code)
        {
            var codeDomProvider = CodeDomProvider.CreateProvider("CSharp");

            //2- Fill compiler parameters
            var compilerParameters = new CompilerParameters();
            compilerParameters.ReferencedAssemblies.Add("system.dll");
            compilerParameters.ReferencedAssemblies.Add("TerrainGenCore.dll");

            compilerParameters.GenerateExecutable = false;
            compilerParameters.GenerateInMemory = true;

            //3- Compile the script
            var compilerResults = codeDomProvider.CompileAssemblyFromSource(compilerParameters, code);

            //4- Check if compilation has errors
            if (compilerResults.Errors.HasErrors)
            {
                var errorMessage = "Compilation failed.\r\n";
                foreach (var error in compilerResults.Output)
                    errorMessage += error + "\r\n";

                Lumberjack.Error(errorMessage);
                return false;
            }

            //5- Get the generated assembly
            var compiledAssembly = compilerResults.CompiledAssembly;

            //Create an instace of the class that contains the script method
            _generatorInstance = compiledAssembly.CreateInstance("TerrainGenerator");

            if (_generatorInstance is null)
                return false;

            var typeInfo = _generatorInstance.GetType();

            _methodGetValue = typeInfo.GetMethod("GetTerrain");
            _methodGetTree = typeInfo.GetMethod("GetTree");
            _methodGetWaterLevel = typeInfo.GetMethod("GetWaterLevel");

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

        public TreeType GetTree(int x, int y, int z)
        {
            return TreeType.None;
//            if (_methodGetValue is null)
//                return TreeType.None;
//            return (TreeType)_methodGetValue.Invoke(_generatorInstance, new object[] { x, y, z });
        }

        public int GetWaterLevel()
        {
            if (_methodGetTree is null)
                return 0;
            return (int)_methodGetWaterLevel.Invoke(_generatorInstance, null);
        }
    }
}

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sys.Compiler
{
    public class CSharpCompiler
    {
        private CompilerResults results;

        public CSharpCompiler()
        {
        }

        public void Compile(string assemblyName, params string[] sources)
        {
            CompilerParameters parameters = new CompilerParameters();
            parameters.GenerateExecutable = false;
            parameters.GenerateInMemory = true;
            parameters.OutputAssembly = assemblyName;

            CodeDomProvider icc = CodeDomProvider.CreateProvider("CSharp");
            this.results = icc.CompileAssemblyFromSource(parameters, sources);
        }

        public bool HasError => results.Errors.Count > 0;

        public string GetError()
        {
            if (results.Errors.Count > 0)
            {
                StringBuilder builder = new StringBuilder();
                foreach (CompilerError CompErr in results.Errors)
                {
                    builder.AppendLine($"Line number {CompErr.Line}, Error Number: {CompErr.ErrorNumber}, \"{CompErr.ErrorText}\"");
                }

                return builder.ToString();
            }

            return null;
        }

        public Assembly GetAssembly()
        {
            return results.CompiledAssembly;
        }
    }
}

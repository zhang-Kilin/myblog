using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.IO;

namespace Ctrip.Framework.MVC.CodeDom
{
    public class DynamicCreateDLL
    {
        private string m_AssemblyName = null;
        public DynamicCreateDLL(string assemblyName)
        {
            this.m_AssemblyName = assemblyName;
        }

        public DynamicCreateDLL() : this(null) { }

        /// <summary>
        /// 生成程序集的名称(全路径)
        /// </summary>
        public string OutputAssemblyName
        {
            get
            {
                return m_AssemblyName;
            }
        }

        /// <summary>
        /// 动态创建dll程序集
        /// </summary>
        /// <param name="classSources"></param>
        public CompilerResults Create(string[] classSources, string[] referenceLibs)
        {
            CompilerResults result = null;

            CSharpCodeProvider provider = new CSharpCodeProvider();
            CompilerParameters param = new CompilerParameters(referenceLibs);

            param.GenerateInMemory = true;
            param.GenerateExecutable = false;
            param.TreatWarningsAsErrors = false;

            if (!string.IsNullOrWhiteSpace(m_AssemblyName))
            {
                param.OutputAssembly = m_AssemblyName;
            }

            result = provider.CompileAssemblyFromSource(param, classSources);

            m_AssemblyName = param.OutputAssembly;

            return result;
        }

    }
}
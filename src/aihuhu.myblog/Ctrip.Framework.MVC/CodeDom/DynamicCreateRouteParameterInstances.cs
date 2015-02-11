using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Ctrip.Framework.MVC.Configuration;
using System.CodeDom;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Text;
using System.IO;
using Ctrip.Framework.MVC.Properties;
using System.Threading;

namespace Ctrip.Framework.MVC.CodeDom
{
    public class DynamicCreateRouteParameterInstances
    {
        /// <summary>
        /// 动态类的命名空间
        /// </summary>
        private static readonly string NAMESPACE_NAME = "_ns_danamic_class";
        /// <summary>
        /// 动态创建类的类名
        /// </summary>
        private static readonly string CLASS_NAME = "DynamicClass";

        /// <summary>
        /// {0}--className   {1}--properties
        /// </summary>
        private const string ClassReflectionStringFormat = "namespace #namespace# { \r\n public class #className#{\r\n #property# \r\n} }";//{0}--className   {1} properties

        private const string PropertyStringFormat = "public object #name# = new { #content# };";

        /// <summary>
        /// 动态创建路由器的参数实例
        /// </summary>
        /// <param name="router"></param>
        /// <returns>key:route name  , value:parameter instance</returns>
        internal IDictionary<string, object> CreateRouteParametersInstance(RouterConfiguration router)
        {
            if (router == null)
            {
                throw new ArgumentNullException("router");
            }

            string[] referenceLibs = GetReferenceLibs(router);

            string classSources = BuildClassSource(router);

            DynamicCreateDLL builder = new DynamicCreateDLL();

            CompilerResults result = builder.Create(new string[] { classSources }, referenceLibs);

            IDictionary<string, object> map = GetInstances(router, result);

            ThreadPool.QueueUserWorkItem(new WaitCallback((obj) =>
            {
                try
                {
                    File.Delete(builder.OutputAssemblyName);
                }
                catch
                {
                }
            }));

            return map;
        }

        private string BuildClassSource(RouterConfiguration router)
        {
            string classSource = null;

            if (router != null)
            {
                string property = string.Empty;
                if (router.Routes != null
                    && router.Routes.Count > 0)
                {
                    List<string> propertyList = new List<string>();
                    foreach (RouteConfiguration item in router.Routes)
                    {
                        property = BuildPropertyString(item);
                        if (!string.IsNullOrWhiteSpace(property))
                        {
                            propertyList.Add(property);
                        }
                    }

                    property = string.Join("\r\n", propertyList.ToArray());
                }

                classSource = ClassReflectionStringFormat.Replace("#namespace#", NAMESPACE_NAME).Replace("#className#", CLASS_NAME).Replace("#property#", property);
            }

            return classSource;
        }


        private IDictionary<string, object> GetInstances(RouterConfiguration router, CompilerResults result)
        {
            IDictionary<string, object> map = new Dictionary<string, object>();

            if (router != null
                && router.Routes != null
                && router.Routes.Count > 0)
            {
                string className = string.Format("{0}.{1}", NAMESPACE_NAME, CLASS_NAME);

                Type type = result.CompiledAssembly.GetType(className);
                if (type != null)
                {
                    object instance = type.GetConstructor(Type.EmptyTypes).Invoke(null);
                    if (instance != null)
                    {
                        foreach (RouteConfiguration item in router.Routes)
                        {
                            if (item != null
                                && !string.IsNullOrWhiteSpace(item.Name))
                            {
                                FieldInfo fi = type.GetField(item.Name);
                                object obj = fi.GetValue(instance);
                                map[item.Name] = obj;
                            }
                        }
                    }
                }
            }

            return map;
        }

        private string BuildPropertyString(RouteConfiguration route)
        {
            string result = string.Empty;
            if (route != null
                && route.Parameter != null
                && route.Parameter.Parameters != null
                && route.Parameter.Parameters.Count > 0)
            {
                StringBuilder sb = new StringBuilder(500);
                string propertyString = null;
                ParameterConfiguraiton parameter = null;
                for (int i = 0; i < route.Parameter.Parameters.Count; i++)
                {
                    parameter = route.Parameter.Parameters[i];
                    if (parameter == null
                        || string.IsNullOrWhiteSpace(parameter.Name))
                    {
                        continue;
                    }
                    if (parameter.Optional)
                    {
                        propertyString = string.Format("{0} = {1}", parameter.Name, "System.Web.Mvc.UrlParameter.Optional");
                    }
                    else
                    {
                        if (typeof(string) == Type.GetType(parameter.Type))
                        {
                            propertyString = string.Format("{0} = \"{1}\"", parameter.Name, parameter.Value);
                        }
                        else if (typeof(char) == Type.GetType(parameter.Type))
                        {
                            propertyString = string.Format("{0} = '{1}'", parameter.Name, parameter.Value);
                        }
                        else
                        {
                            if (string.IsNullOrWhiteSpace(parameter.Value))
                            {
                                continue;
                            }

                            propertyString = string.Format("{0} = {1}", parameter.Name, parameter.Value);
                        }
                    }
                    sb.Append(propertyString);
                    sb.Append(",");
                }

                result = PropertyStringFormat.Replace("#name#", route.Name).Replace("#content#", sb.ToString().Remove(sb.Length - 1, 1));
            }
            return result;
        }

        private string[] GetReferenceLibs(RouterConfiguration router)
        {
            string[] result = null;
            if (router.ReferenceLibs == null
                || router.ReferenceLibs.Count == 0)
            {
                result = new string[0];
            }
            else
            {
                List<string> list = new List<string>(router.ReferenceLibs.Count * 2);
                string path = null;
                for (int i = 0; i < router.ReferenceLibs.Count; i++)
                {
                    if (router.ReferenceLibs[i] == null
                        || string.IsNullOrWhiteSpace(router.ReferenceLibs[i].Path))
                    {
                        continue;
                    }

                    path = router.ReferenceLibs[i].Path;

                    path = Utility.ConvertToFullPath(path, router.BaseDirectory);
                    list.Add(path);
                }
                result = list.ToArray();
            }

            return result;
        }




        //public static object Create(ParameterConfigurationCollection fields)
        //{
        //    return Test(fields);
        //    return null;

        //    if (fields == null)
        //    {
        //        throw new ArgumentNullException("fields");
        //    }
        //    if (fields.Parameters == null
        //        || fields.Parameters.Count == 0)
        //    {
        //        throw new ArgumentException("fields parameters is null or empty. ", "fields");
        //    }

        //    string className = string.Format(ClassNameFormat, CLASS_INDEXER++);
        //    CSharpCodeProvider provider = new CSharpCodeProvider();
        //    CompilerParameters param = new CompilerParameters(new string[] { "System.dll", });
        //    param.ReferencedAssemblies.Add("System.dll");
        //    param.ReferencedAssemblies.Add("System.Web.dll");
        //    param.ReferencedAssemblies.Add("System.Web.Mvc.dll");

        //    param.GenerateInMemory = false;
        //    param.GenerateExecutable = false;
        //    param.TreatWarningsAsErrors = false;
        //    param.OutputAssembly = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "test.dll");

        //    string propertyString = BuildPropertyReflectionString(fields.Parameters);
        //    string source = ClassReflectionStringFormat.Replace("#className#", className).Replace("#property#", propertyString);

        //    CompilerResults results = provider.CompileAssemblyFromSource(param, source);

        //    Type type = results.CompiledAssembly.GetType(string.Format("_ns_danamic_class.{0}", className));

        //    Object obj = type.GetConstructor(Type.EmptyTypes).Invoke(null);

        //    return obj;
        //}

        //private static object Test(ParameterConfigurationCollection fields)
        //{
        //    string className = string.Format(ClassNameFormat, CLASS_INDEXER++);
        //    CSharpCodeProvider provider = new CSharpCodeProvider();
        //    CompilerParameters param = new CompilerParameters();
        //    param.ReferencedAssemblies.Add("System.dll");
        //    param.ReferencedAssemblies.Add("System.Web.dll");
        //    param.ReferencedAssemblies.Add(@"C:\Program Files (x86)\Microsoft ASP.NET\ASP.NET MVC 3\Assemblies\System.Web.Mvc.dll");
        //    param.ReferencedAssemblies.Add("System.Web.Routing.dll");

        //    param.GenerateInMemory = true;
        //    param.GenerateExecutable = false;
        //    param.TreatWarningsAsErrors = false;
        //    param.OutputAssembly = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CodeDom", "_ns_danamic_class.dll");

        //    string source = Resources.MvcClassReflectorFormater;
        //    string propertyString = BuildPropertyReflectionString(fields.Parameters);
        //    source = source.Replace("#namespace#", "_ns_danamic_class")
        //                    .Replace("#classname#", className)
        //                    .Replace("#property#", propertyString);

        //    CompilerResults results = provider.CompileAssemblyFromSource(param, source);

        //    Type type = results.CompiledAssembly.GetType("_ns_danamic_class." + className);

        //    Object obj = type.GetConstructor(Type.EmptyTypes).Invoke(null);

        //    FieldInfo fi = type.GetField("Value");

        //    object result = fi.GetValue(obj);

        //    return result;
        //}

        //private static string BuildPropertyReflectionString(List<ParameterConfiguraiton> parameters)
        //{
        //    string pre = "public object Value = new { #content# };";

        //    StringBuilder sb = new StringBuilder(500);
        //    string propertyString = null;
        //    foreach (ParameterConfiguraiton parameter in parameters)
        //    {
        //        if (parameter.Optional)
        //        {
        //            propertyString = string.Format("{0} = {1}", parameter.Name, "System.Web.Mvc.UrlParameter.Optional");
        //        }
        //        else
        //        {
        //            if (typeof(string) == Type.GetType(parameter.Type))
        //            {
        //                propertyString = string.Format("{0} = \"{1}\"", parameter.Name, parameter.Value);
        //            }
        //            else
        //            {
        //                propertyString = string.Format("{0} = {1}", parameter.Name, parameter.Value);
        //            }
        //        }
        //        sb.Append(propertyString);
        //        sb.Append(",");
        //    }

        //    string result = pre.Replace("#content#", sb.ToString().Remove(sb.Length - 1, 1));

        //    return result;
        //}
    }
}
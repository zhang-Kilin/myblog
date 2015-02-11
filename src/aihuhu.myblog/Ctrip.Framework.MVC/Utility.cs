using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Text.RegularExpressions;

namespace Ctrip.Framework.MVC
{
    internal class Utility
    {
        /// <summary>
        /// 反序列化xml
        /// </summary>
        /// <param name="type"></param>
        /// <param name="xml"></param>
        /// <returns></returns>
        public static object XmlDeserialize(Type type, string xml)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            if (string.IsNullOrWhiteSpace(xml))
            {
                return null;
            }

            byte[] buffer = Encoding.UTF8.GetBytes(xml);
            object result = null;
            using (MemoryStream stream = new MemoryStream(buffer))
            {
                XmlSerializer ser = new XmlSerializer(type);
                result = ser.Deserialize(stream);
            }

            return result;
        }

        /// <summary>
        /// 反序列化xml
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xml"></param>
        /// <returns></returns>
        public static T XmlDeserialize<T>(string xml)
        {
            Type type = typeof(T);
            T result = (T)XmlDeserialize(type, xml);
            return result;
        }

        /// <summary>
        /// 获取全路径
        /// </summary>
        /// <param name="path">虚拟路径</param>
        /// <returns></returns>
        public static string ConvertToFullPath(string path)
        {
            return ConvertToFullPath(path, AppDomain.CurrentDomain.BaseDirectory);
        }

        /// <summary>
        /// 获取全路径
        /// </summary>
        /// <param name="path">虚拟路径</param>
        /// <param name="baseDirectory">根目录</param>
        /// <returns></returns>
        public static string ConvertToFullPath(string path, string baseDirectory)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return path;
            }

            if (Regex.IsMatch(path, "^~?/"))
            {
                path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path.TrimStart('~', '/'));
            }

            baseDirectory = string.IsNullOrWhiteSpace(baseDirectory) ? AppDomain.CurrentDomain.BaseDirectory : baseDirectory;

            if (!Path.IsPathRooted(baseDirectory))
            {
                baseDirectory = ConvertToFullPath(baseDirectory);
            }


            if (!Path.IsPathRooted(path))
            {
                path = Path.Combine(baseDirectory, path);
            }

            path = path.Replace("/", "\\");

            return path;
        }

        /// <summary>
        /// 获取url全路径 , url不能包含host信息
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string ConvertToFullUrl(string url)
        {
            return ConvertToFullUrl(url, "/");
        }

        /// <summary>
        /// 获取url全路径 , url & baseUrl不能包含host信息
        /// \n如果url为空，则直接返回baseUrl
        /// </summary>
        /// <param name="url"></param>
        /// <param name="baseUrl"></param>
        /// <returns></returns>
        public static string ConvertToFullUrl(string url, string baseUrl)
        {
            //如果url为空，则返回baseUrl
            if (string.IsNullOrWhiteSpace(url))
            {
                return baseUrl;
            }
            url = url.Replace("\\", "/");

            //如果是以某种协议开头，则表明为协议地址，直接返回即可
            if (Regex.IsMatch(url, "^[a-z]+:", RegexOptions.IgnoreCase))
            {
                return url;
            }

            //如果是以 ~/ 开头，则是根目录，去除～即可
            if (Regex.IsMatch(url, "^~?/"))
            {
                return url.TrimStart('~');
            }

            baseUrl = (baseUrl ?? "").Replace('\\', '/');
            string[] p1 = url.Split('?');
            string[] p2 = baseUrl.Split('?');
            string param1 = p1.Length > 1 ? p1[1] : null;
            url = p1[0];
            baseUrl = p2[0];//baseUrl忽略参数
            baseUrl = baseUrl.TrimStart('~', '/').TrimEnd('/');

            string[] arr = url.Split('/');
            List<string> arr1 = null;

            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                arr1 = new List<string>(50);
            }
            else
            {
                arr1 = baseUrl.Split('/').ToList();
            }

            foreach (string item in arr)
            {
                if (item == ".."
                    && arr1.Count > 0)
                {
                    arr1.RemoveAt(arr1.Count - 1);
                }
                else if (item == ".")
                {
                    continue;
                }
                else
                {
                    arr1.Add(item);
                }
            }

            string result = string.Join("/", arr1.ToArray()).TrimStart('/', ' ');

            return string.Format("/{0}{1}", result, param1 != null ? string.Format("?{0}", param1) : null);
        }
    }
}
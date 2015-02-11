using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Ctrip.Framework.MVC.Configuration;
using System.Web.Mvc;

namespace Ctrip.Framework.MVC
{
    public class IncludeHelper
    {
        private const string JAVASCRIPT = "<script type=\"text/javascript\" src=\"{0}\"></script>";
        private const string STYLE = "<link type=\"text/css\" rel=\"stylesheet\" href=\"{0}\" />";

        private static Random m_Random = new Random();

        public static MvcHtmlString Include(string name)
        {
            Source source = SourceManager.Sources[name];

            if (source == null)
            {
                return MvcHtmlString.Empty;
            }

            string output = BuildPath(source);

            return MvcHtmlString.Create(output);
        }

        private static string BuildPath(Source source)
        {
            string result = Utility.ConvertToFullUrl(source.FileName, SourceManager.BasePath);

            if (!string.IsNullOrWhiteSpace(result))
            {
                result = string.Format("{0}{1}={2}", result, GetParamOption(result), GetVersion(source.Version));
            }

            switch (source.Type)
            {
                case SourceType.js:
                    result = string.Format(JAVASCRIPT, result);
                    break;
                case SourceType.css:
                    result = string.Format(STYLE, result);
                    break;
                default:
                    result = string.Empty;
                    break;
            }

            return result;
        }

        private static string GetParamOption(string path)
        {
            string pre = "?";
            if (path.IndexOf("?") >= 0)
            {
                pre = "&";
            }
            return pre + "v";
        }

        private static string GetVersion(string version)
        {
            if (string.IsNullOrWhiteSpace(version))
            {
                version = m_Random.Next(100000, 999999).ToString();
            }
            return version;
        }
    }
}
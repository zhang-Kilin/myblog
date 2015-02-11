using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Collections.ObjectModel;
using System.Xml;
using System.IO;

namespace Ctrip.Framework.MVC.Configuration
{
    [Serializable, XmlRoot("route")]
    public class RouteConfiguration
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("url")]
        public string Url { get; set; }

        //[XmlAttribute("controller")]
        //public string Controller { get; set; }

        //[XmlAttribute("action")]
        //public string Action { get; set; }

        [XmlElement("params")]
        public ParameterConfigurationCollection Parameter { get; set; }
    }

    public class RouteConfigurationCollection : Collection<RouteConfiguration>
    {
        private IDictionary<string, RouteConfiguration> m_Mapping = new Dictionary<string, RouteConfiguration>(StringComparer.OrdinalIgnoreCase);

        public void Add(string name, RouteConfiguration route)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException("name");
            }
            if (route == null)
            {
                throw new ArgumentNullException("route");
            }

            if (this.m_Mapping.ContainsKey(name))
            {
                throw new ArgumentException(string.Format("already exists key '{0}'. ", name), "name");
            }

            base.Add(route);
            this.m_Mapping[name] = route;
        }

        public RouteConfiguration this[string name]
        {
            get
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return null;
                }
                RouteConfiguration route;
                if (this.m_Mapping.TryGetValue(name, out route))
                {
                    return route;
                }
                return null;
            }
        }

        public void Remove(string name)
        {
            RouteConfiguration route = this[name];
            if (route == null)
            {
                return;
            }
            this.Remove(route);
            this.m_Mapping.Remove(name);
        }

        protected override void ClearItems()
        {
            base.ClearItems();
            this.m_Mapping.Clear();
        }

        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);
            this.RemoveRouteName(index);
        }

        protected override void InsertItem(int index, RouteConfiguration item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            if (base.Contains(item))
            {
                throw new ArgumentException("alreay exists item. ", "item");
            }
            base.InsertItem(index, item);
        }

        protected override void SetItem(int index, RouteConfiguration item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            if (base.Contains(item))
            {
                throw new ArgumentException("already exists item. ", "item");
            }
            this.RemoveRouteName(index);
            base.SetItem(index, item);
        }

        private void RemoveRouteName(int index)
        {
            string name = GetName(index);
            if (!string.IsNullOrWhiteSpace(name))
            {
                this.m_Mapping.Remove(name);
            }
        }

        private string GetName(int index)
        {
            RouteConfiguration route = base[index];
            string name = null;
            foreach (KeyValuePair<string, RouteConfiguration> current in this.m_Mapping)
            {
                if (current.Value == route)
                {
                    name = current.Key;
                    break;
                }
            }
            return name;
        }
    }

    internal sealed class RouterConfiguration
    {
        private IgnoreRouteConfigurationCollection m_IgnoreRoutes;
        private RouteConfigurationCollection m_Routes;
        private ReferenceLibConfigurationCollection m_ReferenceLibs;
        private string m_BaseDirectory;
        private string m_BasePath;
        private string m_FileName;

        /// <summary>
        /// router配置文件所在目录 ,  默认为应用程序根目录
        /// </summary>
        public string BaseDirectory
        {
            get
            {
                return m_BaseDirectory;
            }
        }

        /// <summary>
        /// 配置文件所在地根目录
        /// </summary>
        public string BasePath
        {
            get
            {
                return m_BasePath;
            }
        }

        /// <summary>
        /// 配置文件名
        /// </summary>
        public string FileName
        {
            get
            {
                return m_FileName;
            }
        }

        /// <summary>
        /// 忽略掉的url路由器集合
        /// </summary>
        public IgnoreRouteConfigurationCollection IgnoreRoutes
        {
            get
            {
                return m_IgnoreRoutes;
            }
        }

        /// <summary>
        /// 路由器集合
        /// </summary>
        public RouteConfigurationCollection Routes
        {
            get
            {
                return m_Routes;
            }
        }

        /// <summary>
        /// 外部引用的dll配置
        /// </summary>
        public ReferenceLibConfigurationCollection ReferenceLibs
        {
            get
            {
                return m_ReferenceLibs;
            }
        }

        public RouterConfiguration(string configFilePath)
        {
            if (string.IsNullOrWhiteSpace(configFilePath))
            {
                throw new ArgumentNullException("filePath");
            }

            configFilePath = Utility.ConvertToFullPath(configFilePath);

            if (!File.Exists(configFilePath))
            {
                throw new FileNotFoundException(string.Format("can not found file '{0}'. ", configFilePath), configFilePath);
            }

            this.m_BaseDirectory = Path.GetDirectoryName(configFilePath);
            string directory = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);

            directory = this.m_BaseDirectory.Replace(directory, string.Empty);

            this.m_BasePath = string.IsNullOrEmpty(directory) ? "/" : directory.Replace("\\", "/");
            this.m_FileName = Path.GetFileName(configFilePath);

            XmlDocument dom = new XmlDocument();
            dom.Load(configFilePath);
            XmlElement element = dom.DocumentElement;

            Init(element);
        }

        public RouterConfiguration(XmlElement element)
        {
            this.m_BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            this.m_BasePath = "/";
            Init(element);
        }

        private void Init(XmlElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            XmlNode node = element.SelectSingleNode("referenceLibs");
            InitReferenceLibs(node);

            node = element.SelectSingleNode("ignoreRoutes");
            InitIgnoreRoutes(node);

            node = element.SelectSingleNode("routes");
            InitRoutes(node);
        }

        private void InitReferenceLibs(XmlNode root)
        {
            this.m_ReferenceLibs = new ReferenceLibConfigurationCollection();
            if (root != null)
            {
                string xml;
                ReferenceLibConfiguration lib;
                foreach (XmlNode item in root.ChildNodes)
                {
                    if (item.NodeType != XmlNodeType.Element)
                    {
                        continue;
                    }
                    xml = item.OuterXml;
                    lib = Utility.XmlDeserialize<ReferenceLibConfiguration>(xml);
                    this.m_ReferenceLibs.Add(lib);
                }
            }
        }

        private void InitIgnoreRoutes(XmlNode root)
        {
            this.m_IgnoreRoutes = new IgnoreRouteConfigurationCollection();
            if (root != null)
            {
                string xml;
                IgnoreRouteConfiguration route;
                foreach (XmlNode item in root.ChildNodes)
                {
                    if (item.NodeType != XmlNodeType.Element)
                    {
                        continue;
                    }
                    xml = item.OuterXml;
                    route = Utility.XmlDeserialize<IgnoreRouteConfiguration>(xml);
                    this.m_IgnoreRoutes.Add(route.Path, route);
                }
            }
        }

        private void InitRoutes(XmlNode root)
        {
            this.m_Routes = new RouteConfigurationCollection();
            if (root != null)
            {
                string xml;
                RouteConfiguration route;
                foreach (XmlNode item in root.ChildNodes)
                {
                    if (item.NodeType != XmlNodeType.Element)
                    {
                        continue;
                    }
                    xml = item.OuterXml;
                    route = Utility.XmlDeserialize<RouteConfiguration>(xml);
                    this.m_Routes.Add(route.Name, route);
                }
            }
        }
    }
}

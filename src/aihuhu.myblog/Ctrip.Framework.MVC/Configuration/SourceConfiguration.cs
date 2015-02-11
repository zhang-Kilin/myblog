using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using System.Collections.Specialized;
using System.Collections;

namespace Ctrip.Framework.MVC.Configuration
{
    [Serializable]
    public enum SourceType
    {
        js,
        css
    }

    [XmlRoot("source")]
    public class Source
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("file")]
        public string FileName { get; set; }

        [XmlAttribute("version")]
        public string Version { get; set; }

        [XmlAttribute("type")]
        public SourceType Type { get; set; }
    }

    public class SourceCollection : NameObjectCollectionBase
    {
        public SourceCollection() : base() { }

        public SourceCollection(int capacity) : base(capacity) { }

        public Source this[string name]
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(name))
                {
                    name = name.ToUpper();
                }
                return this.BaseGet(name) as Source;
            }
            set
            {
                if (!string.IsNullOrWhiteSpace(name))
                {
                    name = name.ToUpper();
                }
                this.BaseSet(name, value);
            }
        }

        public Source this[int index]
        {
            get
            {
                return this.BaseGet(index) as Source;
            }
            set
            {
                this.BaseSet(index, value);
            }
        }

        public void Add(string name, Source package)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                name = name.ToUpper();
            }
            this.BaseAdd(name, package);
        }

        public void Remove(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                name = name.ToUpper();
            }
            this.BaseRemove(name);
        }

        public void Clear()
        {
            this.BaseClear();
        }

        public new IEnumerator<Source> GetEnumerator()
        {
            IEnumerator rator = base.GetEnumerator();

            return new SourceEnumrator(rator, this);
        }

        public class SourceEnumrator : IEnumerator<Source>
        {
            private IEnumerator m_Rator = null;
            private SourceCollection m_Collection = null;
            internal SourceEnumrator(IEnumerator rator, SourceCollection collection)
            {
                this.m_Rator = rator;
                this.m_Collection = collection;
            }

            public Source Current
            {
                get
                {
                    string key = m_Rator.Current as string;

                    return m_Collection[key];
                }
            }

            public void Dispose()
            {
                this.m_Rator = null;
                this.m_Collection = null;
            }

            object IEnumerator.Current
            {
                get
                {
                    return this.Current;
                }
            }

            public bool MoveNext()
            {
                return m_Rator.MoveNext();
            }

            public void Reset()
            {
                this.m_Rator.Reset();
            }
        }
    }

    internal sealed class SourceConfiguration
    {
        private SourceCollection m_Collection;
        private string m_BaseDirectory;
        private string m_BasePath;
        private string m_FileName;

        /// <summary>
        /// 所有的资源package配置
        /// </summary>
        public SourceCollection Sources
        {
            get
            {
                return m_Collection;
            }
        }

        /// <summary>
        /// package配置文件所在目录 ,  默认为应用程序根目录
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

        public SourceConfiguration(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentNullException("filePath");
            }

            filePath = Utility.ConvertToFullPath(filePath);

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(string.Format("can not found file '{0}'. ", filePath), filePath);
            }

            this.m_BaseDirectory = Path.GetDirectoryName(filePath);
            string directory = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);

            directory = this.m_BaseDirectory.Replace(directory, string.Empty);

            this.m_BasePath = string.IsNullOrEmpty(directory) ? "/" : directory.Replace("\\", "/");
            this.m_FileName = Path.GetFileName(filePath);

            XmlDocument dom = new XmlDocument();
            dom.Load(filePath);
            XmlElement element = dom.DocumentElement;

            Init(element);
        }

        public SourceConfiguration(XmlElement element)
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
            if (m_Collection != null)
            {
                lock (m_Collection)
                {
                    m_Collection.Clear();
                    Init(m_Collection, element);
                }
            }
            else
            {
                m_Collection = new SourceCollection(500);
                Init(m_Collection, element);
            }
        }

        private void Init(SourceCollection collection, XmlElement element)
        {
            XmlNodeList nodes = element.SelectNodes("source");
            Source source = null;

            foreach (XmlNode item in nodes)
            {
                if (item.NodeType != XmlNodeType.Element)
                {
                    continue;
                }
                source = Resolve(item);
                if (source != null)
                {
                    collection.Add(source.Name, source);
                }
            }
        }

        private Source Resolve(XmlNode node)
        {
            string xml = node.OuterXml;

            Source result = Utility.XmlDeserialize<Source>(xml);

            return result;
        }
    }

}
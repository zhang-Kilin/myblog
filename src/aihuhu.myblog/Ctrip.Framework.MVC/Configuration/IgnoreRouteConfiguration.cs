using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Collections.ObjectModel;

namespace Ctrip.Framework.MVC.Configuration
{
    [Serializable,XmlRoot("igroute")]
    public class IgnoreRouteConfiguration
    {
        [XmlAttribute("path")]
        public string Path { get; set; }
    }

    public class IgnoreRouteConfigurationCollection:Collection<IgnoreRouteConfiguration>
    {
        private IDictionary<string, IgnoreRouteConfiguration> m_Mapping = new Dictionary<string, IgnoreRouteConfiguration>(StringComparer.OrdinalIgnoreCase);

        public void Add(string name, IgnoreRouteConfiguration route)
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

        public IgnoreRouteConfiguration this[string name]
        {
            get
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return null;
                }
                IgnoreRouteConfiguration route;
                if (this.m_Mapping.TryGetValue(name, out route))
                {
                    return route;
                }
                return null;
            }
        }

        public void Remove(string name)
        {
            IgnoreRouteConfiguration route = this[name];
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

        protected override void InsertItem(int index, IgnoreRouteConfiguration item)
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

        protected override void SetItem(int index, IgnoreRouteConfiguration item)
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
            IgnoreRouteConfiguration route = base[index];
            string name = null;
            foreach (KeyValuePair<string, IgnoreRouteConfiguration> current in this.m_Mapping)
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
}

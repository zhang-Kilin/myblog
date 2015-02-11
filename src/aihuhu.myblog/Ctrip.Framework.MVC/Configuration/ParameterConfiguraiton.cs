using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace Ctrip.Framework.MVC.Configuration
{
    [XmlRoot("param"),Serializable]
    public class ParameterConfiguraiton
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("value")]
        public string Value { get; set; }

        [XmlAttribute("type")]
        public string Type { get; set; }

        [XmlAttribute("optional")]
        public bool Optional { get; set; }
    }

    [XmlRoot("params"),Serializable]
    public class ParameterConfigurationCollection
    {
        [XmlElement("param")]
        public List<ParameterConfiguraiton> Parameters { get; set; }
    }
}
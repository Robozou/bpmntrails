using System.Xml.Serialization;

namespace bpmntrails
{
    [XmlRoot(ElementName = "waypoint", Namespace = "http://www.omg.org/spec/DD/20100524/DI")]
    public class Waypoint
    {
        [XmlAttribute(AttributeName = "x")]
        public string X { get; set; }
        [XmlAttribute(AttributeName = "y")]
        public string Y { get; set; }
    }
}
using System.Xml.Serialization;

namespace bpmntrails
{
    [XmlRoot(ElementName = "Bounds", Namespace = "http://www.omg.org/spec/DD/20100524/DC")]
    public class Bounds
    {
        [XmlAttribute(AttributeName = "height")]
        public string Height { get; set; }
        [XmlAttribute(AttributeName = "width")]
        public string Width { get; set; }
        [XmlAttribute(AttributeName = "x")]
        public string X { get; set; }
        [XmlAttribute(AttributeName = "y")]
        public string Y { get; set; }
    }
}
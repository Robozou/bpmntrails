using System.Xml.Serialization;

namespace bpmntrails
{
    [XmlRoot(ElementName = "BPMNShape", Namespace = "http://www.omg.org/spec/BPMN/20100524/DI")]
    public class BPMNShape
    {
        [XmlElement(ElementName = "Bounds", Namespace = "http://www.omg.org/spec/DD/20100524/DC")]
        public Bounds bounds = new Bounds();
        [XmlAttribute(AttributeName = "bpmnElement")]
        public string bpmnElement { get; set; }
        [XmlAttribute(AttributeName = "id")]
        public string id { get; set; }
    }
}
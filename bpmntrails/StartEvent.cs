using System.Collections.Generic;
using System.Xml.Serialization;

namespace bpmntrails
{
    [XmlRoot(ElementName = "startEvent", Namespace = "http://www.omg.org/spec/BPMN/20100524/MODEL")]
    public class StartEvent
    {
        [XmlAttribute(AttributeName = "id")]
        public string id { get; set; }
        [XmlElement(ElementName = "outgoing", Namespace = "http://www.omg.org/spec/BPMN/20100524/MODEL")]
        public List<string> outgoing = new List<string>();
    }
}
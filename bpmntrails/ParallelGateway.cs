using System.Collections.Generic;
using System.Xml.Serialization;

namespace bpmntrails
{
    [XmlRoot(ElementName = "parallelGateway", Namespace = "http://www.omg.org/spec/BPMN/20100524/MODEL")]
    public class ParallelGateway
    {
        [XmlElement(ElementName = "incoming", Namespace = "http://www.omg.org/spec/BPMN/20100524/MODEL")]
        public List<string> incoming = new List<string>();
        [XmlElement(ElementName = "outgoing", Namespace = "http://www.omg.org/spec/BPMN/20100524/MODEL")]
        public List<string> outgoing = new List<string>();
        [XmlAttribute(AttributeName = "id")]
        public string id { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string name { get; set; }
        [XmlAttribute(AttributeName = "gatewayDirection")]
        public string gatewayDirection { get; set; }
    }
}
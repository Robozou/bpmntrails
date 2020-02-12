using System.Collections.Generic;
using System.Xml.Serialization;

namespace bpmntrails
{
    [XmlRoot(ElementName = "BPMNEdge", Namespace = "http://www.omg.org/spec/BPMN/20100524/DI")]
    public class BPMNEdge : BPMNElement
    {
        [XmlElement(ElementName = "waypoint", Namespace = "http://www.omg.org/spec/DD/20100524/DI")]
        public List<Waypoint> waypoint = new List<Waypoint>();
        [XmlAttribute(AttributeName = "bpmnElement")]
        public string bpmnElement { get; set; }
    }
}
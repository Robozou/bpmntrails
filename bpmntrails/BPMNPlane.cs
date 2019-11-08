using System.Collections.Generic;
using System.Xml.Serialization;

namespace bpmntrails
{
    [XmlRoot(ElementName = "BPMNPlane", Namespace = "http://www.omg.org/spec/BPMN/20100524/DI")]
    public class BPMNPlane
    {
        [XmlElement(ElementName = "BPMNShape", Namespace = "http://www.omg.org/spec/BPMN/20100524/DI")]
        public List<BPMNShape> bpmnShapes = new List<BPMNShape>();
        [XmlElement(ElementName = "BPMNEdge", Namespace = "http://www.omg.org/spec/BPMN/20100524/DI")]
        public List<BPMNEdge> bpmnEdges = new List<BPMNEdge>();
        [XmlAttribute(AttributeName = "bpmnElement")]
        public string bpmnElement { get; set; }
        [XmlAttribute(AttributeName = "id")]
        public string id { get; set; }
    }
}
using System.Xml.Serialization;

namespace bpmntrails
{
    [XmlRoot(ElementName = "sequenceFlow", Namespace = "http://www.omg.org/spec/BPMN/20100524/MODEL")]
    public class SequenceFlow
    {
        [XmlAttribute(AttributeName = "id")]
        public string id { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string name { get; set; }
        [XmlAttribute(AttributeName = "sourceRef")]
        public string sourceRef { get; set; }
        [XmlAttribute(AttributeName = "targetRef")]
        public string targetRef { get; set; }
    }
}
using System.Collections.Generic;
using System.Xml.Serialization;

namespace bpmntrails
{
    [XmlRoot(ElementName = "endEvent", Namespace = "http://www.omg.org/spec/BPMN/20100524/MODEL")]
    public class EndEvent : BPMNElement
    {
        [XmlElement(ElementName = "incoming", Namespace = "http://www.omg.org/spec/BPMN/20100524/MODEL")]
        public List<string> incoming = new List<string>();
    }
}
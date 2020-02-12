using System.Collections.Generic;
using System.Xml.Serialization;

namespace bpmntrails
{
    [XmlRoot(ElementName = "process", Namespace = "http://www.omg.org/spec/BPMN/20100524/MODEL")]
    public class Process : BPMNElement
    {
        [XmlElement(ElementName = "startEvent", Namespace = "http://www.omg.org/spec/BPMN/20100524/MODEL")]
        public List<StartEvent> startEvents = new List<StartEvent>();
        [XmlElement(ElementName = "task", Namespace = "http://www.omg.org/spec/BPMN/20100524/MODEL")]
        public List<Task> tasks = new List<Task>();
        [XmlElement(ElementName = "endEvent", Namespace = "http://www.omg.org/spec/BPMN/20100524/MODEL")]
        public List<EndEvent> endEvents = new List<EndEvent>();
        [XmlElement(ElementName = "sequenceFlow", Namespace = "http://www.omg.org/spec/BPMN/20100524/MODEL")]
        public List<SequenceFlow> sequenceFlows = new List<SequenceFlow>();
        [XmlElement(ElementName = "parallelGateway", Namespace = "http://www.omg.org/spec/BPMN/20100524/MODEL")]
        public List<ParallelGateway> parallelGateways = new List<ParallelGateway>();
        [XmlElement(ElementName = "exclusiveGateway", Namespace = "http://www.omg.org/spec/BPMN/20100524/MODEL")]
        public List<ExclusiveGateway> exclusiveGateways = new List<ExclusiveGateway>();
        [XmlAttribute(AttributeName = "isExecutable")]
        public string isExecutable = "false";
    }
}
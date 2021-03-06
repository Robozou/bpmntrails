﻿using System.Xml.Serialization;

namespace bpmntrails
{
    [XmlRoot(ElementName = "BPMNDiagram", Namespace = "http://www.omg.org/spec/BPMN/20100524/DI")]
    public class Diagram : BPMNElement
    {
        [XmlElement(ElementName = "BPMNPlane", Namespace = "http://www.omg.org/spec/BPMN/20100524/DI")]
        public BPMNPlane bpmnPlane = new BPMNPlane();
    }
}
using System.Xml.Serialization;

namespace bpmntrails
{
    [XmlRoot(ElementName = "definitions", Namespace = "http://www.omg.org/spec/BPMN/20100524/MODEL")]
    public class Definition
    {
        [XmlAttribute(AttributeName = "xmlns")]
        public string xmlns = "http://www.omg.org/spec/BPMN/20100524/MODEL";
        [XmlAttribute(AttributeName = "bpmn", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string bpmn = "http://www.omg.org/spec/BPMN/20100524/MODEL";
        [XmlAttribute(AttributeName = "bpmndi", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string bpmndi = "http://www.omg.org/spec/BPMN/20100524/DI";
        [XmlAttribute(AttributeName = "omgdc", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string omgdc = "http://www.omg.org/spec/DD/20100524/DC";
        [XmlAttribute(AttributeName = "omgdi", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string omgdi = "http://www.omg.org/spec/DD/20100524/DI";
        [XmlAttribute(AttributeName = "xsi", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string xsi = "http://www.w3.org/2001/XMLSchema-instance";
        [XmlAttribute(AttributeName = "exporter")]
        public string exporter = "BMPNTrails";
        [XmlAttribute(AttributeName = "exporterVersion")]
        public string exporterVersion = "1";
        [XmlAttribute(AttributeName = "targetNamespace")]
        public string targetNamespace = "BPMNTrails";
        [XmlAttribute(AttributeName = "schemaLocation", Namespace = "http://www.w3.org/2001/XMLSchema-instance")]
        public string schemaLocation = "http://www.omg.org/spec/BPMN/20100524/MODEL http://www.omg.org/spec/BPMN/2.0/20100501/BPMN20.xsd";
        [XmlElement(ElementName = "process", Namespace = "http://www.omg.org/spec/BPMN/20100524/MODEL")]
        public Process process = new Process();
        [XmlElement(ElementName = "BPMNDiagram", Namespace = "http://www.omg.org/spec/BPMN/20100524/DI")]
        public Diagram diagram = new Diagram();
    }
}

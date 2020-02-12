using System.Xml.Serialization;

namespace bpmntrails
{
    public class BPMNElement
    {
        [XmlAttribute(AttributeName = "id")]
        public string id { get; set; }
    }
}

using System.Xml.Serialization;

namespace bpmntrails
{
    [XmlRoot(ElementName = "DCRID")]
    public class DCRID
    {
        [XmlAttribute(AttributeName = "dcrid")]
        public string dcrid { get; set; }
    }
}
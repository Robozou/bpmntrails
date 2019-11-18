using bpmntrails;
using System.Collections.Generic;
using System.Xml;

namespace DCRReader
{
    public class XMLReader
    {
        // change this to parse and fill out processor
        List<List<EventNode>> trace = new List<List<EventNode>>();
        BPMNTrail trail = new BPMNTrail();
        public void Read()
        {
            XmlDocument xml = new XmlDocument();
            xml.Load("..\\..\\..\\testRead.xml");
            XmlNodeList list = xml.SelectNodes("//trace[@type!='Forbidden']");
            foreach (XmlElement n in list)
            {
                List<EventNode> eventList = new List<EventNode>();
                for (int i = 0; i < n.ChildNodes.Count; i++)
                {
                    eventList.Add(new EventNode { id = n.ChildNodes[i].Attributes["id"].Value, label = n.ChildNodes[i].Attributes["id"].Value });
                }
            }
            Build();
        }

        private void Build()
        {

            foreach (List<EventNode> list in trace)
            {

            }
        }
    }
}

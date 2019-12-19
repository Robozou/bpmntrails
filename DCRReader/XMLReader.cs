using System;
using System.Collections.Generic;
using System.Xml;

namespace DCRReader
{
    public class XMLReader
    {
        // change this to parse and fill out processor
        private List<List<string>> trace;
        private Processor graph;
        private XMLBuilder builder;
        private Dictionary<string, string> idLabel;
        private Dictionary<string, string> labelId;

        public XMLReader()
        {
            trace = new List<List<string>>();
            graph = new Processor();
            builder = null;
            idLabel = new Dictionary<string, string>();
            labelId = new Dictionary<string, string>();
        }

        public void Read(string fileLoc, bool optimize = false)
        {
            try
            {
                ReadTraces();
                ReadGraph();
                Build(optimize);
                Print(fileLoc);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Print(string fileLoc)
        {
            try
            {
                if (builder != null)
                {
                    builder.Print(fileLoc);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void ReadGraph()
        {
            XmlDocument xml = new XmlDocument();
            xml.Load("..\\..\\..\\testGraph.xml");
            XmlNodeList events = xml.SelectNodes("//events//event[not(@type='nesting')]");
            XmlNodeList nestingEvents = xml.SelectNodes("//events//event[@type='nesting']");
            List<string> nestingEventIds = new List<string>();
            foreach (XmlElement n in nestingEvents)
            {
                nestingEventIds.Add(n.Attributes["id"].Value);
            }
            AddEvents(events);
            SetMarkings(xml);
            XmlNodeList conditions = xml.SelectNodes("//constraints/conditions/condition");
            XmlNodeList responses = xml.SelectNodes("//constraints/responses/response");
            XmlNodeList includes = xml.SelectNodes("//constraints/includes/include");
            XmlNodeList excludes = xml.SelectNodes("//constraints/excludes/exclude");
            XmlNodeList milestones = xml.SelectNodes("//constraints/milestones/milestone");
            AddRelations(conditions, nestingEventIds, Processor.cond, xml);
            AddRelations(responses, nestingEventIds, Processor.resp, xml);
            AddRelations(includes, nestingEventIds, Processor.inc, xml);
            AddRelations(excludes, nestingEventIds, Processor.exc, xml);
            AddRelations(milestones, nestingEventIds, Processor.mile, xml);
            graph.Enable();
            graph.Save();
            MakeIdLabelMapping(xml);
        }

        private void MakeIdLabelMapping(XmlDocument xml)
        {
            foreach (XmlElement x in xml.SelectNodes("//labelMapping"))
            {
                idLabel.Add(x.Attributes["eventId"].Value, x.Attributes["labelId"].Value);
                labelId.Add(x.Attributes["labelId"].Value, x.Attributes["eventId"].Value);
            }
        }

        private void AddRelations(XmlNodeList relations, List<string> nestingEventIds, string type, XmlDocument xml)
        {
            foreach (XmlElement r in relations)
            {
                AddRelation(r.Attributes["sourceId"].Value, r.Attributes["targetId"].Value, nestingEventIds, type, xml);
            }
        }

        private void AddRelation(string source, string target, List<string> nestingEventIds, string type, XmlDocument xml)
        {
            if (nestingEventIds.Contains(source) && nestingEventIds.Contains(target))
            {
                XmlNodeList ls = xml.SelectNodes("//events//event[id='" + source + "']");
                XmlNodeList lt = xml.SelectNodes("//events//event[id='" + target + "']");
                foreach (XmlElement s in ls)
                {
                    foreach (XmlElement t in lt)
                    {
                        AddRelation(s.Attributes["id"].Value, t.Attributes["id"].Value, nestingEventIds, type, xml);
                    }
                }

            }
            else if (nestingEventIds.Contains(source))
            {
                XmlNodeList ls = xml.SelectNodes("//events//event[id='" + source + "']");
                foreach (XmlElement s in ls)
                {
                    AddRelation(s.Attributes["id"].Value, target, nestingEventIds, type, xml);
                }
            }
            else if (nestingEventIds.Contains(target))
            {
                XmlNodeList lt = xml.SelectNodes("//events//event[id='" + target + "']");
                foreach (XmlElement t in lt)
                {
                    AddRelation(source, t.Attributes["id"].Value, nestingEventIds, type, xml);
                }
            }
            else
            {
                graph.AddRelation(target, source, type);
            }
        }

        private void AddEvents(XmlNodeList events)
        {
            foreach (XmlElement e in events)
            {
                AddEvent(e);
            }
        }

        private void SetMarkings(XmlDocument xml)
        {
            XmlNodeList pending = xml.SelectNodes("//marking/pendingResponses/event");
            XmlNodeList included = xml.SelectNodes("//marking/included/event");
            XmlNodeList executed = xml.SelectNodes("//marking/executed/event");
            foreach (XmlElement m in pending)
            {
                graph.SetPending(m.Attributes["id"].Value);
            }
            foreach (XmlElement m in included)
            {
                graph.SetIncluded(m.Attributes["id"].Value);
            }
            foreach (XmlElement m in executed)
            {
                graph.SetExecuted(m.Attributes["id"].Value);
            }
        }

        private void AddEvent(XmlElement e)
        {
            graph.AddEvent(e.Attributes["id"].Value);
        }

        private void ReadTraces()
        {
            XmlDocument xml = new XmlDocument();
            xml.Load("..\\..\\..\\testTrace.xml");
            XmlNodeList list = xml.SelectNodes("//trace[not(@type='Forbidden')]");
            foreach (XmlElement n in list)
            {
                List<string> eventList = new List<string>();
                for (int i = 0; i < n.ChildNodes.Count; i++)
                {
                    eventList.Add(n.ChildNodes[i].Attributes["id"].Value);
                }
                trace.Add(eventList);
            }
        }

        private void Build(bool optimize)
        {
            builder = new XMLBuilder(graph, trace, idLabel, labelId);
            builder.Build();
            if (optimize) builder.Optimize();
        }
    }
}

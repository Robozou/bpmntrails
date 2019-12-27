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

        public void Read(string fileLoc, string dcrGraphLoc, string traceListLoc, bool optimize = false)
        {
            try
            {
                ReadTraces(traceListLoc);
            }
            catch (Exception)
            {
                throw;
            }
            try
            {
                ReadGraph(dcrGraphLoc);
            }
            catch (Exception)
            {
                throw;
            }
            try
            {
                Build(optimize);
            }
            catch (Exception)
            {
                throw;
            }
            try
            {
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

        private void ReadGraph(string dcrGraphLoc)
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(dcrGraphLoc);
            XmlNodeList events = xml.SelectNodes("//events//event[not(@type='nesting') and not(@type='subprocess')]");
            XmlNodeList subprocesEvents = xml.SelectNodes("//events//event[@type='subprocess']");
            XmlNodeList nestingEvents = xml.SelectNodes("//events//event[@type='nesting']");
            List<string> nestingEventIds = new List<string>();
            foreach (XmlElement n in nestingEvents)
            {
                nestingEventIds.Add(n.Attributes["id"].Value);
            }
            AddEvents(events, subprocesEvents);
            SetMarkings(xml);
            XmlNodeList conditions = xml.SelectNodes("//constraints/conditions/condition");
            XmlNodeList responses = xml.SelectNodes("//constraints/responses/response");
            XmlNodeList includes = xml.SelectNodes("//constraints/includes/include");
            XmlNodeList excludes = xml.SelectNodes("//constraints/excludes/exclude");
            XmlNodeList milestones = xml.SelectNodes("//constraints/milestones/milestone");
            AddRelations(conditions, nestingEventIds, subprocesEvents, Processor.cond, xml);
            AddRelations(responses, nestingEventIds, subprocesEvents, Processor.resp, xml);
            AddRelations(includes, nestingEventIds, subprocesEvents, Processor.inc, xml);
            AddRelations(excludes, nestingEventIds, subprocesEvents, Processor.exc, xml);
            AddRelations(milestones, nestingEventIds, subprocesEvents, Processor.mile, xml);
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

        private void AddRelations(XmlNodeList relations, List<string> nestingEventIds, XmlNodeList subprocesEvents, string type, XmlDocument xml)
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

        private void AddEvents(XmlNodeList events, XmlNodeList subprocesEvents)
        {
            foreach (XmlElement e in events)
            {
                AddEvent(e);
                foreach (XmlElement s in subprocesEvents)
                {
                    foreach (XmlElement se in s.SelectNodes("/event"))
                    {
                        if (se.Attributes["id"].Value.Equals(e.Attributes["id"].Value))
                        {
                            AddEventToSubprocess(e, s);
                        }
                    }
                }
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

        private void AddEventToSubprocess(XmlElement e, XmlElement s)
        {
            graph.AddEventToSubprocess(e.Attributes["id"].Value, s.Attributes["id"].Value);
        }

        private void ReadTraces(string traceListLoc)
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(traceListLoc);
            XmlNodeList list = xml.SelectNodes("//trace[not(@type='Forbidden')]");
            foreach (XmlElement n in list)
            {
                List<string> eventList = new List<string>();
                for (int i = 0; i < n.ChildNodes.Count; i++)
                {
                    if (n.ChildNodes[i].Name.Equals("event"))
                    {
                        eventList.Add(n.ChildNodes[i].Attributes["id"].Value);
                    }
                }
                trace.Add(eventList);
            }
        }

        private void Build(bool optimize)
        {
            builder = new XMLBuilder(graph, trace, idLabel, labelId);
            builder.Build();
            if (optimize)
            {
                builder.Optimize();
            }
        }
    }
}

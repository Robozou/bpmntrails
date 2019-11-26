using System;
using System.IO;
using System.Xml.Serialization;

namespace bpmntrails
{
    public class BPMNTrail
    {
        private const int SEHW = 36;
        private const int GHW = 50;
        private const int TH = 80;
        private const int TW = 100;

        private int placeX = 10;
        private int placeY = 10;

        private Definition trail;

        public BPMNTrail()
        {
            trail = new Definition();
            trail.process.id = "process_id";
            trail.diagram.id = "diagram_id";
            trail.diagram.bpmnPlane.id = "plane_id";
            trail.diagram.bpmnPlane.bpmnElement = "process_id";
        }

        private void UpdatePlacement()
        {
            placeX += 120;
            if (placeX % 610 == 0)
            {
                placeX = 10;
                placeY += 100;
            }
        }

        public void InsertMergeGate(string eventId, string gateId, string seqflowIdToEvent, string seqflowIdFromEvent)
        {
            AddExclusiveGateway(gateId, true);
            AddSequenceFlow(seqflowIdToEvent, gateId, eventId);
            AddSequenceFlow(seqflowIdFromEvent, eventId, gateId);
            trail.process.sequenceFlows.FindAll(x => x.targetRef.Equals(eventId)).ForEach(x => x.targetRef = gateId);
        }

        public Boolean ContainsEvent(string id)
        {
            foreach (StartEvent se in trail.process.startEvents)
            {
                if (se.id.Equals(id))
                {
                    return true;
                }
            }
            foreach (Task t in trail.process.tasks)
            {
                if (t.id.Equals(id))
                {
                    return true;
                }
            }
            foreach (EndEvent ee in trail.process.endEvents)
            {
                if (ee.id.Equals(id))
                {
                    return true;
                }
            }
            return false;
        }

        public void AddStartEvent(string id)
        {
            StartEvent startEvent = new StartEvent
            {
                id = id
            };
            trail.process.startEvents.Add(startEvent);
            BPMNShape bPMNShape = new BPMNShape
            {
                id = id + "_shape",
                bpmnElement = id
            };
            bPMNShape.bounds.X = placeX + "";
            bPMNShape.bounds.Y = placeY + "";
            bPMNShape.bounds.Width = SEHW + "";
            bPMNShape.bounds.Height = SEHW + "";
            trail.diagram.bpmnPlane.bpmnShapes.Add(bPMNShape);
            UpdatePlacement();
        }

        public void AddTask(string id, string name)
        {
            Task task = new Task
            {
                id = id,
                name = name
            };
            trail.process.tasks.Add(task);
            BPMNShape bPMNShape = new BPMNShape
            {
                id = id + "_shape",
                bpmnElement = id
            };
            bPMNShape.bounds.X = placeX + "";
            bPMNShape.bounds.Y = placeY + "";
            bPMNShape.bounds.Width = TW + "";
            bPMNShape.bounds.Height = TH + "";
            trail.diagram.bpmnPlane.bpmnShapes.Add(bPMNShape);
            UpdatePlacement();
        }

        public void AddEndEvent(string id)
        {
            EndEvent endEvent = new EndEvent
            {
                id = id
            };
            trail.process.endEvents.Add(endEvent);
            BPMNShape bPMNShape = new BPMNShape
            {
                id = id + "_shape",
                bpmnElement = id
            };
            bPMNShape.bounds.X = placeX + "";
            bPMNShape.bounds.Y = placeY + "";
            bPMNShape.bounds.Width = SEHW + "";
            bPMNShape.bounds.Height = SEHW + "";
            trail.diagram.bpmnPlane.bpmnShapes.Add(bPMNShape);
            UpdatePlacement();
        }

        public void AddParallelGateway(string id, Boolean converging)
        {
            ParallelGateway parallelGateway = new ParallelGateway
            {
                id = id,
                gatewayDirection = (converging ? "Converging" : "Diverging")
            };
            trail.process.parallelGateways.Add(parallelGateway);
            BPMNShape bPMNShape = new BPMNShape
            {
                id = id + "_shape",
                bpmnElement = id
            };
            bPMNShape.bounds.X = placeX + "";
            bPMNShape.bounds.Y = placeY + "";
            bPMNShape.bounds.Width = GHW + "";
            bPMNShape.bounds.Height = GHW + "";
            trail.diagram.bpmnPlane.bpmnShapes.Add(bPMNShape);
            UpdatePlacement();
        }

        public void AddExclusiveGateway(string id, Boolean converging)
        {
            ExclusiveGateway exclusiveGateway = new ExclusiveGateway
            {
                id = id,
                gatewayDirection = (converging ? "Converging" : "Diverging")
            };
            trail.process.exclusiveGateways.Add(exclusiveGateway);
            BPMNShape bPMNShape = new BPMNShape
            {
                id = id + "_shape",
                bpmnElement = id
            };
            bPMNShape.bounds.X = placeX + "";
            bPMNShape.bounds.Y = placeY + "";
            bPMNShape.bounds.Width = GHW + "";
            bPMNShape.bounds.Height = GHW + "";
            trail.diagram.bpmnPlane.bpmnShapes.Add(bPMNShape);
            UpdatePlacement();
        }

        public void AddSequenceFlow(string id, string sourceId, string targetId)
        {
            if (!trail.process.sequenceFlows.Exists(seq => seq.sourceRef == sourceId && seq.targetRef == targetId))
            {
                SequenceFlow sequenceFlow = new SequenceFlow();
                sequenceFlow.id = id;
                sequenceFlow.sourceRef = sourceId;
                sequenceFlow.targetRef = targetId;
                trail.process.sequenceFlows.Add(sequenceFlow);
                string sourceType = "";
                string targetType = "";
                foreach (StartEvent se in trail.process.startEvents)
                {
                    if (se.id.Equals(sourceId))
                    {
                        se.outgoing.Add(id);
                        sourceType = "se";
                        break;
                    }
                    else if (se.id.Equals(targetId))
                    {
                        Console.Write("Cannot target start events");
                        break;
                    }
                }
                foreach (EndEvent ee in trail.process.endEvents)
                {
                    if (ee.id.Equals(targetId))
                    {
                        ee.incoming.Add(id);
                        targetType = "se";
                        break;
                    }
                    else if (ee.id.Equals(sourceId))
                    {
                        Console.Write("Cannot use end events as source");
                        break;
                    }
                }
                foreach (Task task in trail.process.tasks)
                {
                    if (task.id.Equals(sourceId))
                    {
                        task.outgoing.Add(id);
                        sourceType = "task";
                    }
                    else if (task.id.Equals(targetId))
                    {
                        task.incoming.Add(id);
                        targetType = "task";
                    }
                }
                foreach (ParallelGateway par in trail.process.parallelGateways)
                {
                    if (par.id.Equals(sourceId))
                    {
                        par.outgoing.Add(id);
                        sourceType = "gate";
                    }
                    else if (par.id.Equals(targetId))
                    {
                        par.incoming.Add(id);
                        targetType = "gate";
                    }
                }
                foreach (ExclusiveGateway inc in trail.process.exclusiveGateways)
                {
                    if (inc.id.Equals(sourceId))
                    {
                        inc.outgoing.Add(id);
                        sourceType = "gate";
                    }
                    else if (inc.id.Equals(targetId))
                    {
                        inc.incoming.Add(id);
                        targetType = "gate";
                    }
                }
                BPMNEdge bPMNEdge = new BPMNEdge
                {
                    id = id + "_edge",
                    bpmnElement = id
                };
                Waypoint first = new Waypoint { X = "0", Y = "0" };
                Waypoint second = new Waypoint { X = "0", Y = "0" };
                switch (sourceType)
                {
                    case "se":
                        first.Y = SEHW / 2 + "";
                        break;
                    case "task":
                        first.Y = TH / 2 + "";
                        break;
                    case "gate":
                        first.Y = GHW / 2 + "";
                        break;
                    default:
                        break;
                }
                switch (targetType)
                {
                    case "se":
                        second.Y = SEHW / 2 + "";
                        second.X = SEHW + "";
                        break;
                    case "task":
                        second.Y = TH / 2 + "";
                        second.X = TW + "";
                        break;
                    case "gate":
                        second.Y = GHW / 2 + "";
                        second.X = GHW + "";
                        break;
                    default:
                        break;
                }
                bPMNEdge.waypoint.Add(first);
                bPMNEdge.waypoint.Add(second);
                trail.diagram.bpmnPlane.bpmnEdges.Add(bPMNEdge);
            }
        }

        public void PrintTrail()
        {
            XmlSerializer serial = new XmlSerializer(typeof(Definition));
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("bpmn", "http://www.omg.org/spec/BPMN/20100524/MODEL");
            TextWriter writer = new StreamWriter("..\\..\\..\\..\\test.xml");
            serial.Serialize(writer, trail, ns);
            writer.Close();
        }
    }
}
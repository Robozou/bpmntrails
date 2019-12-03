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

        public void InsertMergeGate(string eventId, string gateId, string seqflowIdToEvent)
        {
            AddExclusiveGateway(gateId, true);
            trail.process.sequenceFlows.FindAll(x => x.targetRef.Equals(eventId)).ForEach(x => 
                { x.targetRef = gateId; 
                trail.process.exclusiveGateways.Find(y => y.id.Equals(gateId)).incoming.Add(x.id); 
                trail.process.tasks.Find(z => z.id.Equals(eventId)).incoming.Remove(x.id); });
            AddSequenceFlow(seqflowIdToEvent, gateId, eventId);
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
                SequenceFlow sequenceFlow = new SequenceFlow
                {
                    id = id,
                    sourceRef = sourceId,
                    targetRef = targetId
                };
                trail.process.sequenceFlows.Add(sequenceFlow);
                string sourceType = "";
                string targetType = "";
                int x_target = 0, x_source = 0, y_target = 0, y_source = 0;
                Bounds b;
                foreach (StartEvent se in trail.process.startEvents)
                {
                    if (se.id.Equals(sourceId))
                    {
                        se.outgoing.Add(id);
                        sourceType = "se";
                        b = trail.diagram.bpmnPlane.bpmnShapes.Find(x => x.id.Equals(se.id + "_shape")).bounds;
                        x_source = int.Parse(b.X);
                        y_source = int.Parse(b.Y);
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
                        b = trail.diagram.bpmnPlane.bpmnShapes.Find(x => x.id.Equals(ee.id + "_shape")).bounds;
                        x_target = int.Parse(b.X);
                        y_target = int.Parse(b.Y);
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
                        b = trail.diagram.bpmnPlane.bpmnShapes.Find(x => x.id.Equals(task.id + "_shape")).bounds;
                        x_source = int.Parse(b.X);
                        y_source = int.Parse(b.Y);
                    }
                    else if (task.id.Equals(targetId))
                    {
                        task.incoming.Add(id);
                        targetType = "task";
                        b = trail.diagram.bpmnPlane.bpmnShapes.Find(x => x.id.Equals(task.id + "_shape")).bounds;
                        x_target = int.Parse(b.X);
                        y_target = int.Parse(b.Y);
                    }
                }
                foreach (ParallelGateway par in trail.process.parallelGateways)
                {
                    if (par.id.Equals(sourceId))
                    {
                        par.outgoing.Add(id);
                        sourceType = "gate";
                        b = trail.diagram.bpmnPlane.bpmnShapes.Find(x => x.id.Equals(par.id + "_shape")).bounds;
                        x_source = int.Parse(b.X);
                        y_source = int.Parse(b.Y);
                    }
                    else if (par.id.Equals(targetId))
                    {
                        par.incoming.Add(id);
                        targetType = "gate";
                        b = trail.diagram.bpmnPlane.bpmnShapes.Find(x => x.id.Equals(par.id + "_shape")).bounds;
                        x_target = int.Parse(b.X);
                        y_target = int.Parse(b.Y);
                    }
                }
                foreach (ExclusiveGateway exc in trail.process.exclusiveGateways)
                {
                    if (exc.id.Equals(sourceId))
                    {
                        exc.outgoing.Add(id);
                        sourceType = "gate";
                        b = trail.diagram.bpmnPlane.bpmnShapes.Find(x => x.id.Equals(exc.id + "_shape")).bounds;
                        x_source = int.Parse(b.X);
                        y_source = int.Parse(b.Y);
                    }
                    else if (exc.id.Equals(targetId))
                    {
                        exc.incoming.Add(id);
                        targetType = "gate";
                        b = trail.diagram.bpmnPlane.bpmnShapes.Find(x => x.id.Equals(exc.id + "_shape")).bounds;
                        x_target = int.Parse(b.X);
                        y_target = int.Parse(b.Y);
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
                        first.Y = SEHW / 2 + y_source + "";
                        first.X = x_source + "";
                        break;
                    case "task":
                        first.Y = TH / 2 + y_source + "";
                        first.X = x_source + "";
                        break;
                    case "gate":
                        first.Y = GHW / 2 + y_source + "";
                        first.X = x_source + "";
                        break;
                    default:
                        break;
                }
                switch (targetType)
                {
                    case "se":
                        second.Y = SEHW / 2 + y_target + "";
                        second.X = SEHW + x_target + "";
                        break;
                    case "task":
                        second.Y = TH / 2 + y_target + "";
                        second.X = TW + x_target + "";
                        break;
                    case "gate":
                        second.Y = GHW / 2 + y_target + "";
                        second.X = GHW + x_target + "";
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
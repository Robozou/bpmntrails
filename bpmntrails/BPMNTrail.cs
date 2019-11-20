using System;
using System.IO;
using System.Xml.Serialization;

namespace bpmntrails
{
    public class BPMNTrail
    {
        private Definition trail;

        public BPMNTrail()
        {
            trail = new Definition();
            trail.process.id = "process_id";
            trail.diagram.id = "diagram_id";
            trail.diagram.bpmnPlane.id = "plane_id";
            trail.diagram.bpmnPlane.bpmnElement = "process_id";
        }

        public void AddStartEvent(string id)
        {
            StartEvent startEvent = new StartEvent();
            startEvent.id = id;
            trail.process.startEvents.Add(startEvent);
            BPMNShape bPMNShape = new BPMNShape();
            bPMNShape.id = id + "_shape";
            bPMNShape.bpmnElement = id;
            bPMNShape.bounds.X = "0";
            bPMNShape.bounds.Y = "0";
            bPMNShape.bounds.Width = "36";
            bPMNShape.bounds.Height = "36";
            trail.diagram.bpmnPlane.bpmnShapes.Add(bPMNShape);
        }

        public void AddTask(string id, string name)
        {
            Task task = new Task();
            task.id = id;
            task.name = name;
            trail.process.tasks.Add(task);
            BPMNShape bPMNShape = new BPMNShape();
            bPMNShape.id = id + "_shape";
            bPMNShape.bpmnElement = id;
            bPMNShape.bounds.X = "0";
            bPMNShape.bounds.Y = "0";
            bPMNShape.bounds.Width = "100";
            bPMNShape.bounds.Height = "80";
            trail.diagram.bpmnPlane.bpmnShapes.Add(bPMNShape);
        }

        public void AddEndEvent(string id)
        {
            EndEvent endEvent = new EndEvent();
            endEvent.id = id;
            trail.process.endEvents.Add(endEvent);
            BPMNShape bPMNShape = new BPMNShape();
            bPMNShape.id = id + "_shape";
            bPMNShape.bpmnElement = id;
            bPMNShape.bounds.X = "0";
            bPMNShape.bounds.Y = "0";
            bPMNShape.bounds.Width = "36";
            bPMNShape.bounds.Height = "36";
            trail.diagram.bpmnPlane.bpmnShapes.Add(bPMNShape);
        }

        public void AddParallelGateway(string id, Boolean converging)
        {
            ParallelGateway parallelGateway = new ParallelGateway();
            parallelGateway.id = id;
            parallelGateway.gatewayDirection = (converging ? "Converging" : "Diverging");
            trail.process.parallelGateways.Add(parallelGateway);
            BPMNShape bPMNShape = new BPMNShape();
            bPMNShape.id = id + "_shape";
            bPMNShape.bpmnElement = id;
            bPMNShape.bounds.X = "0";
            bPMNShape.bounds.Y = "0";
            bPMNShape.bounds.Width = "50";
            bPMNShape.bounds.Height = "50";
            trail.diagram.bpmnPlane.bpmnShapes.Add(bPMNShape);
        }

        public void AddExclusiveGateway(string id, Boolean converging)
        {
            ExclusiveGateway exclusiveGateway = new ExclusiveGateway();
            exclusiveGateway.id = id;
            exclusiveGateway.gatewayDirection = (converging ? "Converging" : "Diverging");
            trail.process.exclusiveGateways.Add(exclusiveGateway);
            BPMNShape bPMNShape = new BPMNShape();
            bPMNShape.id = id + "_shape";
            bPMNShape.bpmnElement = id;
            bPMNShape.bounds.X = "0";
            bPMNShape.bounds.Y = "0";
            bPMNShape.bounds.Width = "50";
            bPMNShape.bounds.Height = "50";
            trail.diagram.bpmnPlane.bpmnShapes.Add(bPMNShape);
        }

        public void AddSequenceFlow(string id, string sourceId, string targetId)
        {
            SequenceFlow sequenceFlow = new SequenceFlow();
            sequenceFlow.id = id;
            sequenceFlow.sourceRef = sourceId;
            sequenceFlow.targetRef = targetId;
            trail.process.sequenceFlows.Add(sequenceFlow);
            foreach (StartEvent se in trail.process.startEvents)
            {
                if (se.id.Equals(sourceId))
                {
                    se.outgoing.Add(id);
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
                }
                else if (task.id.Equals(targetId))
                {
                    task.incoming.Add(id);
                }
            }
            foreach (ParallelGateway par in trail.process.parallelGateways)
            {
                if (par.id.Equals(sourceId))
                {
                    par.outgoing.Add(id);
                }
                else if (par.id.Equals(targetId))
                {
                    par.incoming.Add(id);
                }
            }
            foreach (ExclusiveGateway inc in trail.process.exclusiveGateways)
            {
                if (inc.id.Equals(sourceId))
                {
                    inc.outgoing.Add(id);
                }
                else if (inc.id.Equals(targetId))
                {
                    inc.incoming.Add(id);
                }
            }
            BPMNEdge bPMNEdge = new BPMNEdge();
            bPMNEdge.id = id + "_edge";
            bPMNEdge.bpmnElement = id;
            bPMNEdge.waypoint.Add(new Waypoint { X = "0", Y = "0" });
            bPMNEdge.waypoint.Add(new Waypoint { X = "0", Y = "0" });
            trail.diagram.bpmnPlane.bpmnEdges.Add(bPMNEdge);
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
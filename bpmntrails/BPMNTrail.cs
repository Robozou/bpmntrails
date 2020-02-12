using System;
using System.Collections.Generic;
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

        public void MoveMergeGate(string mergeGateId, string elementIdToMoveTo, string elementIdToMoveFrom)
        {
            ExclusiveGateway eg = GetExclusiveGateWay(mergeGateId);
            dynamic moveToElement = null;
            if (TaskExists(elementIdToMoveTo))
            {
                moveToElement = GetTask(elementIdToMoveTo);
            }
            else if (ExclusiveGatewayExists(elementIdToMoveTo))
            {
                moveToElement = GetExclusiveGateWay(elementIdToMoveTo);
            }
            dynamic moveFromElement = null;
            if (TaskExists(elementIdToMoveFrom))
            {
                moveFromElement = GetTask(elementIdToMoveFrom);
            }
            else if (ExclusiveGatewayExists(elementIdToMoveFrom))
            {
                moveFromElement = GetExclusiveGateWay(elementIdToMoveFrom);
            }

            if (moveToElement != null && moveFromElement != null)
            {

                string idFromMoveToElement = GetSequenceFlow(moveToElement.outgoing[0]).id;
                string idToMergeGate = GetSequenceFlow(eg.incoming[0]).id;
                string idToMoveFromElement = GetSequenceFlow(moveFromElement.incoming[0]).id;

                string idOfEventBeforeMergeGate = GetSequenceFlow(idToMergeGate).sourceRef;
                string idAfterMoveToEvent = GetSequenceFlow(idFromMoveToElement).targetRef;

                List<string> ls1 = new List<string>
            {
                idFromMoveToElement,
                idToMergeGate,
                idToMoveFromElement
            };
                List<string> ls2 = new List<string>
            {
                idFromMoveToElement + "_edge",
                idToMergeGate + "_edge",
                idToMoveFromElement + "_edge"
            };

                trail.process.sequenceFlows.RemoveAll(x => ls1.Contains(x.id));
                trail.diagram.bpmnPlane.bpmnEdges.RemoveAll(x => ls2.Contains(x.id));

                trail.process.tasks.ForEach(x =>
                    {
                        x.incoming.RemoveAll(y => ls1.Contains(y));
                        x.outgoing.RemoveAll(y => ls1.Contains(y));
                    });
                trail.process.exclusiveGateways.ForEach(x =>
                    {
                        x.incoming.RemoveAll(y => ls1.Contains(y));
                        x.outgoing.RemoveAll(y => ls1.Contains(y));
                    });
                trail.process.parallelGateways.ForEach(x =>
                    {
                        x.incoming.RemoveAll(y => ls1.Contains(y));
                        x.outgoing.RemoveAll(y => ls1.Contains(y));
                    });

                AddSequenceFlow(idFromMoveToElement + "neu", moveToElement.id, eg.id);
                AddSequenceFlow(idToMergeGate + "neu", eg.id, idAfterMoveToEvent);
                AddSequenceFlow(idToMoveFromElement + "neu", idOfEventBeforeMergeGate, moveFromElement.id);
            }
        }

        #region Getters and other small helper methods
        public StartEvent GetStartEvent(string id)
        {
            return trail.process.startEvents.Find(x => x.id.Equals(id));
        }

        public List<StartEvent> GetStartEvents()
        {
            return trail.process.startEvents;
        }

        public bool StartEventExists(string id)
        {
            return trail.process.startEvents.Exists(x => x.id.Equals(id));
        }

        public bool EndEventExists(string id)
        {
            return trail.process.endEvents.Exists(x => x.id.Equals(id));
        }

        public bool ExclusiveGatewayExists(string id)
        {
            return trail.process.exclusiveGateways.Exists(x => x.id.Equals(id));
        }

        public bool ParallelGatewayExists(string id)
        {
            return trail.process.parallelGateways.Exists(x => x.id.Equals(id));
        }

        public bool TaskExists(string id)
        {
            return trail.process.tasks.Exists(x => x.id.Equals(id));
        }

        public SequenceFlow GetSequenceFlow(string id)
        {
            return trail.process.sequenceFlows.Find(x => x.id.Equals(id));
        }

        public Task GetTask(string id)
        {
            return trail.process.tasks.Find(x => x.id.Equals(id));
        }

        public StartEvent GetFirstStartEvent()
        {
            return trail.process.startEvents[0];
        }

        public List<ExclusiveGateway> GetExclusiveGateWays()
        {
            return trail.process.exclusiveGateways;
        }

        public List<ParallelGateway> GetParallelGateWays()
        {
            return trail.process.parallelGateways;
        }

        public EndEvent GetEndEvent(string id)
        {
            return trail.process.endEvents.Find(x => x.id.Equals(id));
        }

        public List<EndEvent> GetEndEvents()
        {
            return trail.process.endEvents;
        }

        public void ChangeSourceRefOnSequenceFlow(string seqFlowId, string newSourceRef)
        {
            trail.process.sequenceFlows.Find(y => y.id.Equals(seqFlowId)).sourceRef = newSourceRef;
        }

        public void ChangeTargetRefOnSequenceFlow(string seqFlowId, string newTargetRef)
        {
            trail.process.sequenceFlows.Find(y => y.id.Equals(seqFlowId)).targetRef = newTargetRef;
        }

        public List<Task> GetTasks()
        {
            return trail.process.tasks;
        }

        public List<SequenceFlow> GetSequenceFlowsBetweenSplitNodes()
        {
            return trail.process.sequenceFlows.FindAll(x => trail.process.exclusiveGateways.Exists(y => y.id.Equals(x.sourceRef) && y.gatewayDirection.Equals("Diverging")) && trail.process.exclusiveGateways.Exists(z => z.id.Equals(x.targetRef) && z.gatewayDirection.Equals("Diverging")));
        }

        public List<SequenceFlow> GetSequenceFlowsBetweenMergeNodes()
        {
            return trail.process.sequenceFlows.FindAll(x => trail.process.exclusiveGateways.Exists(y => y.id.Equals(x.sourceRef) && y.gatewayDirection.Equals("Converging")) && trail.process.exclusiveGateways.Exists(z => z.id.Equals(x.targetRef) && z.gatewayDirection.Equals("Converging")));
        }

        public ExclusiveGateway GetExclusiveGateWay(string id)
        {
            return trail.process.exclusiveGateways.Find(x => x.id.Equals(id));
        }

        public ParallelGateway GetParallelGateWay(string id)
        {
            return trail.process.parallelGateways.Find(x => x.id.Equals(id));
        }

        public List<BPMNElement> GetAllElements()
        {
            List<BPMNElement> elements = new List<BPMNElement>();
            GetTasks().ForEach(x => elements.Add(x));
            GetExclusiveGateWays().ForEach(x => elements.Add(x));
            GetParallelGateWays().ForEach(x => elements.Add(x));
            GetEndEvents().ForEach(x => elements.Add(x));
            GetStartEvents().ForEach(x => elements.Add(x));
            return elements;
        }

        public SequenceFlow GetSequenceFlowByTargetRefId(string id)
        {
            return trail.process.sequenceFlows.Find(x => x.targetRef.Equals(id));
        }

        public List<BPMNElement> GetBPMNEventElements()
        {
            List<BPMNElement> elements = new List<BPMNElement>();
            GetTasks().ForEach(x => elements.Add(x));
            GetEndEvents().ForEach(x => elements.Add(x));
            GetStartEvents().ForEach(x => elements.Add(x));
            return elements;
        }

        public Boolean ContainsEvent(string id)
        {
            return GetBPMNEventElements().Exists(ele => ele.id.Equals(id));
        }

        private bool IsIdInUse(string id)
        {
            return GetAllElements().Exists(x => x.id.Equals(id));
        }

        public Task GetTaskByNameIfIdInList(string name, List<string> idList)
        {
            return trail.process.tasks.Find(x => x.name.Equals(name) && idList.Contains(x.id));
        }
        #endregion

        public void AddBackLoopingSequence(string mergeGateId, string eventId, string seqFlowBackLoopId)
        {
            Task task = GetTask(eventId);
            if (task != null)
            {
                string incId = task.incoming[0];
                SequenceFlow incSeq = GetSequenceFlow(incId);
                if (incSeq != null)
                {
                    string source = incSeq.sourceRef;
                    if (ExclusiveGatewayExists(source))
                    {
                        AddSequenceFlow(seqFlowBackLoopId, source, mergeGateId);
                    }
                    else if (TaskExists(source))
                    {
                        InsertExclusiveGate(eventId, mergeGateId + eventId + seqFlowBackLoopId, seqFlowBackLoopId + mergeGateId + eventId);
                        AddSequenceFlow(seqFlowBackLoopId, mergeGateId + eventId + seqFlowBackLoopId, mergeGateId);
                    }
                }
            }
        }

        public void RemoveTaskAndMoveSequences(string eventId)
        {
            dynamic bpmnElement = null;
            if (TaskExists(eventId))
            {
                bpmnElement = GetTask(eventId);
            }
            else if (ExclusiveGatewayExists(eventId))
            {
                bpmnElement = GetExclusiveGateWay(eventId); ;
            }
            else if (ParallelGatewayExists(eventId))
            {
                bpmnElement = GetParallelGateWay(eventId);
            }

            if (bpmnElement != null)
            {
                string incId = bpmnElement.incoming.Count == 1 ? bpmnElement.incoming[0] : null;
                string outId = bpmnElement.outgoing.Count == 1 ? bpmnElement.outgoing[0] : null;
                SequenceFlow incSeq = GetSequenceFlow(incId);
                SequenceFlow outSeq = GetSequenceFlow(outId);
                if (incSeq != null && outSeq != null)
                {
                    string source = incSeq.sourceRef;
                    string target = outSeq.targetRef;
                    RemoveEventWithSequences(eventId);
                    AddSequenceFlow(incId, source, target);
                }
                return;
            }
            else if (EndEventExists(eventId))
            {
                bpmnElement = GetEndEvent(eventId);
                string incId = bpmnElement.incoming.Count == 1 ? bpmnElement.incoming[0] : null;
                SequenceFlow incSeq = trail.process.sequenceFlows.Find(x => x.id.Equals(incId));
                if (incSeq != null)
                {
                    RemoveEventWithSequences(eventId);
                }
            }
        }

        private void RemoveEventOutGoing(List<string> outgoing)
        {
            outgoing.ForEach(outseq =>
            {
                trail.process.sequenceFlows.RemoveAll(x => x.id.Equals(outseq));
                trail.process.endEvents.ForEach(x => x.incoming.Remove(outseq));
                trail.process.tasks.ForEach(x => x.incoming.Remove(outseq));
                trail.process.exclusiveGateways.ForEach(x => x.incoming.Remove(outseq));
                trail.process.parallelGateways.ForEach(x => x.incoming.Remove(outseq));
                trail.diagram.bpmnPlane.bpmnEdges.RemoveAll(x => x.id.Equals(outseq + "_edge"));
            });
        }

        private void RemoveEventInGoing(List<string> incoming)
        {
            incoming.ForEach(inseq =>
            {
                trail.process.sequenceFlows.RemoveAll(x => x.id.Equals(inseq));
                trail.process.startEvents.ForEach(x => x.outgoing.Remove(inseq));
                trail.process.tasks.ForEach(x => x.outgoing.Remove(inseq));
                trail.process.exclusiveGateways.ForEach(x => x.outgoing.Remove(inseq));
                trail.process.parallelGateways.ForEach(x => x.outgoing.Remove(inseq));
                trail.diagram.bpmnPlane.bpmnEdges.RemoveAll(x => x.id.Equals(inseq + "_edge"));
            });
        }

        public void RemoveEventWithSequences(string eventId)
        {
            if (StartEventExists(eventId))
            {
                StartEvent se = GetStartEvent(eventId);
                RemoveEventOutGoing(se.outgoing);
                trail.process.startEvents.RemoveAll(y => y.id.Equals(se.id));
                trail.diagram.bpmnPlane.bpmnShapes.RemoveAll(y => y.id.Equals(se.id + "_shape"));
            }
            if (EndEventExists(eventId))
            {
                EndEvent ee = GetEndEvent(eventId);
                RemoveEventInGoing(ee.incoming);
                trail.process.endEvents.RemoveAll(y => y.id.Equals(ee.id));
                trail.diagram.bpmnPlane.bpmnShapes.RemoveAll(y => y.id.Equals(ee.id + "_shape"));
            }
            if (TaskExists(eventId))
            {
                Task task = GetTask(eventId);
                RemoveEventOutGoing(task.outgoing);
                RemoveEventInGoing(task.incoming);
                trail.process.tasks.RemoveAll(y => y.id.Equals(task.id));
                trail.diagram.bpmnPlane.bpmnShapes.RemoveAll(y => y.id.Equals(task.id + "_shape"));
            }
            if (ParallelGatewayExists(eventId))
            {
                ParallelGateway pg = GetParallelGateWay(eventId);
                RemoveEventOutGoing(pg.outgoing);
                RemoveEventInGoing(pg.incoming);
                trail.process.parallelGateways.RemoveAll(y => y.id.Equals(pg.id));
                trail.diagram.bpmnPlane.bpmnShapes.RemoveAll(y => y.id.Equals(pg.id + "_shape"));
            }
            if (ExclusiveGatewayExists(eventId))
            {
                ExclusiveGateway eg = GetExclusiveGateWay(eventId);
                RemoveEventOutGoing(eg.outgoing);
                RemoveEventInGoing(eg.incoming);
                trail.process.exclusiveGateways.RemoveAll(y => y.id.Equals(eg.id));
                trail.diagram.bpmnPlane.bpmnShapes.RemoveAll(y => y.id.Equals(eg.id + "_shape"));
            }
        }

        public Boolean HasMergeGate(string eventId)
        {
            List<SequenceFlow> seqList = trail.process.sequenceFlows.FindAll(x => x.targetRef.Equals(eventId));
            List<ExclusiveGateway> mergeList = trail.process.exclusiveGateways.FindAll(x => x.gatewayDirection.Equals("Converging"));
            foreach (ExclusiveGateway eg in mergeList)
            {
                foreach (SequenceFlow seq in seqList)
                {
                    if (eg.id.Equals(seq.sourceRef))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void InsertMergeGate(string eventId, string gateId, string seqflowIdToEvent)
        {
            AddExclusiveGateway(gateId, true);
            InsertGateHelper(eventId, gateId, seqflowIdToEvent);
        }

        private void InsertExclusiveGate(string eventId, string gateId, string seqflowIdToEvent)
        {
            AddExclusiveGateway(gateId, false);
            InsertGateHelper(eventId, gateId, seqflowIdToEvent);
        }

        private void InsertGateHelper(string eventId, string gateId, string seqflowIdToEvent)
        {
            trail.process.sequenceFlows.FindAll(x => x.targetRef.Equals(eventId)).ForEach(x =>
            {
                x.targetRef = gateId;
                GetExclusiveGateWay(gateId).incoming.Add(x.id);
                GetTask(eventId).incoming.Remove(x.id);
            });
            AddSequenceFlow(seqflowIdToEvent, gateId, eventId);
        }

        #region Add node elements
        public void AddStartEvent(string id)
        {
            if (!IsIdInUse(id))
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
            else
            {
                throw new Exception("Id in use.");
            }
        }

        public void AddTask(string id, string name)
        {
            if (!IsIdInUse(id))
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
            else
            {
                throw new Exception("Id in use.");
            }
        }

        public void AddEndEvent(string id)
        {
            if (!IsIdInUse(id))
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
            else
            {
                throw new Exception("Id in use.");
            }
        }

        public void AddParallelGateway(string id, Boolean converging)
        {
            if (!IsIdInUse(id))
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
            else
            {
                throw new Exception("Id in use.");
            }
        }

        public void AddExclusiveGateway(string id, Boolean converging)
        {
            if (!IsIdInUse(id))
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
            else
            {
                throw new Exception("Id in use.");
            }
        }
        #endregion

        public void AddSequenceFlow(string id, string sourceId, string targetId)
        {
            if (!trail.process.sequenceFlows.Exists(seq => seq.sourceRef.Equals(sourceId) && seq.targetRef.Equals(targetId) && seq.id.Equals(id)))
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
                if (StartEventExists(sourceId))
                {
                    StartEvent se = GetStartEvent(sourceId);
                    se.outgoing.Add(id);
                    sourceType = "se";
                    b = trail.diagram.bpmnPlane.bpmnShapes.Find(x => x.id.Equals(se.id + "_shape")).bounds;
                    x_source = int.Parse(b.X);
                    y_source = int.Parse(b.Y);
                }
                else if (StartEventExists(targetId))
                {
                    Console.Write("Cannot target start events");
                }
                if (EndEventExists(targetId))
                {
                    EndEvent ee = GetEndEvent(targetId);
                    ee.incoming.Add(id);
                    targetType = "se";
                    b = trail.diagram.bpmnPlane.bpmnShapes.Find(x => x.id.Equals(ee.id + "_shape")).bounds;
                    x_target = int.Parse(b.X);
                    y_target = int.Parse(b.Y);
                }
                else if (EndEventExists(sourceId))
                {
                    Console.Write("Cannot use end events as source");
                }
                if (TaskExists(sourceId))
                {
                    Task task = GetTask(sourceId);
                    task.outgoing.Add(id);
                    sourceType = "task";
                    b = trail.diagram.bpmnPlane.bpmnShapes.Find(x => x.id.Equals(task.id + "_shape")).bounds;
                    x_source = int.Parse(b.X);
                    y_source = int.Parse(b.Y);
                }
                else if (TaskExists(targetId))
                {
                    Task task = GetTask(targetId);
                    task.incoming.Add(id);
                    targetType = "task";
                    b = trail.diagram.bpmnPlane.bpmnShapes.Find(x => x.id.Equals(task.id + "_shape")).bounds;
                    x_target = int.Parse(b.X);
                    y_target = int.Parse(b.Y);
                }
                if (ParallelGatewayExists(sourceId))
                {
                    ParallelGateway par = GetParallelGateWay(sourceId);
                    par.outgoing.Add(id);
                    sourceType = "gate";
                    b = trail.diagram.bpmnPlane.bpmnShapes.Find(x => x.id.Equals(par.id + "_shape")).bounds;
                    x_source = int.Parse(b.X);
                    y_source = int.Parse(b.Y);
                }
                else if (ParallelGatewayExists(targetId))
                {
                    ParallelGateway par = GetParallelGateWay(targetId);
                    par.incoming.Add(id);
                    targetType = "gate";
                    b = trail.diagram.bpmnPlane.bpmnShapes.Find(x => x.id.Equals(par.id + "_shape")).bounds;
                    x_target = int.Parse(b.X);
                    y_target = int.Parse(b.Y);
                }
                if (ExclusiveGatewayExists(sourceId))
                {
                    ExclusiveGateway exc = GetExclusiveGateWay(sourceId);
                    exc.outgoing.Add(id);
                    sourceType = "gate";
                    b = trail.diagram.bpmnPlane.bpmnShapes.Find(x => x.id.Equals(exc.id + "_shape")).bounds;
                    x_source = int.Parse(b.X);
                    y_source = int.Parse(b.Y);
                }
                else if (ExclusiveGatewayExists(targetId))
                {
                    ExclusiveGateway exc = GetExclusiveGateWay(targetId);
                    exc.incoming.Add(id);
                    targetType = "gate";
                    b = trail.diagram.bpmnPlane.bpmnShapes.Find(x => x.id.Equals(exc.id + "_shape")).bounds;
                    x_target = int.Parse(b.X);
                    y_target = int.Parse(b.Y);
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

        public void PrintTrail(string fileLoc)
        {
            try
            {
                XmlSerializer serial = new XmlSerializer(typeof(Definition));
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add("bpmn", "http://www.omg.org/spec/BPMN/20100524/MODEL");
                TextWriter writer = new StreamWriter(fileLoc);
                serial.Serialize(writer, trail, ns);
                writer.Close();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
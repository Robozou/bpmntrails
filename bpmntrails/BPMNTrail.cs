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
            ExclusiveGateway eg = trail.process.exclusiveGateways.Find(x => x.id.Equals(mergeGateId));
            dynamic moveToElement = null;
            if (trail.process.tasks.Exists(x => x.id.Equals(elementIdToMoveTo)))
            {
                moveToElement = trail.process.tasks.Find(x => x.id.Equals(elementIdToMoveTo));
            }
            else if (trail.process.exclusiveGateways.Exists(x => x.id.Equals(elementIdToMoveTo)))
            {
                moveToElement = trail.process.exclusiveGateways.Find(x => x.id.Equals(elementIdToMoveTo));
            }
            dynamic moveFromElement = null;
            if (trail.process.tasks.Exists(x => x.id.Equals(elementIdToMoveFrom)))
            {
                moveFromElement = trail.process.tasks.Find(x => x.id.Equals(elementIdToMoveFrom));
            }
            else if (trail.process.exclusiveGateways.Exists(x => x.id.Equals(elementIdToMoveFrom)))
            {
                moveFromElement = trail.process.exclusiveGateways.Find(x => x.id.Equals(elementIdToMoveFrom));
            }

            if (moveToElement != null && moveFromElement != null)
            {

                string idFromMoveToElement = trail.process.sequenceFlows.Find(x => x.id.Equals(moveToElement.outgoing[0])).id;
                string idToMergeGate = trail.process.sequenceFlows.Find(x => x.id.Equals(eg.incoming[0])).id;
                string idToMoveFromElement = trail.process.sequenceFlows.Find(x => x.id.Equals(moveFromElement.incoming[0])).id;

                string idOfEventBeforeMergeGate = trail.process.sequenceFlows.Find(x => x.id.Equals(idToMergeGate)).sourceRef;
                string idAfterMoveToEvent = trail.process.sequenceFlows.Find(x => x.id.Equals(idFromMoveToElement)).targetRef;

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

                trail.process.tasks.ForEach(x => { x.incoming.RemoveAll(y => ls1.Contains(y)); x.outgoing.RemoveAll(y => ls1.Contains(y)); });
                trail.process.exclusiveGateways.ForEach(x => { x.incoming.RemoveAll(y => ls1.Contains(y)); x.outgoing.RemoveAll(y => ls1.Contains(y)); });
                trail.process.parallelGateways.ForEach(x => { x.incoming.RemoveAll(y => ls1.Contains(y)); x.outgoing.RemoveAll(y => ls1.Contains(y)); });

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
        #endregion

        public void AddBackLoopingSequence(string mergeGateId, string eventId, string seqFlowBackLoopId)
        {
            Task task = trail.process.tasks.Find(x => x.id.Equals(eventId));
            if (task != null)
            {
                string incId = task.incoming[0];
                SequenceFlow incSeq = trail.process.sequenceFlows.Find(x => x.id.Equals(incId));
                if (incSeq != null)
                {
                    string source = incSeq.sourceRef;
                    if (trail.process.exclusiveGateways.Exists(x => x.id.Equals(source)))
                    {
                        AddSequenceFlow(seqFlowBackLoopId, source, mergeGateId);
                    }
                    else if (trail.process.tasks.Exists(x => x.id.Equals(source)))
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
            if (trail.process.tasks.Exists(x => x.id.Equals(eventId)))
            {
                bpmnElement = trail.process.tasks.Find(x => x.id.Equals(eventId));
            }
            else if (trail.process.exclusiveGateways.Exists(x => x.id.Equals(eventId)))
            {
                bpmnElement = trail.process.exclusiveGateways.Find(x => x.id.Equals(eventId));
            }
            else if (trail.process.parallelGateways.Exists(x => x.id.Equals(eventId)))
            {
                bpmnElement = trail.process.parallelGateways.Find(x => x.id.Equals(eventId));
            }

            if (bpmnElement != null)
            {
                string incId = bpmnElement.incoming.Count == 1 ? bpmnElement.incoming[0] : null;
                string outId = bpmnElement.outgoing.Count == 1 ? bpmnElement.outgoing[0] : null;
                SequenceFlow incSeq = trail.process.sequenceFlows.Find(x => x.id.Equals(incId));
                SequenceFlow outSeq = trail.process.sequenceFlows.Find(x => x.id.Equals(outId));
                if (incSeq != null && outSeq != null)
                {
                    string source = incSeq.sourceRef;
                    string target = outSeq.targetRef;
                    RemoveEventWithSequences(eventId);
                    AddSequenceFlow(incId, source, target);
                }
                return;
            }
            else if (trail.process.endEvents.Exists(x => x.id.Equals(eventId)))
            {
                bpmnElement = trail.process.endEvents.Find(x => x.id.Equals(eventId));
                string incId = bpmnElement.incoming.Count == 1 ? bpmnElement.incoming[0] : null;
                SequenceFlow incSeq = trail.process.sequenceFlows.Find(x => x.id.Equals(incId));
                if (incSeq != null)
                {
                    RemoveEventWithSequences(eventId);
                }
            }
        }

        public Task GetTaskByNameIfIdInList(string name, List<string> idList)
        {
            return trail.process.tasks.Find(x => x.name.Equals(name) && idList.Contains(x.id));
        }

        //TODO NEEDS REFACTORING BADLY
        public void RemoveEventWithSequences(string eventId)
        {
            List<StartEvent> sel = new List<StartEvent>();
            foreach (StartEvent se in trail.process.startEvents)
            {
                if (se.id.Equals(eventId))
                {
                    foreach (string outseq in se.outgoing)
                    {
                        trail.process.sequenceFlows.RemoveAll(x => x.id.Equals(outseq));
                        trail.process.endEvents.ForEach(x => x.incoming.Remove(outseq));
                        trail.process.tasks.ForEach(x => x.incoming.Remove(outseq));
                        trail.process.exclusiveGateways.ForEach(x => x.incoming.Remove(outseq));
                        trail.process.parallelGateways.ForEach(x => x.incoming.Remove(outseq));
                        trail.diagram.bpmnPlane.bpmnEdges.RemoveAll(x => x.id.Equals(outseq + "_edge"));
                    }
                    sel.Add(se);
                }
            }
            sel.ForEach(x => trail.process.startEvents.RemoveAll(y => y.id.Equals(x.id)));
            sel.ForEach(x => trail.diagram.bpmnPlane.bpmnShapes.RemoveAll(y => y.id.Equals(x.id + "_shape")));
            List<EndEvent> eel = new List<EndEvent>();
            foreach (EndEvent ee in trail.process.endEvents)
            {
                if (ee.id.Equals(eventId))
                {
                    foreach (string inseq in ee.incoming)
                    {
                        trail.process.sequenceFlows.RemoveAll(x => x.id.Equals(inseq));
                        trail.process.startEvents.ForEach(x => x.outgoing.Remove(inseq));
                        trail.process.tasks.ForEach(x => x.outgoing.Remove(inseq));
                        trail.process.exclusiveGateways.ForEach(x => x.outgoing.Remove(inseq));
                        trail.process.parallelGateways.ForEach(x => x.outgoing.Remove(inseq));
                        trail.diagram.bpmnPlane.bpmnEdges.RemoveAll(x => x.id.Equals(inseq + "_edge"));
                    }
                    eel.Add(ee);
                }
            }
            eel.ForEach(x => trail.process.endEvents.RemoveAll(y => y.id.Equals(x.id)));
            eel.ForEach(x => trail.diagram.bpmnPlane.bpmnShapes.RemoveAll(y => y.id.Equals(x.id + "_shape")));
            List<Task> tl = new List<Task>();
            foreach (Task t in trail.process.tasks)
            {
                if (t.id.Equals(eventId))
                {
                    foreach (string outseq in t.outgoing)
                    {
                        trail.process.sequenceFlows.RemoveAll(x => x.id.Equals(outseq));
                        trail.process.endEvents.ForEach(x => x.incoming.Remove(outseq));
                        trail.process.tasks.ForEach(x => x.incoming.Remove(outseq));
                        trail.process.exclusiveGateways.ForEach(x => x.incoming.Remove(outseq));
                        trail.process.parallelGateways.ForEach(x => x.incoming.Remove(outseq));
                        trail.diagram.bpmnPlane.bpmnEdges.RemoveAll(x => x.id.Equals(outseq + "_edge"));
                    }
                    foreach (string inseq in t.incoming)
                    {
                        trail.process.sequenceFlows.RemoveAll(x => x.id.Equals(inseq));
                        trail.process.startEvents.ForEach(x => x.outgoing.Remove(inseq));
                        trail.process.tasks.ForEach(x => x.outgoing.Remove(inseq));
                        trail.process.exclusiveGateways.ForEach(x => x.outgoing.Remove(inseq));
                        trail.process.parallelGateways.ForEach(x => x.outgoing.Remove(inseq));
                        trail.diagram.bpmnPlane.bpmnEdges.RemoveAll(x => x.id.Equals(inseq + "_edge"));
                    }
                    tl.Add(t);
                }
            }
            tl.ForEach(x => trail.process.tasks.RemoveAll(y => y.id.Equals(x.id)));
            tl.ForEach(x => trail.diagram.bpmnPlane.bpmnShapes.RemoveAll(y => y.id.Equals(x.id + "_shape")));
            List<ParallelGateway> pgl = new List<ParallelGateway>();
            foreach (ParallelGateway pg in trail.process.parallelGateways)
            {
                if (pg.id.Equals(eventId))
                {
                    foreach (string outseq in pg.outgoing)
                    {
                        trail.process.sequenceFlows.RemoveAll(x => x.id.Equals(outseq));
                        trail.process.endEvents.ForEach(x => x.incoming.Remove(outseq));
                        trail.process.tasks.ForEach(x => x.incoming.Remove(outseq));
                        trail.process.exclusiveGateways.ForEach(x => x.incoming.Remove(outseq));
                        trail.process.parallelGateways.ForEach(x => x.incoming.Remove(outseq));
                        trail.diagram.bpmnPlane.bpmnEdges.RemoveAll(x => x.id.Equals(outseq + "_edge"));
                    }
                    foreach (string inseq in pg.incoming)
                    {
                        trail.process.sequenceFlows.RemoveAll(x => x.id.Equals(inseq));
                        trail.process.startEvents.ForEach(x => x.outgoing.Remove(inseq));
                        trail.process.tasks.ForEach(x => x.outgoing.Remove(inseq));
                        trail.process.exclusiveGateways.ForEach(x => x.outgoing.Remove(inseq));
                        trail.process.parallelGateways.ForEach(x => x.outgoing.Remove(inseq));
                        trail.diagram.bpmnPlane.bpmnEdges.RemoveAll(x => x.id.Equals(inseq + "_edge"));
                    }
                    pgl.Add(pg);
                }
            }
            pgl.ForEach(x => trail.process.parallelGateways.RemoveAll(y => y.id.Equals(x.id)));
            pgl.ForEach(x => trail.diagram.bpmnPlane.bpmnShapes.RemoveAll(y => y.id.Equals(x.id + "_shape")));
            List<ExclusiveGateway> egl = new List<ExclusiveGateway>();
            foreach (ExclusiveGateway eg in trail.process.exclusiveGateways)
            {
                if (eg.id.Equals(eventId))
                {
                    foreach (string outseq in eg.outgoing)
                    {
                        trail.process.sequenceFlows.RemoveAll(x => x.id.Equals(outseq));
                        trail.process.endEvents.ForEach(x => x.incoming.Remove(outseq));
                        trail.process.tasks.ForEach(x => x.incoming.Remove(outseq));
                        trail.process.exclusiveGateways.ForEach(x => x.incoming.Remove(outseq));
                        trail.process.parallelGateways.ForEach(x => x.incoming.Remove(outseq));
                        trail.diagram.bpmnPlane.bpmnEdges.RemoveAll(x => x.id.Equals(outseq + "_edge"));
                    }
                    foreach (string inseq in eg.incoming)
                    {
                        trail.process.sequenceFlows.RemoveAll(x => x.id.Equals(inseq));
                        trail.process.startEvents.ForEach(x => x.outgoing.Remove(inseq));
                        trail.process.tasks.ForEach(x => x.outgoing.Remove(inseq));
                        trail.process.exclusiveGateways.ForEach(x => x.outgoing.Remove(inseq));
                        trail.process.parallelGateways.ForEach(x => x.outgoing.Remove(inseq));
                        trail.diagram.bpmnPlane.bpmnEdges.RemoveAll(x => x.id.Equals(inseq + "_edge"));
                    }
                    egl.Add(eg);
                }
            }
            egl.ForEach(x => trail.process.exclusiveGateways.RemoveAll(y => y.id.Equals(x.id)));
            egl.ForEach(x => trail.diagram.bpmnPlane.bpmnShapes.RemoveAll(y => y.id.Equals(x.id + "_shape")));
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
            trail.process.sequenceFlows.FindAll(x => x.targetRef.Equals(eventId)).ForEach(x =>
                {
                    x.targetRef = gateId;
                    trail.process.exclusiveGateways.Find(y => y.id.Equals(gateId)).incoming.Add(x.id);
                    trail.process.tasks.Find(z => z.id.Equals(eventId)).incoming.Remove(x.id);
                });
            AddSequenceFlow(seqflowIdToEvent, gateId, eventId);
        }

        private void InsertExclusiveGate(string eventId, string gateId, string seqflowIdToEvent)
        {
            AddExclusiveGateway(gateId, false);
            trail.process.sequenceFlows.FindAll(x => x.targetRef.Equals(eventId)).ForEach(x =>
            {
                x.targetRef = gateId;
                trail.process.exclusiveGateways.Find(y => y.id.Equals(gateId)).incoming.Add(x.id);
                trail.process.tasks.Find(z => z.id.Equals(eventId)).incoming.Remove(x.id);
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
                if (trail.process.startEvents.Exists(se => se.id.Equals(sourceId)))
                {
                    StartEvent se = trail.process.startEvents.Find(see => see.id.Equals(sourceId));
                    se.outgoing.Add(id);
                    sourceType = "se";
                    b = trail.diagram.bpmnPlane.bpmnShapes.Find(x => x.id.Equals(se.id + "_shape")).bounds;
                    x_source = int.Parse(b.X);
                    y_source = int.Parse(b.Y);
                }
                else if (trail.process.startEvents.Exists(se => se.id.Equals(targetId)))
                {
                    Console.Write("Cannot target start events");
                }
                if (trail.process.endEvents.Exists(ee => ee.id.Equals(targetId)))
                {
                    EndEvent ee = trail.process.endEvents.Find(eee => eee.id.Equals(targetId));
                    ee.incoming.Add(id);
                    targetType = "se";
                    b = trail.diagram.bpmnPlane.bpmnShapes.Find(x => x.id.Equals(ee.id + "_shape")).bounds;
                    x_target = int.Parse(b.X);
                    y_target = int.Parse(b.Y);
                }
                else if (trail.process.endEvents.Exists(se => se.id.Equals(sourceId)))
                {
                    Console.Write("Cannot use end events as source");
                }
                if (trail.process.tasks.Exists(task => task.id.Equals(sourceId)))
                {
                    Task task = trail.process.tasks.Find(taske => taske.id.Equals(sourceId));
                    task.outgoing.Add(id);
                    sourceType = "task";
                    b = trail.diagram.bpmnPlane.bpmnShapes.Find(x => x.id.Equals(task.id + "_shape")).bounds;
                    x_source = int.Parse(b.X);
                    y_source = int.Parse(b.Y);
                }
                else if (trail.process.tasks.Exists(task => task.id.Equals(targetId)))
                {
                    Task task = trail.process.tasks.Find(taske => taske.id.Equals(targetId));
                    task.incoming.Add(id);
                    targetType = "task";
                    b = trail.diagram.bpmnPlane.bpmnShapes.Find(x => x.id.Equals(task.id + "_shape")).bounds;
                    x_target = int.Parse(b.X);
                    y_target = int.Parse(b.Y);
                }
                if (trail.process.parallelGateways.Exists(par => par.id.Equals(sourceId)))
                {
                    ParallelGateway par = trail.process.parallelGateways.Find(pare => pare.id.Equals(sourceId));
                    par.outgoing.Add(id);
                    sourceType = "gate";
                    b = trail.diagram.bpmnPlane.bpmnShapes.Find(x => x.id.Equals(par.id + "_shape")).bounds;
                    x_source = int.Parse(b.X);
                    y_source = int.Parse(b.Y);
                }
                else if (trail.process.parallelGateways.Exists(par => par.id.Equals(targetId)))
                {
                    ParallelGateway par = trail.process.parallelGateways.Find(pare => pare.id.Equals(targetId));
                    par.incoming.Add(id);
                    targetType = "gate";
                    b = trail.diagram.bpmnPlane.bpmnShapes.Find(x => x.id.Equals(par.id + "_shape")).bounds;
                    x_target = int.Parse(b.X);
                    y_target = int.Parse(b.Y);
                }
                if (trail.process.exclusiveGateways.Exists(exc => exc.id.Equals(sourceId)))
                {
                    ExclusiveGateway exc = trail.process.exclusiveGateways.Find(exce => exce.id.Equals(sourceId));
                    exc.outgoing.Add(id);
                    sourceType = "gate";
                    b = trail.diagram.bpmnPlane.bpmnShapes.Find(x => x.id.Equals(exc.id + "_shape")).bounds;
                    x_source = int.Parse(b.X);
                    y_source = int.Parse(b.Y);
                }
                else if (trail.process.exclusiveGateways.Exists(exc => exc.id.Equals(targetId)))
                {
                    ExclusiveGateway exc = trail.process.exclusiveGateways.Find(exce => exce.id.Equals(targetId));
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
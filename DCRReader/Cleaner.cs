using bpmntrails;
using System.Collections.Generic;

namespace DCRReader
{
    internal class Cleaner
    {
        private List<List<string>> traces;
        private BPMNTrail trail;
        private Dictionary<string, string> labelId;
        private Dictionary<List<string>, int> traceStatus;
        private HashSet<string> bpmnElements;

        public Cleaner(List<List<string>> traces, Dictionary<string, string> labelId)
        {
            this.traces = traces;
            this.labelId = labelId;
            traceStatus = new Dictionary<List<string>, int>();
            bpmnElements = new HashSet<string>();
            foreach (List<string> trace in traces)
            {
                traceStatus[trace] = 0;
            }
        }

        public BPMNTrail Clean(BPMNTrail trail)
        {
            this.trail = trail;
            FindEvents();
            RemoveEvents();
            RemoveRedundantMergeAndSplitNodes();
            CombineLinkedSplitNodes();
            CombineLinkedMergeNodes();
            return trail;
        }

        private void CombineLinkedSplitNodes()
        {
            List<SequenceFlow> sequenceFlows = trail.GetSequenceFlowsBetweenSplitNodes();
            Queue<SequenceFlow> seqQueue = new Queue<SequenceFlow>();
            sequenceFlows.ForEach(seqQueue.Enqueue);
            SequenceFlow curr = null;
            ExclusiveGateway firstEg = null;
            ExclusiveGateway secondEg = null;
            while (seqQueue.Count > 0)
            {
                curr = seqQueue.Dequeue();
                firstEg = trail.GetExclusiveGateWay(curr.sourceRef);
                secondEg = trail.GetExclusiveGateWay(curr.targetRef);
                secondEg.outgoing.ForEach(x =>
                {
                    firstEg.outgoing.Add(x);
                    trail.ChangeSourceRefOnSequenceFlow(x, firstEg.id); 
                });
                secondEg.outgoing.RemoveAll(x => true);
                trail.RemoveEventWithSequences(secondEg.id);
            }
        }

        private void CombineLinkedMergeNodes()
        {
            List<SequenceFlow> sequenceFlows = trail.GetSequenceFlowsBetweenMergeNodes();
            Queue<SequenceFlow> seqQueue = new Queue<SequenceFlow>();
            sequenceFlows.ForEach(seqQueue.Enqueue);
            SequenceFlow curr = null;
            ExclusiveGateway firstEg = null;
            ExclusiveGateway secondEg = null;
            while (seqQueue.Count > 0)
            {
                curr = seqQueue.Dequeue();
                firstEg = trail.GetExclusiveGateWay(curr.sourceRef);
                secondEg = trail.GetExclusiveGateWay(curr.targetRef);
                firstEg.incoming.ForEach(x =>
                {
                    secondEg.incoming.Add(x);
                    trail.ChangeTargetRefOnSequenceFlow(x, secondEg.id);
                });
                firstEg.incoming.RemoveAll(x => true);
                trail.RemoveEventWithSequences(firstEg.id);
            }
        }

        private void RemoveRedundantMergeAndSplitNodes()
        {
            List<string> toBeDel = new List<string>();
            foreach (ExclusiveGateway eg in trail.GetExclusiveGateWays())
            {
                if (eg.gatewayDirection.Equals("Diverging") && eg.outgoing.Count == 1)
                {
                    toBeDel.Add(eg.id);
                }
                else if (eg.gatewayDirection.Equals("Converging") && eg.incoming.Count == 1)
                {
                    toBeDel.Add(eg.id);
                }
            }
            toBeDel.ForEach(x => trail.RemoveTaskAndMoveSequences(x));
        }

        private void FindEvents()
        {
            StartEvent se = trail.GetFirstStartEvent();
            bpmnElements.Add(se.id);
            foreach (List<string> trace in traces)
            {
                trail.GetStartEvent(se.id).outgoing.ForEach(x => FindEvent(x, trace));
            }
        }

        // Skal gennemgå hele trace fra ende til anden ikke gennemgå grafen
        private void FindEvent(string bpmnSeqId, List<string> trace)
        {
            string targetId = trail.GetSequenceFlow(bpmnSeqId).targetRef;
            if (trail.EndEventExists(targetId) && traceStatus[trace] == trace.Count)
            {
                bpmnElements.Add(targetId);
                return;
            }
            else if (trail.ExclusiveGatewayExists(targetId))
            {
                bpmnElements.Add(targetId);
                trail.GetExclusiveGateWay(targetId).outgoing.ForEach(x => FindEvent(x, trace));
            }
            else if (trail.ParallelGatewayExists(targetId))
            {
                bpmnElements.Add(targetId);
                trail.GetParallelGateWay(targetId).outgoing.ForEach(x => FindEvent(x, trace));
            }
            else if (trail.TaskExists(targetId))
            {
                if (trace.Count > traceStatus[trace])
                {
                    Task task = trail.GetTask(targetId);
                    string curr = string.Empty;
                    for (int i = 0; i <= traceStatus[trace]; i++)
                    {
                        curr += trace[i];
                    }
                    if (labelId[task.name].Equals(trace[traceStatus[trace]]))
                    {
                        bpmnElements.Add(task.id);
                        traceStatus[trace]++;
                        foreach (string id in task.outgoing)
                        {
                            FindEvent(id, trace);
                        }
                    }
                }
            }
        }

        private void RemoveEvents()
        {
            List<string> toBeDel = new List<string>();
            foreach (Task task in trail.GetTasks())
            {
                if (!bpmnElements.Contains(task.id))
                {
                    toBeDel.Add(task.id);
                }
            }
            foreach (EndEvent ee in trail.GetEndEvents())
            {
                if (!bpmnElements.Contains(ee.id))
                {
                    toBeDel.Add(ee.id);
                }
            }
            foreach (ExclusiveGateway eg in trail.GetExclusiveGateWays())
            {
                if (!bpmnElements.Contains(eg.id))
                {
                    toBeDel.Add(eg.id);
                }
            }
            foreach (ParallelGateway pg in trail.GetParallelGateWays())
            {
                if (!bpmnElements.Contains(pg.id))
                {
                    toBeDel.Add(pg.id);
                }
            }
            foreach (string id in toBeDel)
            {
                trail.RemoveTaskAndMoveSequences(id);
            }
        }
    }
}
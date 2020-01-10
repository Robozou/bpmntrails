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
            RemoveEvents(); //This should be expanded to also find end nodes.
            RemoveRedundantMergeAndSplitNodes(); //Maybe expand this to include the ability to join linked gates.
            return trail;
        }

        private void RemoveRedundantMergeAndSplitNodes()
        {
            List<string> toBeDel = new List<string>();
            foreach (ExclusiveGateway eg in trail.Definition.process.exclusiveGateways)
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
            StartEvent se = trail.Definition.process.startEvents[0];
            bpmnElements.Add(se.id);
            foreach (List<string> trace in traces)
            {
                trail.Definition.process.startEvents.Find(x => se.id.Equals(x.id)).outgoing.ForEach(x => FindEvent(x, trace));
            }
        }

        // Skal gennemgå hele trace fra ende til anden ikke gennemgå grafen
        private void FindEvent(string bpmnSeqId, List<string> trace)
        {
            string targetId = trail.Definition.process.sequenceFlows.Find(x => x.id.Equals(bpmnSeqId)).targetRef;
            if (trail.Definition.process.endEvents.Exists(x => x.id.Equals(targetId)) && traceStatus[trace] == trace.Count)
            {
                bpmnElements.Add(targetId);
                return;
            }
            else if (trail.Definition.process.exclusiveGateways.Exists(x => x.id.Equals(targetId)))
            {
                bpmnElements.Add(targetId);
                trail.Definition.process.exclusiveGateways.Find(x => x.id.Equals(targetId)).outgoing.ForEach(x => FindEvent(x, trace));
            }
            else if (trail.Definition.process.parallelGateways.Exists(x => x.id.Equals(targetId)))
            {
                bpmnElements.Add(targetId);
                trail.Definition.process.parallelGateways.Find(x => x.id.Equals(targetId)).outgoing.ForEach(x => FindEvent(x, trace));
            }
            else if (trail.Definition.process.tasks.Exists(x => x.id.Equals(targetId)))
            {
                if (trace.Count > traceStatus[trace])
                {
                    Task task = trail.Definition.process.tasks.Find(x => x.id.Equals(targetId));
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
            foreach (Task task in trail.Definition.process.tasks)
            {
                if (!bpmnElements.Contains(task.id))
                {
                    toBeDel.Add(task.id);
                }
            }
            foreach (string id in toBeDel)
            {
                trail.RemoveTaskAndMoveSequences(id);
            }
        }
    }
}
﻿using bpmntrails;
using System;
using System.Collections.Generic;

namespace DCRReader
{
    internal class Cleaner
    {
        private List<List<string>> traces;
        private Dictionary<string, string> labelId;
        private BPMNTrail trail;
        HashSet<string> bpmnElements;

        public Cleaner(List<List<string>> traces, Dictionary<string, string> labelId)
        {
            this.traces = traces;
            this.labelId = labelId;
            bpmnElements = new HashSet<string>();
        }

        public BPMNTrail Clean(BPMNTrail trail)
        {
            this.trail = trail;
            FindEvents();
            RemoveEvents();
            RemoveDanglingEndEvents();
            RemoveRedundantMergeAndSplitNodes();
            return trail;
        }

        private void RemoveRedundantMergeAndSplitNodes()
        {
            List<string> toBeDel = new List<string>();
            foreach(ExclusiveGateway eg in trail.Definition.process.exclusiveGateways)
            {
                if(eg.gatewayDirection.Equals("Diverging") && eg.outgoing.Count == 1)
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

        private void RemoveDanglingEndEvents()
        {
            List<string> toBeDel = new List<string>();
            foreach(EndEvent ee in trail.Definition.process.endEvents)
            {
                foreach(string id in ee.incoming)
                {
                    String sourceId = trail.Definition.process.sequenceFlows.Find(x => x.id.Equals(id)).sourceRef;
                    if(trail.Definition.process.startEvents.Exists(x => x.id.Equals(sourceId)))
                    {
                        toBeDel.Add(ee.id);
                    }
                    else if (trail.Definition.process.parallelGateways.Exists(x => x.id.Equals(sourceId)))
                    {
                        toBeDel.Add(ee.id);
                    }
                    else if (trail.Definition.process.exclusiveGateways.Exists(x => x.id.Equals(sourceId)))
                    {
                        toBeDel.Add(ee.id);
                    }
                }
            }
            toBeDel.ForEach(x => trail.RemoveEventWithSequences(x));
        }

        private void FindEvents()
        {
            StartEvent se = trail.Definition.process.startEvents[0];
            bpmnElements.Add(se.id);
            foreach(List<string> trace in traces)
            {
                trail.Definition.process.startEvents.Find(x => se.id.Equals(x.id)).outgoing.ForEach(x => FindEvent(x, trace, 0));
            }
        }
        // Skal gennemgå hele trace fra ende til anden ikke gennemgå grafen
        private void FindEvent(string bpmnSeqId, List<string> trace, int index)
        {
            string targetId = trail.Definition.process.sequenceFlows.Find(x => x.id.Equals(bpmnSeqId)).targetRef;
            if(trail.Definition.process.endEvents.Exists(x => x.id.Equals(targetId)))
            {
                return;
            }
            else if(trail.Definition.process.exclusiveGateways.Exists(x => x.id.Equals(targetId)))
            {
                ExclusiveGateway eg = trail.Definition.process.exclusiveGateways.Find(x => x.id.Equals(targetId));
                foreach(string id in eg.outgoing)
                {
                    FindEvent(id, trace, index);
                }
            }
            else if (trail.Definition.process.parallelGateways.Exists(x => x.id.Equals(targetId)))
            {
                ParallelGateway pg = trail.Definition.process.parallelGateways.Find(x => x.id.Equals(targetId));
                foreach (string id in pg.outgoing)
                {
                    FindEvent(id, trace, index);
                }
            }
            else if (trail.Definition.process.tasks.Exists(x => x.id.Equals(targetId)))
            {
                Task task = trail.Definition.process.tasks.Find(x => x.id.Equals(targetId));
                if (labelId[task.name].Equals(trace[index]))
                {
                    bpmnElements.Add(task.id);
                    foreach (string id in task.outgoing)
                    {
                        FindEvent(id, trace, index+1);
                    }
                }
            }
        }

        private void RemoveEvents()
        {
            List<string> toBeDel = new List<string>();
            foreach(Task task in trail.Definition.process.tasks)
            {
                if (!bpmnElements.Contains(task.id))
                {
                    toBeDel.Add(task.id);
                }
            }
            foreach(string id in toBeDel)
            {
                trail.RemoveTaskAndMoveSequences(id);
            }
        }
    }
}
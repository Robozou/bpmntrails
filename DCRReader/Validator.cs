using bpmntrails;
using System;
using System.Collections.Generic;

namespace DCRReader
{
    //needs to visit each edge atleast once to see if traces expressed in bpmn is valid in dcr.
    public class Validator
    {
        //Maybe check to see if there are disconnected bpmn events
        Processor graph;
        BPMNTrail trail;
        Dictionary<string, string> labelId;
        List<bool> results;
        public Validator(Processor graph, Dictionary<string, string> labelId)
        {
            this.graph = graph;
            this.labelId = labelId;
            results = new List<bool>();
        }

        public bool Validate(BPMNTrail trail)
        {
            this.trail = trail;
            StartEvent se = trail.Definition.process.startEvents[0];
            RunStates(se, graph);
            return results.TrueForAll(x => x.Equals(true));
        }

        private void RunStates(dynamic bpmnEvent, Processor graph)
        {

            if (bpmnEvent.GetType().Equals(typeof(StartEvent)))
            {
                StartEvent se = (StartEvent)bpmnEvent;
                foreach (string seqId in se.outgoing)
                {
                    RunStates(trail.Definition.process.sequenceFlows.Find(x => x.id.Equals(seqId)), graph);
                }
            }
            else if (bpmnEvent.GetType().Equals(typeof(EndEvent)))
            {
                results.Add(true);
            }
            else if (bpmnEvent.GetType().Equals(typeof(Task)))
            {
                Task task = (Task)bpmnEvent;
                try
                {
                    graph.Execute(labelId[task.name]);
                    foreach (string seqId in task.outgoing)
                    {
                        RunStates(trail.Definition.process.sequenceFlows.Find(x => x.id.Equals(seqId)), graph);
                    }
                }
                catch (Exception)
                {
                    results.Add(false);
                }
            }
            else if (bpmnEvent.GetType().Equals(typeof(ParallelGateway)))
            {
                ParallelGateway pg = (ParallelGateway)bpmnEvent;
                foreach (string seqId in pg.outgoing)
                {
                    RunStates(trail.Definition.process.sequenceFlows.Find(x => x.id.Equals(seqId)), graph);
                }
            }
            else if (bpmnEvent.GetType().Equals(typeof(ExclusiveGateway)))
            {
                ExclusiveGateway eg = (ExclusiveGateway)bpmnEvent;
                foreach (string seqId in eg.outgoing)
                {
                    RunStates(trail.Definition.process.sequenceFlows.Find(x => x.id.Equals(seqId)), graph);
                }
            }
        }
    }
}
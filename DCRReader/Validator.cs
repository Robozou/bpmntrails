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
            StartEvent se = trail.GetFirstStartEvent();
            RunStates(se, graph);
            return results.TrueForAll(x => x.Equals(true));
        }

        private void RunStates(dynamic bpmnEvent, Processor graph)
        {
            if (bpmnEvent.GetType().Equals(typeof(StartEvent)))
            {
                ((StartEvent)bpmnEvent).outgoing.ForEach(x => RunStates(trail.GetSequenceFlow(x), graph));
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
                    task.outgoing.ForEach(x => RunStates(trail.GetSequenceFlow(x), graph));
                }
                catch (Exception)
                {
                    results.Add(false);
                }
            }
            else if (bpmnEvent.GetType().Equals(typeof(ParallelGateway)))
            {
                ((ParallelGateway)bpmnEvent).outgoing.ForEach(x => RunStates(trail.GetSequenceFlow(x), graph));
            }
            else if (bpmnEvent.GetType().Equals(typeof(ExclusiveGateway)))
            {
                ((ExclusiveGateway)bpmnEvent).outgoing.ForEach(x => RunStates(trail.GetSequenceFlow(x), graph));
            }
        }
    }
}
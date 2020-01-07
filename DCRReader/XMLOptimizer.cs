using bpmntrails;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DCRReader
{
    class XMLOptimizer
    {
        int buffer = 0;
        internal BPMNTrail Optimize(Processor graph, List<List<string>> traces, Dictionary<string, HashSet<Tuple<string, string>>> tree, BPMNTrail trail, Dictionary<string, string> labelId)
        {
            //MAKE A TEST CASE WHERE THIS IS TRUE PLZ
            //FIND WAY TO LOOP EVEN ON A SINGLE EVENT IF IT OCCURS SEVERAL TIMES
            //test if we can execute the events in the graph if we skip the events lying in between
            //the the original list and the repeats i.e. if a1 -> a2 -> a3 -> a1 -> a2 -> a4 is the original
            //then a1 -> a2 -> a4 must also be possible
            //if this is the case remove the events in the bpmn that corrosponds to the second a1 -> a2
            //add a merge gate infront of the first a1 -> a2 let a3 point to that and let the first a2 -> a4
            //also find a way to handle when we have several alternating states fx a1 -> a2 -> a1 -> a2 -> ...
            //test that all new executions that may result from the change is still valid in dcr
            //collapse diverging gates if chained
            //finally sweep and remove any merge/xor gates with a 1:1 in/out

            //Maybe take longest found repeating sequence do the thing, and then rerun the finder etc.

            //If split inside of a repeating sequence find a way to move it back to the corret predeceding event.

            Cleaner cleaner = new Cleaner(traces, labelId);
            Validator validator = new Validator(graph, labelId);
            BPMNTrail workingTrail;
            RemoveRepeatingSequences(graph, traces, trail, labelId, validator, out workingTrail);
            RemoveRepeatingEvents(graph, traces, workingTrail, labelId, validator, out workingTrail);
            workingTrail = cleaner.Clean(workingTrail);
            return workingTrail;
        }

        private void RemoveRepeatingEvents(Processor graph, List<List<string>> traces, BPMNTrail trail, Dictionary<string, string> labelId, Validator validator, out BPMNTrail workingTrail)
        {
            workingTrail = trail;
            BPMNTrail oldTrail;
            List<string> ids = FindRepeatingEvents(traces);
            foreach (string id in ids)
            {
                oldTrail = workingTrail;
                workingTrail.RemoveTaskAndMoveSequences(id);
                if (!validator.Validate(workingTrail))
                {
                    workingTrail = oldTrail;
                }
            }
        }

        private List<string> FindRepeatingEvents(List<List<string>> traces)
        {
            List<string> ids = new List<string>();
            foreach (List<string> trace in traces)
            {
                for (int startLoc = 0; startLoc < trace.Count - 1; startLoc++)
                {
                    for (int currLoc = startLoc + 1; currLoc < trace.Count; currLoc++)
                    {
                        if (trace[startLoc].Equals(trace[currLoc]))
                        {
                            string id = string.Empty;
                            for (int i = 0; i <= currLoc; i++)
                            {
                                id += trace[i];
                            }
                            ids.Add(id);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            return ids;
        }

        private void RemoveRepeatingSequences(Processor graph, List<List<string>> traces, BPMNTrail trail, Dictionary<string, string> labelId, Validator validator, out BPMNTrail workingTrail)
        {
            //Key = the repeating sequence Value = the start loc and a list of start locs of the repeats.
            Dictionary<List<string>, Tuple<int, List<int>>> dict;
            Dictionary<string, string> idGate = new Dictionary<string, string>();
            workingTrail = trail;
            BPMNTrail oldTrail;
            foreach (List<string> trace in traces)
            {
                dict = FindRepeatingSequence(trace);
                if (dict.Keys.Count > 0)
                {
                    //Move relevant XOR gates here.
                    //Starting at the first of the repeating sequences go through the flow in the graph. 
                    //If it contains XORs that needs fx 12121 move the XOR to after 1.
                    foreach (List<string> repSeq in dict.Keys)
                    {
                        //FixMergeGateLocationsInRepeatedSequences(repSeq, dict[repSeq], trace, workingTrail);
                        string eventId = string.Empty;
                        string gateId = string.Empty;
                        for (int i = 0; i <= dict[repSeq].Item1; i++)
                        {
                            eventId += trace[i];
                        }
                        if (!eventId.Equals(string.Empty) && !idGate.Keys.Contains(eventId))
                        {
                            oldTrail = workingTrail;
                            gateId = "mergeGate" + buffer;
                            workingTrail.InsertMergeGate(eventId, gateId, "seqflowgateevent" + buffer);
                            idGate[eventId] = gateId;
                            List<string> ids = new List<string>();
                            foreach (int pos in dict[repSeq].Item2)
                            {
                                string doubleEventId = string.Empty;
                                for (int i = 0; i < pos + repSeq.Count; i++)
                                {
                                    doubleEventId += trace[i];
                                    if (i >= pos)
                                    {
                                        ids.Add(doubleEventId);
                                    }
                                }
                            }
                            workingTrail.AddBackLoopingSequence(gateId, ids[0], "backLoopSeqFlow" + buffer);
                            foreach (string id in ids)
                            {
                                workingTrail.RemoveTaskAndMoveSequences(id);
                            }
                            buffer++;

                            if (!validator.Validate(workingTrail))
                            {
                                workingTrail = oldTrail;
                            }
                        }
                    }
                }
            }
        }

        private BPMNTrail FixMergeGateLocationsInRepeatedSequences(List<string> repSeq, Tuple<int, List<int>> tuple, List<string> trace, BPMNTrail workingTrail)
        {
            string idString = string.Empty;
            List<string> repSeqIds = new List<string>();
            List<string> ids = new List<string>();
            for (int i = 0; i < repSeq.Count + tuple.Item1; i++)
            {
                idString += trace[i];
                if (i >= tuple.Item1)
                {
                    repSeqIds.Add(idString);
                }
            }
            foreach (int pos in tuple.Item2)
            {
                idString = string.Empty;
                for (int i = 0; i < pos + repSeq.Count; i++)
                {
                    idString += trace[i];
                    if (i >= pos)
                    {
                        ids.Add(idString);
                    }
                }
            }

            Task current = workingTrail.Definition.process.tasks.Find(x => x.id.Equals(repSeqIds.Last<string>()));
            Task helperTask1 = null;
            Task helperTask2 = null;
            ExclusiveGateway eg = null;
            SequenceFlow seqFlow = null;
            int index = 0;
            string nextId = ids[0];
            string helper = string.Empty;
            while (!current.id.Equals(ids.Last<string>()))
            {
                helper = workingTrail.Definition.process.sequenceFlows.Find(x => x.id.Equals(current.outgoing[0])).targetRef;
                if (helper.Equals(nextId))
                {
                    if (!nextId.Equals(ids.Last<string>()))
                    {
                        nextId = ids[index += 1];
                    }
                    current = workingTrail.Definition.process.tasks.Find(x => x.id.Equals(helper));
                }
                else if (workingTrail.Definition.process.exclusiveGateways.Exists(x => x.id.Equals(helper)))
                {
                    eg = workingTrail.Definition.process.exclusiveGateways.Find(x => x.id.Equals(helper));
                    foreach (string s in eg.outgoing)
                    {
                        helper = workingTrail.Definition.process.sequenceFlows.Find(x => x.id.Equals(s)).targetRef;
                        if (helper.Equals(nextId))
                        {
                            seqFlow = workingTrail.Definition.process.sequenceFlows.Find(x => x.id.Equals(s));
                            helperTask1 = workingTrail.Definition.process.tasks.Find(x => x.id.Equals(helper));
                            helperTask2 = workingTrail.Definition.process.tasks.Find(x => x.name.Equals(helperTask1.name) && repSeqIds.Contains(x.id));
                            workingTrail.MoveMergeGate(eg.id, helperTask2.id, helperTask1.id);
                            if (!nextId.Equals(ids.Last<string>()))
                            {
                                nextId = ids[index += 1];
                            }
                            current = helperTask1;
                            break;
                        }
                    }

                }
            }


            return workingTrail;
        }

        private Dictionary<List<string>, Tuple<int, List<int>>> FindRepeatingSequence(List<string> trace)
        {
            Dictionary<List<string>, Tuple<int, List<int>>> dict = new Dictionary<List<string>, Tuple<int, List<int>>>();
            List<int> places;
            int maxLen = trace.Count / 2;
            for (int length = 2; length <= maxLen; length++)
            {
                for (int startLocOfSearchString = 0; startLocOfSearchString + length <= trace.Count - startLocOfSearchString - length; startLocOfSearchString++)
                {
                    List<string> ls = trace.GetRange(startLocOfSearchString, length);
                    for (int startLocOfTestString = 0; startLocOfTestString + length + startLocOfSearchString + length <= trace.Count; startLocOfTestString++)
                    {
                        List<string> test = trace.GetRange(length + startLocOfSearchString + startLocOfTestString, length);
                        if (ls.SequenceEqual<string>(test))
                        {
                            if (dict.ContainsKey(ls))
                            {
                                dict[ls].Item2.Add(length + startLocOfSearchString + startLocOfTestString);
                            }
                            else
                            {
                                places = new List<int>
                                {
                                    length + startLocOfSearchString + startLocOfTestString
                                };
                                dict[ls] = new Tuple<int, List<int>>(startLocOfSearchString, places);
                            }
                        }
                    }
                }
            }

            if (dict.Keys.Count > 1)
            {
                List<List<string>> subs = new List<List<string>>();
                foreach (List<string> ls in dict.Keys)
                {
                    List<List<string>> others = dict.Keys.ToList().FindAll(x => !x.Equals(ls));
                    foreach (List<string> other in others)
                    {
                        if (other.Count > ls.Count)
                        {
                            for (int i = 0; i + ls.Count < other.Count; i++)
                            {
                                List<string> news = other.GetRange(i, ls.Count);
                                if (news.SequenceEqual(ls))
                                {
                                    subs.Add(ls);
                                }
                            }
                        }
                        if (ls.Count + dict[ls].Item1 >= dict[other].Item1 && dict[ls].Item1 < dict[other].Item1)
                        {
                            subs.Add(other);
                        }
                    }
                }
                foreach (List<string> ls in subs)
                {
                    dict.Remove(ls);
                }
            }

            return dict;
        }
    }
}

using bpmntrails;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

namespace DCRReader
{
    class XMLOptimizer
    {
        int buffer = 0;
        internal BPMNTrail Optimize(Processor graph, List<List<string>> trace, Dictionary<string, HashSet<Tuple<string, string>>> tree, BPMNTrail trail, Dictionary<string, string> labelId)
        {
            Dictionary<List<string>, Tuple<int, List<int>>> dict;
            Dictionary<string, string> idGate = new Dictionary<string, string>();
            Validator validator = new Validator(graph, labelId);
            BPMNTrail workingTrail = trail;
            BPMNTrail oldTrail;
            foreach (List<string> l in trace)
            {
                dict = FindRepeatingSequence(l);
                if (dict.Keys.Count > 0)
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

                    if (dict.Keys.Count > 1)
                    {
                        List<List<string>> subs = new List<List<string>>();
                        foreach(List<string> ls in dict.Keys)
                        {
                            List<List<string>> others = dict.Keys.ToList().FindAll(x => !x.Equals(ls));
                            foreach(List<string> other in others)
                            {
                                if (other.Count > ls.Count)
                                {
                                    for(int i = 0; i+ls.Count<other.Count; i++)
                                    {
                                        List<string> news = other.GetRange(i, ls.Count);
                                        if (news.SequenceEqual(ls))
                                        {
                                            subs.Add(ls);
                                        }
                                    }
                                }
                                if (ls.Count + dict[ls].Item1 >= dict[other].Item1 && dict[ls].Item1<dict[other].Item1)
                                {
                                    subs.Add(other);
                                }
                            }
                        }
                        foreach(List<string> ls in subs)
                        {
                            dict.Remove(ls);
                        }
                    }

                    foreach (List<string> ls in dict.Keys)
                    {
                        
                        string eventId = String.Empty;
                        string gateId = String.Empty;
                        for (int i = 0; i <= dict[ls].Item1; i++)
                        {
                            eventId += l[i];
                        }
                        if (!eventId.Equals(String.Empty) && !idGate.Keys.Contains(eventId))
                        {
                            oldTrail = workingTrail;
                            gateId = "mergeGate" + buffer;
                            workingTrail.InsertMergeGate(eventId, gateId, "seqflowgateevent" + buffer);
                            idGate[eventId] = gateId;
                            List<string> ids = new List<string>();
                            foreach (int pos in dict[ls].Item2)
                            {
                                string doubleEventId = String.Empty;
                                for (int i = 0; i < pos + ls.Count; i++)
                                {
                                    doubleEventId += l[i];
                                    if (i >= pos) ids.Add(doubleEventId);
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
            return workingTrail;
        }

        private Dictionary<List<string>, Tuple<int, List<int>>> FindRepeatingSequence(List<string> list)
        {
            Dictionary<List<string>, Tuple<int, List<int>>> dict = new Dictionary<List<string>, Tuple<int, List<int>>>();
            List<int> places;
            int maxLen = list.Count / 2;
            for (int length = 2; length <= maxLen; length++)
            {
                for (int startLocOfSearchString = 0; startLocOfSearchString + length <= list.Count - startLocOfSearchString - length; startLocOfSearchString++)
                {
                    List<string> ls = list.GetRange(startLocOfSearchString, length);
                    for (int startLocOfTestString = 0; startLocOfTestString + length + startLocOfSearchString + length <= list.Count; startLocOfTestString++)
                    {
                        List<string> test = list.GetRange(length + startLocOfSearchString + startLocOfTestString, length);
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
            //foreach(KeyValuePair<List<string>, Tuple<int, List<int>>> kvp in dict)
            //{
            //    Console.WriteLine("Key: ");
            //    kvp.Key.ForEach(x => Console.Write(x + ", "));
            //    Console.WriteLine("\nValue: ");
            //    Console.Write(kvp.Value.Item1 + ": ");
            //    kvp.Value.Item2.ForEach(x => Console.Write(x + ", "));
            //}
            //string s = Console.ReadLine();
            return dict;
        }
    }
}

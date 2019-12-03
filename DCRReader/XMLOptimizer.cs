using System;
using System.Collections.Generic;
using System.Linq;
using bpmntrails;

namespace DCRReader
{
    class XMLOptimizer
    {
        int buffer = 0;
        internal BPMNTrail Optimize(Processor graph, List<List<string>> trace, Dictionary<string, HashSet<Tuple<string, string>>> tree, BPMNTrail trail)
        {
            Dictionary<List<string>, Tuple<int, List<int>>> dict;
            foreach (List<string> l in trace)
            {
                dict = FindRepeatingSequence(l);
                if (dict.Keys.Count > 0)
                {
                    //test if we can execute the events in the graph if we skip the events lying in between
                    //the the original list and the repeats i.e. if a1 -> a2 -> a3 -> a1 -> a2 -> a4 is the original
                    //then a1 -> a2 -> a4 must also be possible
                    //if this is the case remove the events in the bpmn that corrosponds to the second a1 -> a2
                    //add a merge gate infront of the first a1 -> a2 let a3 point to that and let the first a2 -> a4
                    //also find a way to handle when we have several alternating states fx a1 -> a2 -> a1 -> a2 -> ...
                    foreach(List<string> ls in dict.Keys) 
                    {
                        string eventId = String.Empty;
                        for (int i = 0; i < dict[ls].Item1; i++)
                        {
                            eventId += l[i];
                        }
                        trail.InsertMergeGate(eventId,"mergeGate"+buffer,"seqflowgateevent"+buffer);
                        buffer++;
                    }
                    
                    
                    
                }
            }
            return trail;
        }

        private Dictionary<List<string>, Tuple<int, List<int>>> FindRepeatingSequence(List<string> list)
        {
            Dictionary<List<string>, Tuple<int, List<int>>> dict = new Dictionary<List<string>, Tuple<int, List<int>>>();
            List<int> places;
            int maxLen = list.Count / 2;
            for(int length = 2; length<=maxLen; length++)
            {
                for(int startLocOfSearchString = 0; startLocOfSearchString+length<=list.Count-startLocOfSearchString-length; startLocOfSearchString++)
                {
                    List<string> ls = list.GetRange(startLocOfSearchString, length);
                    for(int startLocOfTestString = 0; startLocOfTestString+length+startLocOfSearchString+length <= list.Count; startLocOfTestString++)
                    {
                        List<string> test = list.GetRange(length + startLocOfSearchString + startLocOfTestString, length);
                        if (ls.SequenceEqual<string>(test))
                        {
                            if (dict.ContainsKey(ls))
                            {
                                dict[ls].Item2.Add(length + startLocOfSearchString + startLocOfTestString);
                            } else
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

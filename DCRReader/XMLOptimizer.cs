using System;
using System.Collections.Generic;
using System.Linq;
using bpmntrails;

namespace DCRReader
{
    class XMLOptimizer
    {
        internal void Optimize(Processor graph, List<List<string>> trace, Dictionary<string, HashSet<Tuple<string, string>>> tree, BPMNTrail trail)
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
                }
            }
        }

        private Dictionary<List<string>, Tuple<int, List<int>>> FindRepeatingSequence(List<string> list)
        {
            Dictionary<List<string>, Tuple<int, List<int>>> dict = new Dictionary<List<string>, Tuple<int, List<int>>>();
            List<int> places;
            int maxLen = list.Count / 2;
            for(int i = 2; i<=maxLen; i++)
            {
                for(int j = 0; j+i<=list.Count-j-i; j++)
                {
                    List<string> ls = list.GetRange(j, i);
                    for(int k = 0; k+i+j+i <= list.Count; k++)
                    {
                        List<string> test = list.GetRange(i + j + k, i);
                        if (ls.SequenceEqual<string>(test))
                        {
                            if (dict.ContainsKey(ls))
                            {
                                dict[ls].Item2.Add(i + j + k);
                            } else
                            {
                                places = new List<int>
                                {
                                    i + j + k
                                };
                                dict[ls] = new Tuple<int, List<int>>(j, places);
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

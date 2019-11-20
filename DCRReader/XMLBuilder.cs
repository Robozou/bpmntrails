using System.Collections.Generic;
using bpmntrails;
using System;

namespace DCRReader
{
    class XMLBuilder
    {
        
        // make the trail in here
        BPMNTrail trail = new BPMNTrail();
        private Processor graph;
        private List<List<EventNode>> trace;
        private Dictionary<int, List<Tuple<string, int>>> tree;

        public XMLBuilder(Processor graph, List<List<EventNode>> trace)
        {
            this.graph = graph;
            this.trace = trace;
            this.tree = new Dictionary<int, List<Tuple<string, int>>>();
        }

        public void Build()
        {
            GrowTree();
        }

        private void GrowTree()
        {
            int state;
            string id;
            int newState;
            List<Tuple<string, int>> tuples;
            foreach (List<EventNode> ls in trace)
            {
                foreach (EventNode n in ls)
                {
                    state = graph.GetHashCode();
                    id = n.id;
                    graph.execute(id);
                    newState = graph.GetHashCode();
                    if (tree.ContainsKey(state))
                    {
                        tuples = tree[state];
                        tuples.Add(new Tuple<string, int>(id, newState));
                    }
                    else
                    {
                        tuples = new List<Tuple<string, int>>();
                        tuples.Add(new Tuple<string, int>(id, newState));
                    }
                    tree.Add(state, tuples);
                }
                graph.Load();
            }
        }
    }
}

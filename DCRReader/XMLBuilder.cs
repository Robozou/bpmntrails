using bpmntrails;
using System;
using System.Collections.Generic;

namespace DCRReader
{
    class XMLBuilder
    {
        int padding = 0;
        // make the trail in here
        BPMNTrail trail;
        private Processor graph;
        private List<List<string>> trace;
        private Dictionary<string, HashSet<Tuple<string, string>>> tree;
        private XMLOptimizer optimus;
        private Dictionary<string, string> idLabel;
        private Dictionary<string, string> labelId;

        public XMLBuilder(Processor graph, List<List<string>> trace, Dictionary<string, string> idLabel, Dictionary<string, string> labelId)
        {
            trail = new BPMNTrail();
            this.graph = graph;
            this.trace = trace;
            tree = new Dictionary<string, HashSet<Tuple<string, string>>>();
            optimus = new XMLOptimizer();
            this.idLabel = idLabel;
            this.labelId = labelId;
        }

        public void Optimize()
        {
            trail = optimus.Optimize(graph, trace, tree, trail, labelId);
        }

        public void Print()
        {
            trail.PrintTrail();
        }

        public void Build()
        {
            GrowTree();
            trail.AddStartEvent(graph.GetHashCode() + "");
            AddTasks();
            AddFlows();
        }

        private void AddFlows()
        {
            string current;
            foreach (string key in tree.Keys)
            {
                if (tree[key].Count > 1)
                {
                    current = "exgate" + padding;
                    trail.AddExclusiveGateway(current, false);
                    trail.AddSequenceFlow("seqflow" + padding, key + "", current);
                    padding++;
                }
                else
                {
                    current = key + "";
                }
                if (tree[key].Count > 0)
                {
                    foreach (Tuple<string, string> transition in tree[key])
                    {
                        trail.AddSequenceFlow("seqflow" + padding, current, transition.Item2 + "");
                        padding++;
                    }
                }
                else
                {
                    current = "end" + padding;
                    trail.AddEndEvent(current);
                    trail.AddSequenceFlow("seqflow" + padding, key + "", current);
                    padding++;
                }
            }
        }

        private void AddTasks()
        {
            foreach (string key in tree.Keys)
            {
                foreach (Tuple<string, string> transition in tree[key])
                {
                    if (!trail.ContainsEvent(transition.Item2 + ""))
                    {
                        trail.AddTask(transition.Item2 + "", idLabel[transition.Item1]);
                    }
                }
            }
        }

        private void GrowTree()
        {
            string state;
            string id;
            string newState;
            HashSet<Tuple<string, string>> tuples;
            foreach (List<string> ls in trace)
            {
                foreach (string n in ls)
                {
                    state = graph.GetHashCode();
                    id = n;
                    graph.Execute(id);
                    newState = graph.GetHashCode();
                    if (!tree.ContainsKey(newState))
                    {
                        tree[newState] = new HashSet<Tuple<string, string>>();
                    }
                    if (tree.ContainsKey(state))
                    {
                        tuples = tree[state];
                        tuples.Add(new Tuple<string, string>(id, newState));
                    }
                    else
                    {
                        tuples = new HashSet<Tuple<string, string>>
                        {
                            new Tuple<string, string>(id, newState)
                        };
                    }
                    tree[state] = tuples;
                }
                graph.Load();
            }
        }
    }
}

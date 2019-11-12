using bpmntrails;
using System;
using System.Collections.Generic;

namespace DotReader
{
    public class Builder : IBuilder
    {
        private List<Edge> edges;
        private List<Node> nodes;
        private BPMNTrail trail;
        private int _seqnum = -1;
        private int _splitnum = -1;
        private int _padding = -1;

        public Builder(List<Edge> edges, List<Node> nodes)
        {
            this.edges = edges;
            this.nodes = nodes;
            trail = new BPMNTrail();
        }

        public void Build()
        {
            trail.addStartEvent("start");
            string startNodeId = FindStartNode().id;
            List<string> tail = new List<string>();
            tail.Add(startNodeId);
            NextNode(startNodeId, "start", tail);
        }

        private void NextNode(string fromIdNode, string fromIdElement, List<string> tail)
        {
            string splitName;
            string taskId;
            string endName;
            if (fromIdNode == null || fromIdElement == null)
            {
                return;
            }

            List<Edge> outGoing = OutGoingEdges(FindNode(fromIdNode));
            outGoing = outGoing.FindAll(e => !IsToNode(FindNode(fromIdNode), e)); //Find way to handle slfloops
            outGoing.Sort((a, b) => { return FindNode(a.toId).timesUsed.CompareTo(FindNode(b.toId).timesUsed); });

            List<Node> n = new List<Node>();
            foreach(Edge e in outGoing)
            {
                n.Add(FindNode(e.toId));
            }
            n.ForEach(Console.WriteLine);
            Console.ReadLine();
            
            if (outGoing.Count > 1)
            {
                splitName = splitnum;
                trail.addExclusiveGateway(splitName, false);
                trail.addSequenceFlow(seqnum, fromIdElement, splitName);
                foreach (Edge e in outGoing)
                {
                    if (FindNode(tail[tail.Count-1]).timesUsed>0)
                    {
                        tail.ForEach(l => FindNode(l).timesUsed--);
                        taskId = (e.name + padding).Replace(" ", "");
                        trail.addTask(taskId, e.name);
                        trail.addSequenceFlow(seqnum, splitName, taskId);
                        tail.Add(e.toId);
                        NextNode(e.toId, taskId, tail);
                    }
                }
            }
            else if (outGoing.Count == 1)
            {
                if (FindNode(tail[tail.Count - 1]).timesUsed > 0)
                {
                    tail.ForEach(l => FindNode(l).timesUsed--);
                    taskId = (outGoing[0].name + padding).Replace(" ", "");
                    trail.addTask(taskId, outGoing[0].name);
                    trail.addSequenceFlow(seqnum, fromIdElement, taskId);
                    tail.Add(outGoing[0].toId);
                    NextNode(outGoing[0].toId, taskId, tail);
                }
            }
            else
            {
                endName = "end" + padding;
                trail.addEndEvent(endName);
                trail.addSequenceFlow(seqnum, fromIdElement, endName);
            }
        }

        private string seqnum
        {
            get
            {
                _seqnum++;
                return "seq" + _seqnum;
            }
        }

        private string splitnum
        {
            get
            {
                _splitnum++;
                return "split" + _splitnum;
            }
        }

        private int padding
        {
            get
            {
                _padding++;
                return _padding;
            }
        }

        private Node FindStartNode()
        {
            foreach (Node n in nodes)
            {
                Boolean hasNoInc = true;
                foreach (Edge e in edges)
                {
                    if (IsToNode(n, e))
                    {
                        hasNoInc = false;
                    }
                }
                if (hasNoInc)
                {
                    return n;
                }
            }
            return null;
        }

        private Node FindNode(string id)
        {
            return nodes.Find(n => n.id.Equals(id));
        }

        private List<Edge> OutGoingEdges(Node n)
        {
            return edges.FindAll(e => IsFromNode(n, e));
        }

        private Boolean IsEndNode(Node n)
        {
            return !edges.Exists(e => IsFromNode(n, e));
        }

        private Boolean IsFromNode(Node n, Edge e)
        {
            return e.fromId.Equals(n.id);
        }

        private Boolean IsToNode(Node n, Edge e)
        {
            return e.toId.Equals(n.id);
        }

        public void Print(string fileLoc)
        {
            trail.printTrail();
        }
    }
}

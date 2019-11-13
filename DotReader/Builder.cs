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
        string splitName;
        string taskId;
        string endName;
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
            List<string> newTail = new List<string>();
            if (fromIdNode == null || fromIdElement == null)
            {
                return;
            }

            List<Edge> outGoing = OutGoingEdges(FindNode(fromIdNode));
            outGoing = outGoing.FindAll(e => !IsToNode(FindNode(fromIdNode), e)); //Find way to handle slfloops
            outGoing.Sort((a, b) => { return FindNode(a.toId).timesUsed.CompareTo(FindNode(b.toId).timesUsed); });

            if (outGoing.Count > 0)
            {
                string from = fromIdElement;
                Boolean isSplit = false;
                if(outGoing.Count > 1)
                {
                    splitName = splitnum;
                    from = splitName;
                    isSplit = true;
                    trail.addExclusiveGateway(splitName, false);
                    trail.addSequenceFlow(seqnum, fromIdElement, splitName);
                }
                foreach (Edge e in outGoing)
                {
                    //if (FindNode(tail[tail.Count-1]).timesUsed>0)
                    //{
                    //tail.ForEach(l => FindNode(l).timesUsed--);
                    tail.ForEach(t => newTail.Add(t));
                    AddTask(e, from, newTail, isSplit);
                    //}
                }
            }
            else
            {
                endName = "end" + padding;
                trail.addEndEvent(endName);
                trail.addSequenceFlow(seqnum, fromIdElement, endName);
                tail.ForEach(l => FindNode(l).timesUsed--);
                tail.ForEach(Console.WriteLine);
                Console.ReadLine();
            }
        }

        private void AddTask(Edge e, string fromId, List<string> tail, Boolean isSplit)
        {
            Node n = FindNode(e.toId);
            if (
                 n.timesUsed == 0
                 ||
                 (n.timesUsed - 1 == tail.FindAll(t => t.Equals(n.id)).Count && !isSplit)
               )
            {
                return;
            }
            taskId = (e.name + padding).Replace(" ", "");
            trail.addTask(taskId, e.name);
            trail.addSequenceFlow(seqnum, fromId, taskId);
            if (!isSplit)tail.Add(e.toId);
            NextNode(e.toId, taskId, tail);
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

        //Todo make real print fucntion
        public void Print(string fileLoc)
        {
            trail.printTrail();
        }
    }
}

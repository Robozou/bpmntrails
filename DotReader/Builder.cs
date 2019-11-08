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
            NextNode(FindStartNode().id, "start");
        }

        private void NextNode(string fromIdNode, string fromIdElement)
        {
            if (fromIdNode == null || fromIdElement == null)
            {
                return;
            }

            List<Edge> outGoing = OutGoingEdges(FindNode(fromIdNode));
            outGoing = outGoing.FindAll(e => !IsToNode(FindNode(fromIdNode), e));
            if (outGoing.Count > 1)
            {
                string splitname = "split" + splitnum;
                trail.addParallelGateway(splitname, false);
                trail.addSequenceFlow(seqnum, fromIdElement, splitname);
                foreach (Edge e in outGoing)
                {
                    string id1 = e.name + padding;
                    trail.addTask(id1, e.name);
                    trail.addSequenceFlow(seqnum, splitname, id1);
                    NextNode(FindNode(e.toId).id, splitname);
                }
            }
            else if (outGoing.Count == 1)
            {
                string id2 = outGoing[0].name + padding;
                trail.addTask(id2, outGoing[0].name);
                trail.addSequenceFlow(seqnum, fromIdElement, id2);
                Node node = FindNode(outGoing[0].toId);
                NextNode(node.id, outGoing[0].name);
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

        private int splitnum
        {
            get
            {
                _splitnum++;
                return _splitnum;
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

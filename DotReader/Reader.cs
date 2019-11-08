using System;
using System.Collections.Generic;
using System.IO;

namespace DotReader
{
    public class Reader : IReader
    {
        private string[] dotFile;
        private List<Node> _nodes;
        private List<Edge> _edges;

        public Reader(string id = "node")
        {
            _nodes = new List<Node>();
            _edges = new List<Edge>();
        }

        public List<Edge> edges
        {
            get
            {
                return _edges;
            }
        }

        public List<Node> nodes
        {
            get
            {
                return _nodes;
            }
        }

        public void Read(string fileLoc)
        {
            ExtractData(fileLoc);
        }

        private void ExtractData(string fileLoc)
        {
            int brakLoc;
            int anLoc1;
            int anLoc2;
            int arrowLoc;
            dotFile = File.ReadAllLines(fileLoc);
            for (int i = 4; i < dotFile.Length - 1; i++)
            {
                if (!dotFile[i].Contains("->"))
                {
                    brakLoc = dotFile[i].IndexOf("[", StringComparison.Ordinal);
                    anLoc1 = dotFile[i].IndexOf("\"", StringComparison.Ordinal) + 1;
                    anLoc2 = (dotFile[i].Substring(anLoc1)).IndexOf("\"", StringComparison.Ordinal);
                    _nodes.Add(new Node(int.Parse(dotFile[i].Substring(anLoc1, anLoc2)), (dotFile[i].Substring(0, brakLoc).Replace(" ", ""))));
                }
                if (dotFile[i].Contains("->"))
                {
                    arrowLoc = dotFile[i].IndexOf("->", StringComparison.Ordinal);
                    brakLoc = (dotFile[i].Substring(arrowLoc)).IndexOf("[", StringComparison.Ordinal);
                    anLoc1 = dotFile[i].IndexOf("\"", StringComparison.Ordinal) + 1;
                    anLoc2 = (dotFile[i].Substring(anLoc1)).IndexOf("]", StringComparison.Ordinal) - 1;
                    _edges.Add(new Edge(dotFile[i].Substring(0, arrowLoc).Replace(" ", ""), dotFile[i].Substring(arrowLoc + 3, brakLoc - 3).Replace(" ", ""), dotFile[i].Substring(anLoc1, anLoc2)));
                }
            }
        }
    }
}

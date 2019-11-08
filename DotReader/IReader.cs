using System.Collections.Generic;

namespace DotReader
{
    public interface IReader
    {
        void Read(string fileLoc);
        List<Edge> edges { get; }
        List<Node> nodes { get; }
    }
}
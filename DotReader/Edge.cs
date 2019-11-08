namespace DotReader
{
    public class Edge
    {
        public string fromId { get; }
        public string toId { get; }
        public string name { get; }

        public Edge(string fromId, string toId, string name)
        {
            this.fromId = fromId;
            this.toId = toId;
            this.name = name;
        }
    }
}

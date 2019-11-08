namespace DotReader
{
    public class Node
    {
        public int timesUsed { get; }
        public string id { get; }

        public Node(int timesUsed, string id)
        {
            this.timesUsed = timesUsed;
            this.id = id;
        }
    }
}

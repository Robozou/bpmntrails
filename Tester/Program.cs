using DCRReader;

namespace Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            //IReader reader = new Reader();
            //reader.Read("..\\..\\..\\4013.txt");
            //IBuilder builder = new Builder(reader.edges, reader.nodes);
            //builder.Build();
            //builder.Print("");
            //Console.ReadLine();
            XMLReader xml = new XMLReader();
            xml.Read();
        }
    }
}

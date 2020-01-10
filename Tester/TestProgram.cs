using DCRReader;

namespace Tester
{
    class TestProgram
    {
        static void Main(string[] args)
        {
            XMLReader xml = new XMLReader();
            xml.Read("..\\..\\..\\..\\test.xml", "..\\..\\..\\testGraph.xml", "..\\..\\..\\testTrace.xml", true);
        }
    }
}

using DCRReader;
using System;

namespace Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            XMLReader xml = new XMLReader();
            xml.Read("..\\..\\..\\..\\test.xml", true);
        }
    }
}

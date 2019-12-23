using DCRReader;
using System;

namespace Tester
{
    class MainBoi
    {
        static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("--Must provide location of output, dcr graph and trace list, in that order.\n--And lastly add -f if you want the unoptimized result.");
                return;
            }
            string fileLoc = args[0];
            string dcrGraphLoc = args[1];
            string traceListLoc = args[2];
            bool optimize = true;
            if (args.Length >= 4 && args[3].Equals("-f"))
            {
                optimize = false;
            }
            XMLReader xml = new XMLReader();
            xml.Read(fileLoc, dcrGraphLoc, traceListLoc, optimize);
        }
    }
}

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;

namespace Getter
{
    class Program
    {
        static string[] lines = System.IO.File.ReadAllLines("..\\..\\..\\..\\..\\login.txt");
        static string testgraph = "4013";
        static void Main(string[] args)
        {
            GetGraphAndTrace();
        }

        private static void GetGraphAndTrace()
        {
            XmlDocument graph = GetGraph(testgraph);
            XmlDocument trace = GetTrace(testgraph);
            SaveXMLDoc(graph, "..\\..\\..\\..\\testGraph.xml");
            SaveXMLDoc(trace, "..\\..\\..\\..\\testTrace.xml");
        }

        private static void SaveXMLDoc(XmlDocument xml, string fileLoc)
        {
            xml.Save(fileLoc);
        }

        private static XmlDocument GetTrace(string graphId)
        {
            HttpWebRequest request = WebRequest.Create("https://repository.dcrgraphs.net/api/graphs/"+graphId+"/sims?format=DCRXMLLog&filter=exportlog&isScenario=true") as HttpWebRequest;
            request.Method = "GET";
            request.Headers["Authorization"] = Authorize();
            request.ContentLength = 0;
            string resp;
            XmlDocument xml = new XmlDocument();
            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                using (TextReader reader = new StreamReader(response.GetResponseStream()))
                {
                    resp = reader.ReadToEnd();
                    xml.LoadXml(resp);
                }
            }
            return xml;
        }

        private static XmlDocument GetGraph(string graphId)
        {
            HttpWebRequest request = WebRequest.Create("https://repository.dcrgraphs.net/api/graphs/" + graphId) as HttpWebRequest;
            request.Method = "GET";
            request.Headers["Authorization"] = Authorize();
            request.ContentLength = 0;
            string resp;
            XmlDocument xml = new XmlDocument();
            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                using (TextReader reader = new StreamReader(response.GetResponseStream()))
                {
                    resp = reader.ReadToEnd();
                    xml.LoadXml(resp);
                }
            }
            return xml;
        }

        private static XmlDocument GetGraphs()
        {
            HttpWebRequest request = WebRequest.Create("https://repository.dcrgraphs.net/api/graphs/") as HttpWebRequest;
            request.Method = "GET";
            request.Headers["Authorization"] = Authorize();
            request.ContentLength = 0;
            string resp;
            XmlDocument xml = new XmlDocument();
            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                using (TextReader reader = new StreamReader(response.GetResponseStream()))
                {
                    resp = reader.ReadToEnd();
                    xml.LoadXml(resp);
                }
            }
            return xml;
        }

        private static string Authorize()
        {
            return "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes(lines[0] + ":" + lines[1]));
        }
    }
}

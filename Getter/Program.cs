﻿using System;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;

namespace Getter
{
    class Program
    {
        static string TraceLoc = "..\\..\\..\\..\\testTrace.xml";
        static string GraphLoc = "..\\..\\..\\..\\testGraph.xml";
        static string testgraph = "13709";
        static string[] lines = File.ReadAllLines("..\\..\\..\\..\\..\\login.txt");
        static string locationOfGraphList = "..\\..\\..\\..\\..\\graphs.xml";
        static void Main(string[] args)
        {
            GetAndSaveGraphAndTrace();
        }

        private static XmlDocument GetGraphsWithTraces()
        {
            XmlDocument xml = new XmlDocument();
            if (File.Exists(locationOfGraphList))
            {
                xml.Load(locationOfGraphList);
            }
            else
            {
                xml = GetAndCacheGraphsWithTraces();
            }
            return xml;
        }

        private static XmlDocument GetAndCacheGraphsWithTraces()
        {
            XmlDocument graphs = GetGraphs();
            foreach (XmlElement graph in graphs.SelectNodes("//graph"))
            {
                XmlDocument trace = GetTrace(graph.Attributes["id"].Value);
                if (0 == trace.SelectNodes("/log/trace").Count)
                {
                    graph.ParentNode.RemoveChild(graph);
                }
            }
            SaveXMLDoc(graphs, locationOfGraphList);
            return graphs;
        }

        private static void GetAndSaveGraphAndTrace()
        {
            XmlDocument graph = GetGraph(testgraph);
            XmlDocument trace = GetTrace(testgraph);
            SaveXMLDoc(graph, GraphLoc);
            SaveXMLDoc(trace, TraceLoc);
        }

        private static void SaveXMLDoc(XmlDocument xml, string fileLoc)
        {
            xml.Save(fileLoc);
        }

        private static XmlDocument GetTrace(string graphId)
        {
            HttpWebRequest request = WebRequest.Create("https://repository.dcrgraphs.net/api/graphs/" + graphId + "/sims?format=DCRXMLLog&filter=exportlog&isScenario=true") as HttpWebRequest;
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

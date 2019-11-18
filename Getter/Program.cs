using System;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;

namespace Getter
{
    class Program
    {
        static void Main(string[] args)
        {
            //Console.WriteLine("Username:");
            //string user = Console.ReadLine();
            //Console.WriteLine("Password:");
            //string pswd = Console.ReadLine();
            string[] lines = System.IO.File.ReadAllLines("..\\..\\..\\..\\..\\login.txt");
            HttpWebRequest request = WebRequest.Create("https://repository.dcrgraphs.net/api/graphs/4013/sims?format=DCRXMLLog&filter=exportlog&isScenario=true") as HttpWebRequest;
            request.Method = "GET";
            request.Headers["Authorization"] = "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes(lines[0] + ":" + lines[1]));
            request.ContentLength = 0;
            string resp;
            XmlDocument xml = new XmlDocument();
            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                using (TextReader reader = new StreamReader(response.GetResponseStream()))
                {
                    resp = reader.ReadToEnd();
                    xml.LoadXml(resp);
                    xml.Save("..\\..\\..\\..\\testRead.xml");
                }
            }
        }
    }
}

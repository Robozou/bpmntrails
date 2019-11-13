using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;

namespace Getter
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Username:");
            string user = Console.ReadLine();
            Console.WriteLine("Password:");
            string pswd = Console.ReadLine();
            HttpWebRequest request = WebRequest.Create("https://repository.dcrgraphs.net/api/graphs/4013/sims?format=DCRXMLLog&filter=exportlog&isScenario=true") as HttpWebRequest;
            //HttpWebRequest request = WebRequest.Create("https://repository.dcrgraphs.net/api/graphs/4013/sims?format=XES&filter=exportlog") as HttpWebRequest;
            request.Method = "POST";
            request.Headers["Authorization"] = "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes(user + ":" + pswd));
            request.ContentLength = 0 ;
            string resp;
            using(HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                using(TextReader reader = new StreamReader(response.GetResponseStream()))
                {
                    JavaScriptSerializer js = new JavaScriptSerializer();
                }
                resp = ((new StreamReader(response.GetResponseStream())).ReadToEnd()).Replace("\\r\\n", "").Replace("\\\"", "\"").Replace("\"<events", "<events").Replace("</events>\"", "</events>").Replace(" =\"", "="); ;
            }

            (new StreamWriter("..\\..\\..\\..\\testRead.xml")).WriteLine(resp);
            
        }
    }
}

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _4chanThreadRipper
{
    static class Program
    {
        internal static Uri url;
        internal static string savePath;

        internal struct chanThread
        {
            public List<post> posts;
        }
        internal struct post
        {
            public int no;
            public int resto;
            public bool sticky;
            public bool closed;
            public string now;
            public int time;
            public string name;
            public string trip;
            public string id;
            public string capcode;
            public string country;
            public string country_name;
            public string sub;
            public string com;
            public long tim;
            public string filename;
            public string ext;
            public int fsize;
            public string md5;
            public int w;
            public int h;
            public int tn_w;
            public int tn_h;
            public bool filedeleted;
            public bool spoiler;
            public int custom_spoiler;
            public int replies;
            public int images;
            public bool bumplimit;
            public bool imagelimit;
            public string tag;
            public string semantic_url;
            public int since4pass;
            public int unique_ips;
            public bool m_img;
            public bool archived;
            public int archived_on;
        }

        static void Main()
        {
            Console.WriteLine("Insert thread Url:");
            url = new UriBuilder(Console.ReadLine()).Uri;
            chanThread t = getAllLinks(url);
            
            savePath = Path.Combine(Path.GetTempPath(),@"threadRip_" + url.Segments[3].Replace("/", ""));
            Console.WriteLine("Creating folder in temp");
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Downloading " + t.posts.Count + " Files...");
            Console.ForegroundColor = ConsoleColor.White;
            int count = 1;
            File.AppendAllText(Path.Combine(savePath, "index.txt"), "Url to Thread: " + url + Environment.NewLine);
            foreach (post s in t.posts)
            {
                if (!String.IsNullOrEmpty(s.filename))
                {
                    //async starts here
                    Console.Write("Starting Download [" + count + "] for " + s.filename + s.ext + "...");
                    string downloadUrl = String.Format("https://is2.4chan.org/{0}/{1}{2}", url.Segments[1].Replace("/", ""), s.tim, s.ext);
                    using (WebClient myWebClient = new WebClient())
                    {
                        ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                        myWebClient.DownloadFile(downloadUrl, Path.Combine(savePath, s.filename + s.ext));
                    }
                    File.AppendAllText(Path.Combine(savePath, "index.txt"), "File " + s.filename + s.ext + " downloaded" + Environment.NewLine);
                    Console.WriteLine("Finished!");
                }
                count++;
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("ThreadRip finished!");
            Thread.Sleep(TimeSpan.FromSeconds(2));
            Process.Start(savePath);
        }

        private static chanThread getAllLinks(Uri url)
        {

            List<string> ret = new List<string>();

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(String.Format("https://a.4cdn.org/{0}/thread/{1}.json", url.Segments[1].Replace("/", ""), url.Segments[3].Replace("/", "")));
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (String.IsNullOrEmpty(response.CharacterSet))
                {
                    readStream = new StreamReader(receiveStream);
                }
                else
                {
                    readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                }

                string data = readStream.ReadToEnd();

                response.Close();
                readStream.Close();

                return JsonConvert.DeserializeObject<chanThread>(data);
            }
            return new chanThread();
        }
    }
}

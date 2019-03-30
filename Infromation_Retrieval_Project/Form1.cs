using mshtml;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows.Forms;

namespace Infromation_Retrieval_Project
{
    public partial class Form1 : Form
    {
        ConcurrentHashSet<string> allLinks;
        ConcurrentQueue<string> links;
        ConcurrentQueue<KeyValuePair<string, IHTMLDocument2>> docs2;

        string MyConnection2 = @"Data Source=DESKTOP-4M6RSUD;Initial Catalog=ir;Integrated Security=True";

        private delegate void SafeCallDelegate(List<string> all, string countall, List<string> unvisited, string countUnVisited);

        HttpWebRequest myWebRequest;
        WebResponse myWebResponse;
        Stream streamResponse;
        StreamReader sReader;
        private System.Windows.Forms.Timer timer1;
        int seconds = 0;
        int minute = 0;
        int hours = 0;

        int count = 0;
        int unvisited = 0;

       int visited= 0;

        CancellationTokenSource cts = new CancellationTokenSource();

        Thread mainThread;

        Thread mainThread2;
        Thread mainThread3;
        Thread mainThread4;
        Thread mainThread5;

        string[] blacklistLinks = { "fb,", "facebook", "twitter", ".jpg", ".png", ".jpeg", "instagram", "rss", "Rss","youtube","mediawiki"};
        string baseurl = "https://en.wikipedia.org/wiki/Main_Page";

        public Form1()
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            stopBtn.Visible = false;
            Save.Visible = false;

            allLinks = new ConcurrentHashSet<string>();
           links = new ConcurrentQueue<string>();
            docs2 = new ConcurrentQueue<KeyValuePair<string, IHTMLDocument2>>();


        }
        private void button1_Click(object sender, EventArgs e)
        {
            stopBtn.Visible = true;
            string urll = baseurl;
            //allLinks.Add(urll);
            //unvisitedLinks.Add(urll);
            allLinks.Add(urll);
            links.Enqueue(urll);
            Logger.clear();
            mainThread2 = new Thread(threadWrapper);
            mainThread2.Start();
            mainThread3 = new Thread(threadWrapper);
            mainThread3.Start();
            mainThread = new Thread(threadWrapper2);
            mainThread.Start();
            mainThread4 = new Thread(threadWrapper);
            mainThread4.Start();
            mainThread5 = new Thread(threadWrapper);
            mainThread5.Start();
            


            timer1 = new System.Windows.Forms.Timer();
            timer1.Tick += new EventHandler(timer1_Tick);
            timer1.Interval = 1000; // 1 second
            timer1.Start();





        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            seconds++;
            if (seconds == 60)
            {
                minute++;
                seconds = 0;
            }
            if (minute == 60)
            {
                hours++;
                minute = 0;
            }
            label1.Text = hours + ":" + minute + ":" + seconds;
        }


        void threadF(String URL)
        {
            try
            {
                // Create a new 'WebRequest' object to the mentioned URL.

                WebRequest request = WebRequest.Create(URL);
                if (request is HttpWebRequest)
                {

                    myWebRequest = (HttpWebRequest)request;
                    myWebRequest.AllowAutoRedirect = true;
                    myWebResponse = myWebRequest.GetResponse();
                    streamResponse = myWebResponse.GetResponseStream();
                    sReader = new StreamReader(streamResponse);

                    string rString = sReader.ReadToEnd();

                    string[] splits2 = { "lang=" };
                    string[] metas = rString.Split(splits2, StringSplitOptions.None);
                    if (metas.Length == 1 || metas[1].Substring(1, 2) == "en")
                    {

                        IHTMLDocument2 doc = (IHTMLDocument2)new HTMLDocument();
                        doc.write(rString);

                        //   MessageBox.Show(doc.body.innerText);
                        if (doc.body.innerText != null&& doc.body.innerText.Trim().Length>50)
                        {
                            addInDB(count, URL, doc.body.innerText.Trim());
                            visited++;
                            unvisited--;


                        }
                        if (docs2.Count<50)
                        {
                            docs2.Enqueue(new KeyValuePair<string, IHTMLDocument2>(URL, doc));

                        }

                    }
                }
                
            }
            catch (Exception e)
            {
                Logger.Log(URL, e);
        
            }
            WriteTextSafe(null,links.Count.ToString(), null, Convert.ToString(visited));


            //streamResponse.Close();
            //sReader.Close();
            //myWebResponse.Close();
          

        }
        void threadWrapper()
        {
            while (true)
            {
                if (visited > 3200)
                {
                    break;
                }

                if (links.Count > 0)
                {
                    string url;
                    links.TryDequeue(out url);
                    if (url != null)
                    {


                       threadF(url);
                    }

                }

                else
                {
                    Thread.Sleep(500);
                }
            }

        }

        void threadWrapper2()
        {
            while (true)
            {
                if (links.Count > 3200)
                {
                    break;
                }

                if(links.Count>100)
                    Thread.Sleep(15000);


               else if (docs2.Count > 0)
                {
                    KeyValuePair<string, IHTMLDocument2> pair;
                    bool retuend=docs2.TryDequeue(out pair);
                    if (retuend)
                    {
                        parseDoc(pair.Key, pair.Value);

                    }

                }

                else
                {
                    Thread.Sleep(1000);
                }
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            cts.Cancel();
            streamResponse.Close();
            sReader.Close();
            myWebResponse.Close();
            timer1.Stop();
            mainThread.Abort();
            mainThread2.Abort();
            mainThread3.Abort();
            mainThread4.Abort();
            mainThread5.Abort();


            MessageBox.Show("Done");
            Save.Visible = true;

        }

        private void addInDB(int index, string url, string html)
        {
            try
            {
                //This is my connection string i have assigned the database file address path  
                //    string MyConnection2 = "Data Source=DESKTOP-4M6RSUD;Initial Catalog=ir;Integrated Security=True";
                
                //This is my insert query in which i am taking input from the user through windows forms  
                using (SqlConnection cnn = new SqlConnection(MyConnection2))
                {
                    string sql = "INSERT INTO main2 (path,[content]) VALUES(@url2,@content)";
                    cnn.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, cnn))
                    {
                 
                        cmd.Parameters.AddWithValue("@url2", url);

                        cmd.Parameters.AddWithValue("@content", html);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void clearDB()
        {
            try
            {
                //This is my connection string i have assigned the database file address path  

                //This is my insert query in which i am taking input from the user through windows forms  
                using (SqlConnection cnn = new SqlConnection(MyConnection2))
                {
                    string sql = "delete from main2 where 1=1";
                    cnn.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, cnn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void WriteTextSafe(List<string> all, string countall, List<string> visited, string countVisited)
        {
            if (visitedSize.InvokeRequired && allSize.InvokeRequired && listBox1.InvokeRequired && listBox2.InvokeRequired)
            {
                var d = new SafeCallDelegate(WriteTextSafe);
                Invoke(d, new object[] { all, countall, visited, countVisited });
            }
            else
            {
                visitedSize.Text = countVisited;
                allSize.Text = countall;
              //  listBox2.DataSource = visitedLinks.ToList();
               // listBox1.DataSource = all;

            }
        }

        private void Save_Click(object sender, EventArgs e)
        {
            //SaveUtil.save(allLinks, visitedLinks, unvisitedLinks,seconds,minute,hours);
        }

        private void restore_Click(object sender, EventArgs e)
        {

            DataviewModel model = SaveUtil.GetDataviewModel();
            if (model != null)
            {


                //visitedLinks = model.visitedLinks2;
                //unvisitedLinks = model.unvisitedLinks2;
                //seconds = model.second2;
                //minute = model.min2;
                //hours = model.hour2;
                //label1.Text = hours + ":" + minute + ":" + seconds;

                //WriteTextSafe(null,unvisitedLinks.Count.ToString(), null, visitedLinks.Count.ToString());
            }
            else
                MessageBox.Show("No Saved Data");
        }

        private void clear_Click(object sender, EventArgs e)
        {
            clearDB();

        }

        private void log_Click(object sender, EventArgs e)
        {
            Process.Start(@"log.txt");
        }
        private void parseDoc(string URL, IHTMLDocument2 doc)
        {
            try
            {
                Uri uri = null;
                IHTMLElementCollection elements = (IHTMLElementCollection)doc.links;
                if (Uri.IsWellFormedUriString(URL, UriKind.RelativeOrAbsolute))
                {
                    uri = new Uri(URL);
                }
                foreach (IHTMLElement el in elements)
                {

                    string temp = (string)el.getAttribute("href", 0);
                    Boolean breakLink = false;
                    foreach (string item in blacklistLinks)
                    {
                        if (temp.Contains(item))
                        {
                            breakLink = true;
                            break;
                        }
                    }
                    if (breakLink)
                    {
                        breakLink = false;
                        continue;
                    }
                    if (temp.StartsWith("http") || temp.StartsWith("/") || temp.StartsWith("https") || temp.StartsWith("about:"))
                    {
                        Logger.trace("1abuot---" + temp);

                        if (temp.StartsWith("/"))
                        {

                            temp = URL + temp;

                        }
                        else if (temp.Contains("blank#"))
                        {
                            temp = temp.Replace("about:blank", URL);
                        }
                        else if (temp.StartsWith("about:"))
                        {

                            if (temp.Contains("www.") || temp.Contains("//m.")||temp.Contains("en."))
                            {
                                if (temp.StartsWith("http"))
                                    temp = temp.Replace("about:", "");
                                else
                                    temp = temp.Replace("about:", "https:");


                            }
                            else
                            {
                                if (temp[6] == '/')
                                    temp = temp.Replace("about:", uri.GetLeftPart(System.UriPartial.Authority));
                                else
                                    temp = temp.Replace("about:", uri.GetLeftPart(System.UriPartial.Authority) + "/");


                            }



                        }
                        Logger.trace("2abuot---" + temp);

                        if (!allLinks.Contains(temp))
                        {
                            allLinks.Add(temp);
                            links.Enqueue(temp);
                            unvisited++;

                        }


                    }
                    else
                    {
                        Logger.Log("another-- " + temp);
                    }


                }
                }
            catch (Exception)
            {


            }


            
        }
    }
}

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
        ConcurrentBag<string> allLinks;
        ConcurrentBag<string> visitedLinks;
        ConcurrentBag<string> unvisitedLinks;
        string MyConnection2 = @"Data Source=(LocalDB)\MSSQLLocalDB;
                AttachDbFilename=|DataDirectory|\IR.mdf;
                Integrated Security=True;User Instance=false;";

        private delegate void SafeCallDelegate(List<string> all, string countall, List<string> unvisited, string countUnVisited);

        WebRequest myWebRequest;
        WebResponse myWebResponse;
        Stream streamResponse;
        StreamReader sReader;
        private System.Windows.Forms.Timer timer1;
        int seconds = 0;
        int minute = 0;
        int hours = 0;

        int count = 0;

        CancellationTokenSource cts = new CancellationTokenSource();

        Thread mainThread;

        Thread mainThread2;

        string[] blacklistLinks = { "fb,", "facebook", "twitter", ".jpg", ".png", ".jpeg", "instagram", "rss", "Rss" };
        string baseurl = "http://www.egypttoday.com";

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            stopBtn.Visible = false;
            Save.Visible = false;
           
            allLinks = new ConcurrentBag<string>();
            visitedLinks = new ConcurrentBag<string>();
            unvisitedLinks = new ConcurrentBag<string>();


        }
        private void button1_Click(object sender, EventArgs e)
        {
            stopBtn.Visible = true;
            string urll = "http://www.egypttoday.com";
            //allLinks.Add(urll);
            //unvisitedLinks.Add(urll);
            //urll = "http://www.cnn.com";
            allLinks.Add(urll);
            unvisitedLinks.Add(urll);
            mainThread = new Thread(threadWrapper);
            mainThread.Start();
            mainThread2 = new Thread(threadWrapper);
            mainThread2.Start();

            timer1 = new System.Windows.Forms.Timer();
            timer1.Tick += new EventHandler(timer1_Tick);
            timer1.Interval = 1000; // 1 second
            timer1.Start();
            
            backgroundWorker1.RunWorkerAsync();




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
                myWebRequest = WebRequest.Create(URL);
                // The response object of 'WebRequest' is assigned to a WebResponse' variable.
                myWebResponse = myWebRequest.GetResponse();
                streamResponse = myWebResponse.GetResponseStream();
                sReader = new StreamReader(streamResponse);
                string rString = sReader.ReadToEnd();
                HTMLDocument y = new HTMLDocument();
                IHTMLDocument2 doc = (IHTMLDocument2)y;
                doc.write(rString);
                //MessageBox.Show(doc.links.toString());
                if (allLinks.Count <= 3000)
                {
                    IHTMLElementCollection elements = doc.links;
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
                            if (temp.StartsWith("/"))
                            {
                                temp = URL + temp;
                            }
                            else if (temp.StartsWith("about:"))
                            {
                                Trace.WriteLine(temp);
                                temp = temp.Replace("about:", baseurl);
                            }
                            if (!allLinks.Contains(temp))
                            {
                                allLinks.Add(temp);
                                unvisitedLinks.Add(temp);
                            }
                        }



                    }
                }
                visitedLinks.Add(URL);
                addInDB(count, URL, doc.body.innerText);
            }
            catch
            {
                //count--;
            }
            //visitedSize.Text = visitedLinks.Count.ToString();
            //allSize.Text = allLinks.Count.ToString();
            ////listBox2.DataSource = null;
            //listBox2.DataSource = visitedLinks.ToList();
            //listBox1.DataSource = null;
            //listBox1.DataSource = allLinks.ToList();
            ////        break;
            WriteTextSafe(allLinks.ToList(), allLinks.Count.ToString(), visitedLinks.ToList(), visitedLinks.Count.ToString());


            //streamResponse.Close();
            //sReader.Close();
            //myWebResponse.Close();
            //MessageBox.Show("Done");
            //timer1.Stop();
        }
        void threadWrapper()
        {
            while (true)
            {
                if (visitedLinks.Count > 3000)
                {
                    break;
                }

                if (unvisitedLinks.Count > 0)
                {
                    string url;
                    unvisitedLinks.TryTake(out url);
                    threadF(url);
                }

                else
                {
                    break;
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
                    string sql = "INSERT INTO main2 (index2,path,[content]) VALUES(@index2,@url2,@content)";
                    cnn.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, cnn))
                    {
                        cmd.Parameters.AddWithValue("@index2", index);

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
                visitedSize.Text = visitedLinks.Count.ToString();
                allSize.Text = all.Count.ToString();
                listBox2.DataSource = visitedLinks.ToList();
                listBox1.DataSource = all;

            }
        }

        private void Save_Click(object sender, EventArgs e)
        {
            SaveUtil.save(allLinks, visitedLinks, unvisitedLinks,seconds,minute,hours);
        }

        private void restore_Click(object sender, EventArgs e)
        {

            DataviewModel model = SaveUtil.GetDataviewModel();
            if (model != null)
            {


                allLinks = model.allLinks2;
                visitedLinks = model.visitedLinks2;
                unvisitedLinks = model.unvisitedLinks2;
                seconds = model.second2;
                minute = model.min2;
                hours = model.hour2;
                label1.Text = hours + ":" + minute + ":" + seconds;

                WriteTextSafe(allLinks.ToList(), allLinks.Count.ToString(), visitedLinks.ToList(), visitedLinks.Count.ToString());
            }
            else
                MessageBox.Show("No Saved Data");
        }

        private void clear_Click(object sender, EventArgs e)
        {
            clearDB();

        }
    }
}

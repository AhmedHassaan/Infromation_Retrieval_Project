using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.IO;
using mshtml;
using System.Threading;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Collections.Concurrent;

namespace Infromation_Retrieval_Project
{
    public partial class Form1 : Form
    {
        ConcurrentBag<string> allLinks;
        ConcurrentBag<string> visitedLinks;
        ConcurrentBag<string> unvisitedLinks;
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
        string[] blacklistLinks = { "fb,", "facebook", "twitter", ".jpg", ".png", ".jpeg","instagram","rss","Rss"};
        string baseurl = "http://www.egypttoday.com";
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            stopBtn.Visible = false;
            clearDB();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            stopBtn.Visible = true;
            allLinks = new ConcurrentBag<string>();
            visitedLinks = new ConcurrentBag<string>();
            unvisitedLinks = new ConcurrentBag<string>();
            string urll = "http://www.egypttoday.com";
            //allLinks.Add(urll);
            //unvisitedLinks.Add(urll);
            //urll = "http://www.cnn.com";
            allLinks.Add(urll);
            unvisitedLinks.Add(urll);
            
            mainThread = new Thread(threadF);
            mainThread.Start();
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


        void threadF()
        {

            while (unvisitedLinks.Count > 0)
            {
                count++;
                String URL;
                unvisitedLinks.TryTake(out URL);
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
                        if (temp.StartsWith("http") || temp.StartsWith("/") || temp.StartsWith("https")||temp.StartsWith("about:"))
                        {
                            if (temp.StartsWith("/"))
                                temp = URL + temp;
                            else if (temp.StartsWith("about:")) {
                                Trace.WriteLine(temp);
                                temp=temp.Replace("about:", baseurl);
                            }
                            if (!allLinks.Contains(temp))
                            {
                                allLinks.Add(temp);
                                unvisitedLinks.Add(temp);
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
                visitedSize.Text = count.ToString();
                allSize.Text = allLinks.Count.ToString();
                listBox2.DataSource = null;
                listBox2.DataSource = visitedLinks.ToList();
                listBox1.DataSource = null;
                listBox1.DataSource = allLinks.ToList();
                if (count == 3000)
                    break;
            }
            streamResponse.Close();
            sReader.Close();
            myWebResponse.Close();
            MessageBox.Show("Done");
            timer1.Stop();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            mainThread.Abort();
            streamResponse.Close();
            sReader.Close();
            myWebResponse.Close();
            timer1.Stop();
            MessageBox.Show("Done");
        }

        private void addInDB(int index,string url, string html)
        {
            try
            {
                //This is my connection string i have assigned the database file address path  
                string MyConnection2 = "Data Source=DESKTOP-4M6RSUD;Initial Catalog=ir;Integrated Security=True";
                //This is my insert query in which i am taking input from the user through windows forms  
                using (SqlConnection cnn = new SqlConnection(MyConnection2))
                {
                    string sql = "INSERT INTO main2 (url2,[content]) VALUES(@url2,@content)";
                    cnn.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, cnn))
                    {
                        cmd.Parameters.AddWithValue("@url2",url);

                        cmd.Parameters.AddWithValue("@content",html);
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
                string MyConnection2 = "Data Source=DESKTOP-4M6RSUD;Initial Catalog=ir;Integrated Security=True";
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

    }
}

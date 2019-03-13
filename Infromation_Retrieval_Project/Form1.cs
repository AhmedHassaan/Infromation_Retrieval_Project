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
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;

namespace Infromation_Retrieval_Project
{
    public partial class Form1 : Form
    {
        List<string> allLinks;
        List<string> visitedLinks;
        List<string> unvisitedLinks;
        WebRequest myWebRequest;
        WebResponse myWebResponse;
        Stream streamResponse;
        StreamReader sReader;
        private System.Windows.Forms.Timer timer1;
        int seconds = 0;
        int minute = 0;
        int hours = 0;
        Thread mainThread;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            stopBtn.Visible = false;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            stopBtn.Visible = true;
            allLinks = new List<string>();
            visitedLinks = new List<string>();
            unvisitedLinks = new List<string>();
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

            int count = 0;
            while (unvisitedLinks.Count > 0)
            {
                count++;
                String URL = unvisitedLinks[0];
                unvisitedLinks.RemoveAt(0);
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
                        if (temp.StartsWith("http") || temp.StartsWith("/"))
                        {
                            if (temp.StartsWith("/"))
                                temp = URL + temp;
                            if (!allLinks.Contains(temp))
                            {
                                allLinks.Add(temp);
                                unvisitedLinks.Add(temp);
                            }
                        }

                    }
                    visitedLinks.Add(URL);
                    //addInDB(doc,count, URL, rString);
                }
                catch
                {
                    //count--;
                }
                visitedSize.Text = count.ToString();
                allSize.Text = allLinks.Count.ToString();
                listBox2.DataSource = null;
                listBox2.DataSource = visitedLinks;
                listBox1.DataSource = null;
                listBox1.DataSource = allLinks;
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

        private void addInDB(IHTMLDocument2 h,int index,string url, string html)
        {

        }
        private void clearDB()
        {

        }

    }
}

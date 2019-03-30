using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infromation_Retrieval_Project
{
    class Logger
    {
        public static void Log(string url,Exception e)
        {
            try
            {
                File.AppendAllText("log.txt", "\n e---" + ".." + url + ".." + e.StackTrace);

            }
            catch (Exception)
            {

            }
                   
            

        }
        public static void Log(string e)
        {
            try
            {
                File.AppendAllText("log.txt", "\n" + e);

            }
            catch (Exception)
            {

            }
        }
            public static void trace(string e)
            {
                try
                {
                    File.AppendAllText("links.txt", "\n" + e);

                }
                catch (Exception)
                {

                }

            }

        public static void clear()
        {
            File.Delete("log.txt");

        }
    }
}

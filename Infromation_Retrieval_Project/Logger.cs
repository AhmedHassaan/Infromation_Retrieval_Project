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
        public static void Log(Exception e)
        {
                File.AppendAllText("log.txt", "\n" + e.StackTrace);
                   
            

        }
        public static void Log(string e)
        {
            File.AppendAllText("log.txt", "\n" + e);



        }
    }
}

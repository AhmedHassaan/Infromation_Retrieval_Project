using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infromation_Retrieval_Project
{
    class SaveUtil
    {
        private static void saveFile(string json)
        {
            File.WriteAllText("ir.json", json);
        }
        public static void save(ConcurrentHashSet<string> allLinks,
        ConcurrentQueue<string> visitedLinks,
        ConcurrentQueue<string> unvisitedLinks,
        int second,
        int min,
        int hour
        )
        {
            var x = new DataviewModel
            {
                visitedLinks2 = visitedLinks,
                unvisitedLinks2 = unvisitedLinks,
                second2 = second,
            min2 = min,
            hour2=hour
            };
            var json =Newtonsoft.Json.JsonConvert.SerializeObject(x);
            saveFile(json);
        }
        public static DataviewModel GetDataviewModel()
        {
            if (File.Exists("ir.json"))
            {
                string json = File.ReadAllText("ir.json");
                return Newtonsoft.Json.JsonConvert.DeserializeObject<DataviewModel>(json);
            }
            else
                return null;
        }

    }

  public  class DataviewModel
    {
        public ConcurrentQueue<string> allLinks2;
        public ConcurrentQueue<string> visitedLinks2;
        public ConcurrentQueue<string> unvisitedLinks2;
        public int second2;
        public int min2;
        public int hour2;

    }
}

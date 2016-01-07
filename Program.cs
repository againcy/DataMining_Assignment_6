using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace DataMining_Assignment_6
{
    class Program
    {
        /*
        https://www.kaggle.com/c/prudential-life-insurance-assessment
        */
        static DataSet train, test;
        static void Input()
        {
            train = new DataSet();
            train.Input(@"G:\Data Mining\Assignment_6\train.csv", true);
            test = new DataSet();
            test.Input(@"G:\Data Mining\Assignment_6\test.csv", false);
            //test = train;
        }
        static void Test()
        {
            StreamWriter sw = new StreamWriter("test.txt");
            /*
            DecisionTree tree = new DecisionTree(train);
            for (int i = 0; i < train.FeatureName.Length; i++)
            {
                double IGR = tree.GetIGR(i);
                sw.WriteLine(train.FeatureName[i] + "," + IGR.ToString());
                Console.WriteLine(i.ToString());
            }*/
            for(int i = 0;i<train.FeatureName.Length;i++)
            {
                sw.WriteLine(train.FeatureName[i] + "," + train.FeatureIGR[i].ToString());
            }
            sw.Close();
        }
        
        /// <summary>
        /// 随机森林
        /// </summary>
        /// <param name="repeatTime">森林重复次数</param>
        /// <param name="n">每个森林的决策树个数</param>
        static void RandomForest(int repeatTime, int n)
        {
            for (int repeat = 0; repeat < repeatTime; repeat++)
            {
                string dirName = @"G:\Data Mining\Assignment_6\" + System.DateTime.Now.Date.ToLongDateString();
                if (Directory.Exists(dirName) == false) Directory.CreateDirectory(dirName);
                string fileName = dirName + @"\" + System.DateTime.Now.ToLongTimeString().Replace(":","_");
                bool uniform;
                if (repeat < repeatTime * 8 / 10) uniform = true;
                else uniform = false;
                StreamWriter sw = new StreamWriter(fileName + "_" + uniform.ToString() + ".txt");
                RandomForest forest = new RandomForest(train);
                forest.GenerateForest(n, 10, uniform);
                foreach (var ex in test.Examples)
                {
                    string detail;
                    int label = forest.Test(ex, out detail) + 1;
                    sw.WriteLine(detail);
                }
                sw.Close();
            }
        }

        /// <summary>
        /// 汇总结果
        /// </summary>
        static void CountResult(bool uniform)
        {
            string dirName = @"G:\Data Mining\Assignment_6\";
            //election[x][i]=j 表示第x个数据被分到i类一共j次
            Dictionary<int, Dictionary<int, int>> election = new Dictionary<int, Dictionary<int, int>>();
            string pattern;
            if (uniform == true) pattern = "*_True.txt";
            else pattern = "*_False.txt";
            foreach (var file in Directory.GetFiles(dirName, pattern, SearchOption.AllDirectories))
            {
                StreamReader sr = new StreamReader(file);
                string line;
                while ((line = sr.ReadLine())!=null)
                {
                    var data = line.Split(',').Select<string, int>(x => Convert.ToInt32(x)).ToArray();
                    if (election.ContainsKey(data[0]) == false)
                    {
                        election.Add(data[0], new Dictionary<int, int>());
                        for (int i = 1; i <= DecisionTree.MaxLabel; i++) election[data[0]].Add(i, 0);
                    }
                    for (int i = 1; i < data.Length; i++)
                    {
                        election[data[0]][data[i]]++;
                    }
                }
                sr.Close();
            }
            //输出结果
            StreamWriter sw = new StreamWriter(dirName + @"result.csv");
            sw.WriteLine("Id,Response");
            foreach(var id in election.Keys)
            {
                //选取最多投票的类
                int max = 0;
                int label = 1;
                for(int i=1;i<=DecisionTree.MaxLabel;i++)
                {
                    if (election[id][i]>max)
                    {
                        max = election[id][i];
                        label = i;
                    }
                }
                sw.WriteLine(id.ToString() + "," + label.ToString());
            }
        }
        static void Main(string[] args)
        {
            Input();
            RandomForest(2,2);
            //CountResult(true);
            //Test();
            Console.WriteLine("Over");
            Console.ReadLine();
        }
    }
}

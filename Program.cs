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
            
        static void RandomForest()
        {
            RandomForest forest = new RandomForest(train);
            forest.GenerateForest(50, 9);
            StreamWriter sw =new StreamWriter("result.csv");
            foreach(var ex in test.Examples)
            {
                sw.Write(ex.id.ToString() + ",");
                List<int> usedF;
                int label = forest.Test(ex,out usedF) + 1;
                sw.WriteLine(label.ToString());
                /*
                foreach (var f in usedF) sw.Write(f.ToString() + ",");
                sw.WriteLine(ex.label.ToString());
                */
            }
            sw.Close();
        }
        static void Main(string[] args)
        {
            Input();
            RandomForest();
            //Test();
            Console.WriteLine("Over");
            Console.ReadLine();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataMining_Assignment_6
{
    class RandomForest
    {
        private List<DecisionTree> forest;
        private DataSet dataset;
        private Random rand;
        public RandomForest(DataSet ds)
        {
            dataset = ds;
            forest = new List<DecisionTree>();
            rand = new Random();
        }
        private int MaxLabel;

        /// <summary>
        /// 生成随机森林，共n棵决策树，每次采样k个特征
        /// </summary>
        /// <param name="n">决策树的个数</param>
        /// <param name="k">每次采样的特征数</param>
        public void GenerateForest(int n, int k)
        {
            int progress = 10;
            Console.WriteLine(System.DateTime.Now.ToLongTimeString() + " RF Start...");
            for (int i = 0; i < n; i++)
            {
                DataSet newSet = new DataSet();
                newSet.FeatureType = dataset.FeatureType;
                //对训练集的行进行随机采样
                int N = dataset.Examples.Count;
                var arrEx = dataset.Examples.ToArray();
                for (int j = 0; j < N; j++)
                {
                    //有放回随机
                    newSet.AddExample(arrEx[rand.Next() % N]);
                }
                newSet.RecordDiscreteFeature();
                DecisionTree tree = new DecisionTree(newSet);
                MaxLabel = DecisionTree.MaxLabel;

                //对训练集的列进行随机采样（特征采样）
                int cntFeature = dataset.FeatureType.Length;
                Split newSplit = new Split(cntFeature);
                List<int> check = new List<int>();
                //随机选取=k个特征
                for (int j = 0; j < k; j++)
                {
                    int f = rand.Next() % cntFeature;
                    while (check.Contains(f)==true || dataset.FeatureIGR[f]<0.01) f = rand.Next() % cntFeature;//无放回随机
                    check.Add(f);
                }
                for(int j = 0;j<cntFeature;j++)
                {
                    if (check.Contains(j)==false) newSplit.splitOption[j] = 3;
                }
                tree.Root.split = new Split(newSplit);
                tree.GenerateSplit(tree.Root, 0);
                forest.Add(tree);
                if (i>n*(double)(progress)/100.0)
                {
                    Console.WriteLine(System.DateTime.Now.ToLongTimeString() + "  " + progress + "%");
                    progress += 10;
                }
            }
            Console.WriteLine(System.DateTime.Now.ToLongTimeString() + " RF End...");
        }

        /// <summary>
        /// 对data做测试
        /// </summary>
        /// <param name="data">测试数据</param>
        /// <param name="usedFeatures">使用的特征</param>
        /// <returns>可能性最大的类</returns>
        public int Test(Example data, out List<int> usedFeatures)
        {
            var usedFeatures_dict = new Dictionary<int,List<int>>();
            int[] label = new int[MaxLabel];
            for (int i = 0; i < MaxLabel; i++)
            {
                usedFeatures_dict.Add(i, new List<int>());
            }
            
            label.Initialize();
            foreach (var tree in forest)
            {
                List<int> usedF;
                int l = tree.Test(data, out usedF);
                label[l]++;
                foreach (var f in usedF) if (usedFeatures_dict[l].Contains(f) == false) usedFeatures_dict[l].Add(f);
            }
            int max = label[0];
            int result = 0; 
            for(int i=1;i<MaxLabel;i++)
            {
                if (label[i]>max)
                {
                    max = label[i];
                    result = i;
                }
            }
            usedFeatures = usedFeatures_dict[data.label];
            return result;
        }

    }
}

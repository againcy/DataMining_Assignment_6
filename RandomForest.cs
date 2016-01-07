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
        private Dictionary<int, DataSet> dataset_label;//按类标存储的数据集
        private Random rand;
        public RandomForest(DataSet ds)
        {
            dataset = ds;
            forest = new List<DecisionTree>();
            rand = new Random();
            dataset_label = null;
            
        }
        private int MaxLabel;

        /// <summary>
        /// 将数据集按类标存放
        /// </summary>
        private void Generate_dataset_label()
        {
            dataset_label = new Dictionary<int, DataSet>();
            foreach (var ex in dataset.Examples)
            {
                int l = ex.label;
                if (dataset_label.ContainsKey(l) == false)
                {
                    dataset_label.Add(l, new DataSet());
                }
                dataset_label[l].AddExample(ex);
            }
        }
        /// <summary>
        /// 对训练集的行进行随机采样
        /// </summary>
        /// <param name="uniform">是否按照类标均匀采样</param>
        /// <returns>采样后的训练集</returns>
        private DataSet SampleExample(bool uniform)
        {
            //对训练集的行进行随机采样
            DataSet newSet = new DataSet();
            newSet.FeatureType = dataset.FeatureType;

            if (uniform == true)
            {
                if (dataset_label == null) Generate_dataset_label();
                int NperClass = dataset.Examples.Count / MaxLabel;
                //按类标均匀采样，每个类包含的数据量相等
                for (int i = 0; i < MaxLabel; i++)
                {
                    var arrEx = dataset_label[i].Examples.ToArray();
                    for (int j = 0; j < NperClass; j++)
                    {
                        //有放回随机
                        newSet.AddExample(arrEx[rand.Next() % arrEx.Length]);
                    }
                }
            }
            else
            {
                //随机采样
                int N = dataset.Examples.Count;
                var arrEx = dataset.Examples.ToArray();
                for (int j = 0; j < N; j++)
                {
                    //有放回随机
                    newSet.AddExample(arrEx[rand.Next() % N]);
                }
                newSet.RecordDiscreteFeature();
            }
            return newSet;
        }

        /// <summary>
        /// 对训练集列进行随机采样（特征采样）
        /// </summary>
        /// <param name="cntFeature">特征数</param>
        /// <param name="k">采样特征数</param>
        /// <returns></returns>
        private Split SampleFeature(int cntFeature, int k)
        {
            Split newSplit = new Split(cntFeature);
            List<int> check = new List<int>();
            //随机选取=k个特征
            for (int j = 0; j < k; j++)
            {
                int f = rand.Next() % cntFeature;
               // while (check.Contains(f) == true || dataset.FeatureIGR[f] < 0.01) f = rand.Next() % cntFeature;//无放回随机
                while (check.Contains(f) == true) f = rand.Next() % cntFeature;//无放回随机
                check.Add(f);
            }
            for (int j = 0; j < cntFeature; j++)
            {
                if (check.Contains(j) == false) newSplit.splitOption[j] = 3;
            }
            return newSplit;
        }

        /// <summary>
        /// 生成随机森林，共n棵决策树，每次采样k个特征
        /// </summary>
        /// <param name="n">决策树的个数</param>
        /// <param name="k">每次采样的特征数</param>
        /// <param name="uniformSample">是否保证每个类采样后数据量相同</param>
        public void GenerateForest(int n, int k, bool uniformSample)
        {
            int progress = 10;
            Console.WriteLine(System.DateTime.Now.ToLongTimeString() + " RF Start...");
            MaxLabel = DecisionTree.MaxLabel;
            for (int i = 0; i < n; i++)
            {
                //行采样
                DataSet newSet = SampleExample(uniformSample);
                //列采样
                Split newSplit = SampleFeature(dataset.FeatureType.Length, k);
                //生成决策树
                DecisionTree tree = new DecisionTree(newSet);
                tree.Root.split = new Split(newSplit);
                tree.GenerateSplit(tree.Root, 0);
                //加入森林
                forest.Add(tree);
                //汇报进度
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
        /// <param name="detailResult">结果细节，包含每棵树的预测结果</param>
        /// <returns>可能性最大的类</returns>
        public int Test(Example data, out string detailResult)
        {
            detailResult = data.id.ToString();
            int[] label = new int[MaxLabel];
            foreach (var tree in forest)
            {
                int l = tree.Test(data);
                detailResult += "," + (l + 1).ToString();
                label[l]++;
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
            return result;
        }

    }
}

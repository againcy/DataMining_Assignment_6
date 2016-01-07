using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace DataMining_Assignment_6
{
    /// <summary>
    /// 数据集类
    /// </summary>
    class DataSet
    {
        /// <summary>
        /// 数据
        /// </summary>
        public List<Example> Examples
        {
            get
            {
                return examples;
            }
            set
            {
                examples = value;
            }
        }
        private List<Example> examples;

        /// <summary>
        /// 特征的名称
        /// </summary>
        public string[] FeatureName
        {
            get
            {
                return featureName;
            }
            set
            {
                featureName = value;
            }
        }
        private string[] featureName;

        /// <summary>
        /// 特征的类型1:离散, 0:连续
        /// </summary>
        public int[] FeatureType
        {
            get
            {
               return featureType;
            }
            set
            {
                featureType = value;
            }
        }
        private int[] featureType;

        /// <summary>
        /// 单独按特征划分的信息熵增益率
        /// </summary>
        public double[] FeatureIGR
        {
            get
            {
                return featureIGR;
            }
            set
            {
                featureIGR = value;
            }
        }
        private double[] featureIGR;

        /// <summary>
        /// 记录每个离散特征的取值
        /// </summary>
        public List<double>[] DiscreteFeature
        {
            get
            {
                return discreteFeature;
            }
            set
            {
                discreteFeature = value;
            }
        }
        private List<double>[] discreteFeature;

        public DataSet()
        {
            examples = new List<Example>();
        }

        /// <summary>
        /// 添加一个数据
        /// </summary>
        /// <param name="ex"></param>
        public void AddExample(Example ex)
        {
            examples.Add(ex);
        }

        /// <summary>
        /// 设置每个特征的类型（离散 连续）
        /// </summary>
        private void SetFeatureType()
        {
            string[] continuous = new string[11]{"Product_Info_4",
                                                "Ins_Age Ht",
                                                "Wt",
                                                "BMI",
                                                "Employment_Info_1",
                                                "Employment_Info_6",
                                                "Insurance_History_5",
                                                "Family_Hist_2",
                                                "Family_Hist_3",
                                                "Family_Hist_4",
                                                "Family_Hist_5" };
            for (int i = 0; i < featureType.Length; i++)
            {
                if (continuous.Contains(featureName[i]) == true)
                {
                    featureType[i] = 0;
                }
                else
                {
                    featureType[i] = 1;
                }
            }
        }

        /// <summary>
        /// 记录每个离散特征的取值
        /// </summary>
        public void RecordDiscreteFeature()
        {
            discreteFeature = new List<double>[featureType.Length];
            for (int i = 0; i < featureType.Length; i++)
            {
                if (featureType[i] == 1)
                {
                    discreteFeature[i] = new List<double>();
                    foreach (var d in examples)
                    {
                        if (discreteFeature[i].Contains(d.features[i]) == false) discreteFeature[i].Add(d.features[i]);
                    }
                }
            }
        }

        private void GetIGR()
        {
            var curSet = this;
            double E_raw = DecisionTree.Entropy(curSet);
            featureIGR = new double[featureType.Length];
            for (int splitFeature = 0; splitFeature < featureType.Length; splitFeature++)
            {
                if (curSet.FeatureType[splitFeature] == 1)
                {
                    //离散特征 
                    double H;
                    double E_split = DecisionTree.SplitEntropy_Discrete(curSet, splitFeature, out H);
                    featureIGR[splitFeature] = (E_raw - E_split) / H;
                }
                else
                {
                    //连续特征
                    double threshold;
                    double H;
                    double E_split = DecisionTree.SplitEntropy_Numeric(curSet, splitFeature, out threshold, out H);
                    featureIGR[splitFeature] = (E_raw - E_split) / H;
                }
            }
        }
        /// <summary>
        /// 对数据集的预处理
        /// </summary>
        private void PreProcess()
        {
            SetFeatureType();
            RecordDiscreteFeature();
           // GetIGR();
        }

        /// <summary>
        /// 读入数据集
        /// </summary>
        /// <param name="file">文件路径</param>
        /// <param name="hasLabel">是否需要读取类标</param>
        public void Input(string file, bool hasLabel)
        {
            StreamReader sr = new StreamReader(file);
            
            string line = sr.ReadLine();
            //读取表头 即特征名称
            featureName = line.Split(',').Skip(1).Take(126).ToArray();
            featureType = new int[featureName.Length];
            while ((line = sr.ReadLine()) != null)
            {
                Example ex = new Example();
                string[] data = line.Split(',').ToArray();
                
                ex.id = Convert.ToInt32(data[0]);
                if (hasLabel == true)
                {
                    ex.label = Convert.ToInt32(data[data.Length - 1]) - 1;
                    ex.features = new double[data.Length - 2];
                }
                else
                {
                    ex.features = new double[data.Length - 1];
                }
                for (int i = 1; i < data.Length - 1; i++)
                {
                    if (data[i] == "") ex.features[i-1] = -1;
                    else
                    {
                        if (i == 2)
                        {
                            //Product_Info_2
                            ex.features[i-1] = (data[i][0] - 'A') * 16 + data[i][1];
                        }
                        else
                        {
                            ex.features[i-1] = Convert.ToDouble(data[i]);
                        }
                    }
                }
                
               
                examples.Add(ex);
            }
            sr.Close();
            PreProcess();
        }
    }
}

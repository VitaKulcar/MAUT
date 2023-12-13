using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAUT
{
    internal class Alternative
    {
        public string Name { get; set; }
        public Dictionary<EndNode, double> EndValues { get; set; }
        public Dictionary<EndNode, double> EndUtility { get; set; }
        public Dictionary<TreeNode, double> FinalUtility { get; set; }
        public double Utility { get; set; }

        public Alternative() { }

        public Alternative(string name, Dictionary<EndNode, double> endValues)
        {
            Name = name;
            EndValues = endValues;
            EndUtility = new();
            foreach (var v in EndValues)
                EndUtility[v.Key] = utilityCalculation(v.Value, v.Key);
        }

        double utilityCalculation(double x, EndNode node)
        {
            switch (node.SelectedFunction)
            {
                case "logaritemska":
                    double logMin = Math.Log10(node.MinValue);
                    double logMax = Math.Log10(node.MaxValue);
                    double logValue = Math.Log10(x);
                    return (logValue - logMin) / (logMax - logMin);
                case "padajoča logaritemska":
                    double logMin1 = Math.Log10(node.MinValue);
                    double logMax1 = Math.Log10(node.MaxValue);
                    double logValue1 = Math.Log10(x);
                    double normalizedValue2 = (logValue1 - logMin1) / (logMax1 - logMin1);
                    return 1 - normalizedValue2;
                case "eksponentna":
                    double range = node.MaxValue - node.MinValue;
                    double normalizedValue = (x - node.MinValue) / range;
                    return normalizedValue * normalizedValue;
                case "padajoča eksponentna":
                    double range1 = node.MaxValue - node.MinValue;
                    double normalizedValue1 = (x - node.MinValue) / range1;
                    return 1 - normalizedValue1 * normalizedValue1;
                case "linearna":
                    double slope = 1 / (node.MaxValue - node.MinValue);
                    double intercept = -node.MinValue * slope;
                    return slope * x + intercept;
                default: return 0;
            }
        }
    }
}

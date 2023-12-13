using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MAUT
{
    internal class EndNode
    {
        public string Name { get; set; }
        public double MinValue { get; set; }
        public double MaxValue { get; set; }
        public string SelectedFunction { get; set; }

        public EndNode(string name, double minValue, double maxValue, string selectedFunction)
        {
            Name = name;
            MinValue = minValue;
            MaxValue = maxValue;
            SelectedFunction = selectedFunction;
        }

        public double utilityCalculation(double x)
        {
            switch (SelectedFunction)
            {
                case "logaritemska":
                    return Math.Log(x / MinValue) / Math.Log(MaxValue / MinValue);
                case "padajoča logaritemska":
                    return Math.Log(MinValue / x) / Math.Log(MaxValue / MinValue);
                case "eksponentna":
                    double range = MaxValue - MinValue;
                    double normalizedValue = (x - MinValue) / range;
                    return normalizedValue * normalizedValue;
                case "padajoča eksponentna":
                    double range1 = MaxValue - MinValue;
                    double normalizedValue1 = (x - MinValue) / range1;
                    return 1 - normalizedValue1 * normalizedValue1;
                case "linearna":
                    double slope = 1 / (MaxValue - MinValue);
                    double intercept = -MinValue * slope;
                    return slope * x + intercept;
                default: return 0;
            }
        }
    }
}

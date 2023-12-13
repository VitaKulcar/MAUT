using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MAUT
{
    partial class AlternativeWindow : Window
    {
        private List<EndNode> EndNodes { get; set; }
        internal delegate void AddAlternative(Alternative a);
        internal event AddAlternative addAlternative;

        internal AlternativeWindow(List<EndNode> endNodes)
        {
            InitializeComponent();
            EndNodes = endNodes;

            StackPanel panel = new StackPanel();
            TextBlock text = new();
            text.Text = "Vpišite naziv alternative";
            TextBox box = new();
            box.Name = "nazivAlternative";
            panel.Children.Add(text);
            panel.Children.Add(box);
            form.Children.Add(panel);

            foreach (var node in EndNodes)
            {
                StackPanel stackPanel = new StackPanel();

                TextBlock textBlock = new();
                textBlock.Text = "\nVnesite vrednost za parameter " + node.Name;

                TextBlock textBlock1 = new();
                textBlock1.Text = "Domena parametra: od " + node.MinValue + " do " + node.MaxValue;

                TextBox textBox = new();
                textBox.Name = node.Name;

                stackPanel.Children.Add(textBlock);
                stackPanel.Children.Add(textBlock1);
                stackPanel.Children.Add(textBox);
                form.Children.Add(stackPanel);
            }
        }

        private void StopAlternativeAdding(object sender, RoutedEventArgs e)
        {

            string anser = null;

            Dictionary<EndNode, double> values = new();

            for (int i = 1; i < form.Children.Count; i++)
            {
                StackPanel stackPanel = (StackPanel)form.Children[i];
                TextBox textBlock = (TextBox)stackPanel.Children[2];
                anser = validation(textBlock.Text, textBlock.Name);
                if (anser != null)
                {
                    double.TryParse(anser, out double value);
                    values.Add(EndNodes[i - 1], value);
                }
            }

            if (anser != null)
            {
                StackPanel panel = (StackPanel)form.Children[0];
                TextBox box = (TextBox)panel.Children[1];

                Alternative a = new Alternative(box.Text, values);
                addAlternative?.Invoke(a);
                DialogResult = true;
            }
        }

        private string validation(string textNumber, string name)
        {
            double value = 0;
            var parseSucess = double.TryParse(textNumber, out value);
            if (parseSucess)
            {
                var node = EndNodes.Find(x => x.Name == name);
                if (value >= node.MinValue && value <= node.MaxValue)
                    return value.ToString();
                else
                {
                    MessageBox.Show("Vnesena vrednost za parameter " + name + " presega domeno.");
                    return null;
                }
            }
            else
            {
                MessageBox.Show("Vnesena vrednost za parameter " + name + " ni veljavno število.");
                return null;
            }
        }
    }
}

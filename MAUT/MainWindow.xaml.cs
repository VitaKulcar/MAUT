using Newtonsoft.Json;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MAUT
{
    public partial class MainWindow : Window
    {
        ObservableCollection<TreeNode> TreeNodes { get; set; }
        private List<EndNode> EndNodes { get; set; }
        private List<Node> Nodes { get; set; }
        private ObservableCollection<Alternative> Alternatives { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            step2.Visibility = Visibility.Collapsed;
            step3.Visibility = Visibility.Collapsed;

            TreeNodes = new();
            EndNodes = new();
            Nodes = new();
            Alternatives = new();
            tree.ItemsSource = TreeNodes;

            DataContext = this;
            PlotModel plotModel = new();
            plotView.Model = plotModel;

            PlotModel plotModel1 = new();
            var axis = new CategoryAxis { Position = AxisPosition.Left };
            plotModel1.Axes.Add(axis);
            var yAxis = new LinearAxis { Position = AxisPosition.Bottom, Minimum = 0, Maximum = 1 };
            plotModel1.Axes.Add(yAxis);
            finalUtility.Model = plotModel1;
        }

        private void addParameter(object sender, RoutedEventArgs e)
        {
            if (tree.Items.Count == 0)
            {
                string name = parameterName.Text;
                parameterName.Text = null;
                TreeNode newNode = new(name);
                TreeNodes.Add(newNode);
            }
            else
            {
                if (tree.SelectedItem == null)
                {
                    MessageBox.Show("Izberite parameter iz katerega želite izpeljali naslednji parameter.");
                }
                else
                {
                    TreeNode selectedNode = (TreeNode)tree.SelectedItem;
                    string name = parameterName.Text;
                    parameterName.Text = null;
                    TreeNode newNode = new(name);
                    selectedNode.Children.Add(newNode);

                    checkTree(null, null);
                }
            }
        }

        private void checkTree(object sender, RoutedEventArgs e)
        {
            List<TreeNode> finalElements = new();
            List<TreeNode> otherElements = new();
            notifications.Text = "";

            foreach (TreeNode rootNode in TreeNodes)
            {
                checkNode(rootNode, finalElements, otherElements);
            }

            EndNodes.Clear();
            foreach (var node in finalElements)
            {
                EndNodes.Add(new EndNode(node.Name, 0, 0, ""));
            }
            Nodes.Clear();
            foreach (var node in otherElements)
            {
                bool containsNode = false;
                foreach (var n in Nodes)
                {
                    if (n.Name == node.Name)
                    {
                        containsNode = true;
                        break;
                    }
                }
                if (!containsNode)
                {
                    Dictionary<string, double> dic = new Dictionary<string, double>();
                    foreach (var x in node.Children)
                    {
                        dic.Add(x.Name, 0);
                    }
                    Nodes.Add(new Node(node.Name, dic));
                }
            }
        }

        private void checkNode(TreeNode node, List<TreeNode> finalElements, List<TreeNode> otherElements)
        {
            if (node.Children.Count == 0)
            {
                finalElements.Add(node);
            }
            else if (node.Children.Count == 1)
            {
                notifications.Text += $"\nParameter {node.Name} ne more imeti samo enega podrednega parametra";

                foreach (TreeNode childNode in node.Children)
                {
                    checkNode(childNode, finalElements, otherElements);
                }
            }
            else if (node.Children.Count > 1)
            {
                foreach (TreeNode childNode in node.Children)
                {
                    otherElements.Add(node);
                    checkNode(childNode, finalElements, otherElements);
                }
            }
        }

        private void saveTree(object sender, RoutedEventArgs e)
        {
            JsonSerializer serializer = new();
            using (StreamWriter streamWriter = new("tree.json"))
            using (JsonTextWriter jsonWriter = new(streamWriter))
            {
                serializer.Serialize(jsonWriter, TreeNodes);
            }
        }

        private void loadTree(object sender, RoutedEventArgs e)
        {
            try
            {
                JsonSerializer serializer = new();
                using (StreamReader streamReader = new("tree.json"))
                using (JsonTextReader jsonReader = new(streamReader))
                {
                    ObservableCollection<TreeNode> nodes = serializer.Deserialize<ObservableCollection<TreeNode>>(jsonReader);
                    TreeNodes = nodes;
                    tree.ItemsSource = TreeNodes;
                }
                checkTree(null, null);
            }
            catch(Exception ex)
            {

                MessageBox.Show("Ni mogoče naložiti drevesa. Ustvarite novo drevo.");
            }
        }

        private void stopTreeEditing(object sender, RoutedEventArgs e)
        {
            checkTree(null, null);

            if (TreeNodes.Count > 2)
            {
                MessageBox.Show("Ne morete nadaljevati brez drevesa. Drevo rabi imeti vsaj več kot 2 parametra.");
            }
            else if (notifications.Text != "")
            {
                MessageBox.Show("Ne morete nadaljevati zaradi napak v drevesu.");
            }
            else
            {
                topButtons.Visibility = Visibility.Collapsed;
                bottomButtons.Visibility = Visibility.Collapsed;
                step2.Visibility = Visibility.Visible;

                utilityFunctionTable.ItemsSource = EndNodes;
                weightsForSuperiorParameters.ItemsSource = Nodes;

                //koristnosti
                List<AlternativesTableDisplay> alternativesTableDisplays = new();
                List<string> list = new();
                list = addAllTreeNodesInList(list, TreeNodes.First());
                alternativesTableDisplays.Add(new("parametri", list));
                foreach (var d in alternativesTableDisplays)
                {
                    DataGridTextColumn col = new();
                    col.Width = 130;
                    col.Header = d.Header;

                    Binding binding = new Binding();
                    binding.Converter = new ListToStringConverter();
                    binding.Source = d.Values;
                    col.Binding = binding;

                    alternativesTable.Columns.Add(col);
                }
                alternativesTable.ItemsSource = alternativesTableDisplays;

                //vrednosti
                List<AlternativesTableDisplay> alternativesTableDisplaysVal = new();
                List<string> listVal = new();
                listVal = addAllEndNodesInList(listVal, TreeNodes.First());
                alternativesTableDisplaysVal.Add(new("parametri", listVal));
                foreach (var d in alternativesTableDisplaysVal)
                {
                    DataGridTextColumn col = new();
                    col.Width = 130;
                    col.Header = d.Header;

                    Binding binding = new Binding();
                    binding.Converter = new ListToStringConverter();
                    binding.Source = d.Values;
                    col.Binding = binding;

                    alternativesValuesTable.Columns.Add(col);
                }
                alternativesValuesTable.ItemsSource = alternativesTableDisplaysVal;
            }
        }

        private void domainChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            DataGridRow dataGridRow = findVisualParent<DataGridRow>(textBox);
            if (dataGridRow != null)
            {
                ComboBox comboBox = findVisualChild<ComboBox>(dataGridRow, "selectedFunction");
                if (comboBox != null)
                {
                    comboBox.SelectedIndex = -1;
                }
            }
        }

        private void selectedFunctionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems != null && e.AddedItems.Count > 0)
            {
                ComboBoxItem item = (ComboBoxItem)e.AddedItems[0];
                string selectedFunction = (string)item.Content;

                ComboBox comboBox = (ComboBox)sender;
                DataGridRow dataGridRow = findVisualParent<DataGridRow>(comboBox);
                if (dataGridRow != null)
                {
                    TextBox textFrom = findVisualChild<TextBox>(dataGridRow, "textFrom");
                    TextBox textTo = findVisualChild<TextBox>(dataGridRow, "textTo");

                    if (textFrom != null && textTo != null)
                    {
                        string name = "";
                        double fromValue = 0;
                        double toValue = 0;
                        if (double.TryParse(textFrom.Text, out fromValue) && double.TryParse(textTo.Text, out toValue) && fromValue < toValue)
                        {
                            EndNode endNode = (EndNode)dataGridRow.DataContext;
                            EndNodes.Remove(endNode);

                            switch (selectedFunction)
                            {
                                case "logaritemska":
                                    calculateFunction(fromValue, toValue, "logaritemska");
                                    name = "logaritemska";
                                    break;
                                case "padajoča logaritemska":
                                    calculateFunction(fromValue, toValue, "padajoča logaritemska");
                                    name = "padajoča logaritemska";
                                    break;
                                case "eksponentna":
                                    calculateFunction(fromValue, toValue, "eksponentna");
                                    name = "eksponentna";
                                    break;
                                case "padajoča eksponentna":
                                    calculateFunction(fromValue, toValue, "padajoča eksponentna");
                                    name = "padajoča eksponentna";
                                    break;
                                case "linearna":
                                    calculateFunction(fromValue, toValue, "linearna");
                                    name = "linearna";
                                    break;
                            }

                            endNode.MinValue = fromValue;
                            endNode.MaxValue = toValue;
                            endNode.SelectedFunction = name;
                            EndNodes.Add(endNode);
                        }
                        else
                        {
                            MessageBox.Show("Napaka pri vnosu domene.");
                        }
                    }
                }
            }
        }

        private static T findVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parent = VisualTreeHelper.GetParent(child);
            if (parent == null)
            {
                return null;
            }
            T parentT = parent as T;
            return parentT ?? findVisualParent<T>(parent);
        }

        private static T findVisualChild<T>(DependencyObject parent, string childName) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child != null && child is T && child is FrameworkElement && ((FrameworkElement)child).Name == childName)
                {
                    return (T)child;
                }
                else
                {
                    T childOfChild = findVisualChild<T>(child, childName);
                    if (childOfChild != null)
                    {
                        return childOfChild;
                    }
                }
            }
            return null;
        }

        private void calculateFunction(double min, double max, string functionName)
        {
            List<double> xList = new List<double>();
            List<double> yList = new List<double>();

            for (double x = min; x <= max; x += 0.1)
            {
                xList.Add(x);

                switch (functionName)
                {
                    case "logaritemska":
                        double logMin = Math.Log10(min);
                        double logMax = Math.Log10(max);
                        double logValue = Math.Log10(x);
                        var result = (logValue - logMin) / (logMax - logMin);
                        yList.Add(result);
                        break;
                    case "padajoča logaritemska":
                        double logMin1 = Math.Log10(min);
                        double logMax1 = Math.Log10(max);
                        double logValue1 = Math.Log10(x);
                        double normalizedValue2 = (logValue1 - logMin1) / (logMax1 - logMin1);
                        var result1 = 1 - normalizedValue2;
                        yList.Add(result1);
                        break;
                    case "eksponentna":
                        double range = max - min;
                        double normalizedValue = (x - min) / range;
                        var value2 = normalizedValue * normalizedValue;
                        yList.Add(value2);
                        break;
                    case "padajoča eksponentna":
                        double range1 = max - min;
                        double normalizedValue1 = (x - min) / range1;
                        var value3 = 1 - normalizedValue1 * normalizedValue1;
                        yList.Add(value3);
                        break;
                    case "linearna":
                        double slope = 1 / (max - min);
                        double intercept = -min * slope;
                        var value4 = slope * x + intercept;
                        yList.Add(value4);
                        break;
                }
            }

            drawGraph(xList, yList, functionName);
        }

        private void drawGraph(List<double> xList, List<double> yList, string name)
        {
            PlotModel plotModel = new();
            plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Minimum = 0, Maximum = 1 });
            LineSeries lineSeries = new();

            for (int i = 0; i < xList.Count; i++)
            {
                lineSeries.Points.Add(new DataPoint(xList[i], yList[i]));
            }

            plotModel.Series.Add(lineSeries);

            plotModel.Title = name;
            plotView.Model = plotModel;
            plotView.DataContext = plotModel;
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)sender;
            double val = 0;
            double.TryParse(textBox.Text, out val);
            var child = (KeyValuePair<string, double>)textBox.DataContext;
            var parent = findVisualParent<ListBox>(textBox);
            Node x = (Node)parent.DataContext;

            if (val != 0)
            {
                foreach (var c in x.Children)
                {
                    if (child.Key == c.Key)
                    {
                        x.Children[child.Key] = val;
                    }
                }
            }
        }

        private void stopParameterEditing(object sender, RoutedEventArgs e)
        {
            bool valid = true;
            foreach (var x in EndNodes)
            {
                if (x.SelectedFunction == "")
                    valid = false;
            }
            foreach (var x in Nodes)
            {
                double sum = 0;
                foreach (var y in x.Children)
                {
                    sum += y.Value;
                    if (y.Value == 0)
                        valid = false;
                }
                if (sum != 1)
                    valid = false;
            }

            if (!valid) MessageBox.Show("Parametri niso vredu ocenjeni.");
            else
            {
                step2.Visibility = Visibility.Collapsed;
                step3.Visibility = Visibility.Visible;
            }
        }

        private void addAlternative(object sender, RoutedEventArgs e)
        {
            AlternativeWindow window = new(EndNodes);
            Alternative a = new();
            window.addAlternative += (_a) =>
            {
                a.EndValues = _a.EndValues;
                a.EndUtility = _a.EndUtility;
                a.Name = _a.Name;
            };
            window.ShowDialog();

            if (window.DialogResult == true)
            {
                Dictionary<TreeNode, double> final = new();

                Node node = Nodes.First();
                final = CalculateUtility(node, a, final);

                double sestevek = 0;
                foreach (var childNode in node.Children)
                {
                    var najden = final.Where(x => x.Key.Name == childNode.Key).First();
                    sestevek += najden.Value;
                }
                TreeNode tn = new(node.Name);
                final.Add(tn, sestevek);

                a.Utility = sestevek;
                a.FinalUtility = final;
                Alternatives.Add(a);
                AddColumnsToAlternativesTable();
                AddColumnsToAlternativesValuesTable();
                UpdateGraph();
            }
        }

        private List<string> addAllTreeNodesInList(List<string> list, TreeNode first)
        {
            list.Add(first.Name);
            foreach (TreeNode node in first.Children)
                list = addAllTreeNodesInList(list, node);
            return list;
        }

        private List<string> addAllEndNodesInList(List<string> list, TreeNode first)
        {
            foreach (TreeNode node in first.Children)
            {
                if (node.Children.Count() == 0)
                    list.Add(node.Name);
                else
                    list = addAllEndNodesInList(list, node);
            }
            return list;
        }

        public void UpdateGraph()
        {
            PlotModel plotModel = new();

            BarSeries barSeries = new();
            barSeries.Title = "ocena";
            for (int i = 0; i < Alternatives.Count; i++)
            {
                barSeries.Items.Add(new BarItem(Alternatives[i].Utility));
            }

            var xAxis = new CategoryAxis { Position = AxisPosition.Left };
            for (int i = 0; i < Alternatives.Count; i++)
            {
                xAxis.Labels.Add(Alternatives[i].Name);
            }
            plotModel.Axes.Add(xAxis);

            var yAxis = new LinearAxis { Position = AxisPosition.Bottom, Minimum = 0, Maximum = 1 };
            plotModel.Axes.Add(yAxis);

            plotModel.Title = "Končne ocene alternativ";
            plotModel.Series.Add(barSeries);
            finalUtility.Model = plotModel;
        }

        public void AddColumnsToAlternativesValuesTable()
        {
            alternativesValuesTable.Columns.Clear();

            List<AlternativesTableDisplay> alternativesTableDisplays = new();

            List<string> list = new();
            list = addAllEndNodesInList(list, TreeNodes.First());
            alternativesTableDisplays.Add(new("parametri", list));
            foreach (Alternative alternative in Alternatives)
            {
                List<string> utility = new();
                foreach (var l in list)
                {
                    string value = alternative.EndValues.Where(x => x.Key.Name == l).First().Value.ToString();
                    utility.Add(value);
                }
                alternativesTableDisplays.Add(new(alternative.Name, utility));
            }

            foreach (var d in alternativesTableDisplays)
            {
                DataGridTextColumn col = new();
                col.Width = 100;
                col.Header = d.Header;

                Binding binding = new Binding();
                binding.Converter = new ListToStringConverter();
                binding.Source = d.Values;
                col.Binding = binding;

                alternativesValuesTable.Columns.Add(col);
            }
        }

        private void AddColumnsToAlternativesTable()
        {
            alternativesTable.Columns.Clear();

            List<AlternativesTableDisplay> alternativesTableDisplays = new();

            List<string> list = new();
            list = addAllTreeNodesInList(list, TreeNodes.First());
            alternativesTableDisplays.Add(new("parametri", list));
            foreach (var a in Alternatives)
            {
                List<string> utility = new();
                foreach (var l in list)
                {
                    string value = a.FinalUtility.Where(x => x.Key.Name == l).First().Value.ToString();
                    utility.Add(value);
                }
                alternativesTableDisplays.Add(new(a.Name, utility));
            }

            foreach (var d in alternativesTableDisplays)
            {
                DataGridTextColumn col = new();
                col.Width = 100;
                col.Header = d.Header;

                Binding binding = new Binding();
                binding.Converter = new ListToStringConverter();
                binding.Source = d.Values;
                col.Binding = binding;

                alternativesTable.Columns.Add(col);
            }
        }

        private Dictionary<TreeNode, double> CalculateUtility(Node node, Alternative a, Dictionary<TreeNode, double> final)
        {
            foreach (var child in node.Children)
            {
                if (EndNodes.Any(x => x.Name == child.Key))
                {
                    var found = a.EndUtility.Where(x => x.Key.Name == child.Key).First();
                    double koristnost = found.Value * child.Value;
                    TreeNode n = new(found.Key.Name);
                    final.Add(n, koristnost);
                }
                else
                {
                    Node midleNode = Nodes.Where(x => x.Name == child.Key).First();
                    final = CalculateUtility(midleNode, a, final);
                    TreeNode treeNode = new(midleNode.Name);

                    double sestevek = 0;
                    if (final.Where(x => x.Key.Name == treeNode.Name).Count() == 0)
                    {
                        foreach (var childNode in midleNode.Children)
                        {
                            var found = final.Where(x => x.Key.Name == childNode.Key).First();
                            sestevek += found.Value;
                        }
                        final.Add(treeNode, sestevek * child.Value);
                    }
                }
            }
            return final;
        }
    }

    public class ListToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            List<string> list = value as List<string>;
            if (list != null)
            {
                return string.Join('\n', list);
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAUT
{
    internal class Node
    {
        public string Name { get; set; }
        public Dictionary<string, double> Children { get; set; }

        public Node(string name, Dictionary<string, double> children)
        {
            Name = name;
            Children = children;
        }
    }
}

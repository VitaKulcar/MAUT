using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAUT
{
    internal class ProjectSaving
    {
        public List<TreeNode> TreeNodes { get; set; }
        public List<EndNode> EndNodes { get; set; }
        public List<Node> Nodes { get; set; }
        public List<Alternative> Alternatives { get; set; }

        public ProjectSaving(List<TreeNode> treeNodes, List<EndNode> endNodes, List<Node> nodes, List<Alternative> alternatives)
        {
            TreeNodes = treeNodes;
            EndNodes = endNodes;
            Nodes = nodes;
            Alternatives = alternatives;
        }

        public ProjectSaving()
        {
        }
    }
}

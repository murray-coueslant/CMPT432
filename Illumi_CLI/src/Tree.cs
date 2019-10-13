using System.Reflection.Metadata.Ecma335;
namespace Illumi_CLI {
    internal class Tree {
        public Tree (TreeNode root = null) {
            Root = root;
            currentNode = Root;
        }
        public TreeNode Root { get; set; }
        public TreeNode currentNode { get; set; }
        public void SetRoot (TreeNode newRoot) {
            Root = newRoot;
            currentNode = Root;
        }
        public void AddLeafNode (TreeNode node) {
            if (Root is null) {
                SetRoot (node);
            } else {
                currentNode.AddChild (node);
            }

        }
        public void AddBranchNode (TreeNode node) {
            if (Root is null) {
                SetRoot (node);
            } else {
                currentNode.AddChild (node);
                UpdateCurrentNode ();
            }
        }
        public void UpdateCurrentNode () {
            currentNode = currentNode.MostRecentChild;
        }
        public void Ascend () {
            if (currentNode != Root) {
                System.Console.WriteLine ($"Ascending from node [{currentNode.Type}].");
                currentNode = currentNode.Parent;
            } else {
                System.Console.WriteLine ("Reached root!");
            }
            return;
        }

        public static void PrintTree (TreeNode root, string indent, bool lastChild) {
            if (root.NodeToken != null) {
                System.Console.WriteLine (indent + "+- " + root.NodeToken.Text);
            } else {
                System.Console.WriteLine (indent + "+- " + root.Type);
            }

            indent += lastChild ? "   " : "|  ";

            for (int i = 0; i < root.Children.Count; i++) {
                PrintTree (root.Children[i], indent, i == root.Children.Count - 1);
            }
        }
    }
}
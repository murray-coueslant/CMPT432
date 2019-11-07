using System.Security.Cryptography.X509Certificates;
namespace Illumi_CLI {
    class AbstractSyntaxTree {
        public ASTNode Root { get; set; }
        public ASTNode CurrentNode { get; set; }
        public AbstractSyntaxTree (ASTNode root = null) {
            Root = root;
            CurrentNode = Root;
        }
        public void AddBranchNode (string nodeText) {
            ASTNode newNode = new ASTNode (nodeText);
            if (Root is null) {
                Root = newNode;
                CurrentNode = Root;
            } else {
                CurrentNode.AddDescendant (newNode);
                CurrentNode = newNode;
            }
        }
        public void AddLeafNode (string nodeText) {
            ASTNode newNode = new ASTNode (nodeText);
            if (Root is null) {
                Root = newNode;
                CurrentNode = Root;
            } else {
                CurrentNode.AddDescendant (newNode);
            }
        }
        public void Ascend (Session session) {
            if (CurrentNode.Parent != null) {
                System.Console.WriteLine ($"[ Info ] - [ Tree ] -> Ascending from node [ {CurrentNode.Text} ] to [ {CurrentNode.Parent.Text} ].");
                CurrentNode = CurrentNode.Parent;
            }
        }
        public static void PrintTree (ASTNode root, string indent = "", bool lastChild = true) {
            // ├──
            // └──
            // │
            string marker = lastChild ? "└── " : "├── ";

            System.Console.Write (indent);
            System.Console.Write (marker);
            System.Console.Write ($"{root.Text}");
            System.Console.WriteLine ();

            indent += lastChild ? "    " : "│   ";

            for (int i = 0; i < root.Descendants.Count; i++) {
                PrintTree (root.Descendants[i], indent, i == root.Descendants.Count - 1);
            }
        }
    }
}
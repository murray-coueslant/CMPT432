using System.Security.Cryptography.X509Certificates;
namespace Illumi_CLI {
    class AbstractSyntaxTree {
        public ASTNode Root { get; set; }
        public ASTNode CurrentNode { get; set; }
        public Session CurrentSession { get; set; }
        public AbstractSyntaxTree (Session currentSession, ASTNode root = null) {
            Root = root;
            CurrentNode = Root;
            CurrentSession = currentSession;
        }
        public void AddBranchNode (ASTNode newNode) {
            if (newNode != null) {
                if (Root is null) {
                    Root = newNode;
                    CurrentNode = Root;
                } else {
                    CurrentNode.AddDescendant (newNode);
                    CurrentNode = newNode;
                }
                CurrentSession.Diagnostics.Semantic_ReportAddingASTNode (newNode);
            }
        }
        public void AddBranchNode (Token token) {
            ASTNode newNode = new ASTNode (token);
            if (Root is null) {
                Root = newNode;
                CurrentNode = Root;
            } else {
                CurrentNode.AddDescendant (newNode);
                CurrentNode = newNode;
            }
            CurrentSession.Diagnostics.Semantic_ReportAddingASTNode (newNode);
        }
        public void AddLeafNode (ASTNode newNode) {
            if (newNode != null) {
                if (Root is null) {
                    Root = newNode;
                    CurrentNode = Root;
                } else {
                    CurrentNode.AddDescendant (newNode);
                }
                CurrentSession.Diagnostics.Semantic_ReportAddingASTNode (newNode);
            }
        }
        public void AddLeafNode (Token token) {
            ASTNode newNode = new ASTNode (token);
            if (Root is null) {
                Root = newNode;
                CurrentNode = Root;
            } else {
                CurrentNode.AddDescendant (newNode);
            }
            CurrentSession.Diagnostics.Semantic_ReportAddingASTNode (newNode);
        }
        public void Ascend (Session session) {
            if (CurrentNode.Parent != null) {
                session.Diagnostics.Tree_ReportAscendingLevel (CurrentNode);
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
            System.Console.Write ($"{root.Token.Text}");
            System.Console.WriteLine ();

            indent += lastChild ? "    " : "│   ";

            for (int i = 0; i < root.Descendants.Count; i++) {
                PrintTree (root.Descendants[i], indent, i == root.Descendants.Count - 1);
            }
        }
    }
}
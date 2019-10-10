using System.Collections.Generic;

namespace Illumi_CLI {
    class CSTNode : TreeNode {
        public Token Token { get; set; }
        public string Type { get; set; }

        public CSTNode Parent { get; set; }

        public List<CSTNode> Children { get; set; }

        public CSTNode (TreeNode parent = null, List<TreeNode> children = null, bool leaf = false, Token token = null) : base (leaf) {
            Token = token;
            Type = Token.Kind.ToString ();
            Parent = parent;
            if (children is null) {
                Children = new List<TreeNode> ();
            } else {
                Children = children;
            }
            Leaf = leaf;
        }

        public CSTNode (string type = null) : base () {
            Type = type;
            Token = null;
        }

        public void SetParent (CSTNode newParent) {
            Parent = newParent;
        }
    }
}
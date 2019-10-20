using System;
using System.Collections.Generic;

namespace Illumi_CLI {
    class TreeNode {
        public TreeNode (TreeNode parent = null, List<TreeNode> children = null, bool leaf = false, Token token = null, string type = null) {
            Parent = parent;
            if (children is null) {
                Children = new List<TreeNode> ();
            } else {
                Children = children;
            }
            Leaf = leaf;
            NodeToken = token;
            Type = type;
        }
        public void AddChild (TreeNode newChild) {
            Children.Add (newChild);
            newChild.SetParent (this);
            MostRecentChild = newChild;
        }
        public void SetParent (TreeNode newParent) {
            Parent = newParent;
        }
        public TreeNode Parent { get; set; }
        public List<TreeNode> Children { get; private set; }
        public TreeNode MostRecentChild { get; private set; }
        public bool Leaf { get; set; }
        public Token NodeToken { get; set; }
        public string Type { get; set; }
    }
}
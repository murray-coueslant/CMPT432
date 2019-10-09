using System.Collections.Generic;

namespace Illumi_CLI {
    class TreeNode {
        public TreeNode (TreeNode parent = null, List<TreeNode> children = null, boolean leaf = false) {
            Parent = parent;
            Children = children;
            Leaf = leaf;
        }

        public void AddChild (TreeNode newChild) {
            Children.Add (newChild);
            newChild.Parent = this;
            mostRecentChild = newChild;
        }

        // public void RemoveChild() { }

        public void SetParent (TreeNode newParent) {
            Parent = newParent;
        }

        public TreeNode Parent { get; private set; }
        public List<TreeNode> Children { get; private set; }
        public TreeNode mostRecentChild { get; private set; }
        public boolean Leaf { get; set; }
    }
}
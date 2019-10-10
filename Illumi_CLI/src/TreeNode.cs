using System.Collections.Generic;

namespace Illumi_CLI {
    class TreeNode {
        public TreeNode (TreeNode parent = null, List<TreeNode> children = null, bool leaf = false) {

        }

        public void AddChild (TreeNode newChild) {
            Children.Add (newChild);
            newChild.SetParent (this);
            mostRecentChild = newChild;
        }

        // public void RemoveChild() { }

        public void SetParent (TreeNode newParent) {
            Parent = newParent;
        }

        // public TreeNode Parent { get; set; }
        // public List<TreeNode> Children { get; private set; }
        public TreeNode mostRecentChild { get; private set; }
        public bool Leaf { get; set; }
    }
}
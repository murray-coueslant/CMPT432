using System.Collections.Generic;

namespace Illumi_CLI
{
    class TreeNode
    {
        public TreeNode(TreeNode parent = null, List<TreeNode> children = null)
        {
            Parent = parent;
            Children = children;
        }

        public void AddChild(TreeNode newChild) { }

        public void RemoveChild() { }

        public void SetParent(TreeNode newParent) { }

        public TreeNode Parent { get; private set; }
        public List<TreeNode> Children { get; private set; }
    }
}
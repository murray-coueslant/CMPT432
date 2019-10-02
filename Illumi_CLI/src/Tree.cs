namespace Illumi_CLI
{
    internal class Tree
    {
        public Tree(TreeNode root = null)
        {
            Root = root;
        }

        public TreeNode Root { get; }

        public void SetRoot(TreeNode newRoot) { }

        public void DisplayTree() { }
    }
}
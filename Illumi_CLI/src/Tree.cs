namespace Illumi_CLI {
    internal class Tree {
        public Tree (TreeNode root = null) {
            Root = root;
        }

        public TreeNode Root { get; set; }

        public void SetRoot (TreeNode newRoot) {
            Root = newRoot;
        }

        public void DisplayTree () {

        }
    }
}
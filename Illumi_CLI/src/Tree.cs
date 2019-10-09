namespace Illumi_CLI {
    internal class Tree {
        public Tree (TreeNode root = null) {
            Root = root;
        }

        public TreeNode Root { get; set; }

        public void SetRoot (TreeNode newRoot) {
            Root = newRoot;
        }

        // public void DisplayTree () {
        //     foreach (TreeNode child in Root.Children) {
        //         DisplayChildren (child);
        //     }
        // }

        // public void DisplayChildren (TreeNode node) {
        //     foreach (TreeNode child in node.Children) {
        //         DisplayChildren (child);
        //         System.Console.WriteLine (child.ToString ());
        //     }
        // }
    }
}
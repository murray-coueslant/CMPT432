namespace Illumi_CLI {
    class CSTNode : TreeNode {
        public Token Token { get; set; }

        public CSTNode (Token token) : base () {
            Token = token;
        }
    }
}
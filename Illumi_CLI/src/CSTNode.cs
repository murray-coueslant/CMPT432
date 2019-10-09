namespace Illumi_CLI {
    class CSTNode : TreeNode {
        public Token Token { get; set; }
        public string Type { get; set; }

        public CSTNode (Token token = null) : base () {
            Token = token;
        }

        public CSTNode (string type = null) : base () {
            Type = type;
        }
    }
}
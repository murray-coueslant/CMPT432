using System;
using System.Collections.Generic;
using System.Linq;
namespace Illumi_CLI {
    class ASTNode {
        public List<ASTNode> Descendants { get; set; }
        public ASTNode Parent { get; set; }
        public Token Token { get; set; }
        public bool Visited { get; set; }
        public ASTNode (Token token = null, List<ASTNode> descendants = null, ASTNode parent = null) {
            Token = token;
            if (descendants == null) {
                Descendants = new List<ASTNode> ();
            } else {
                Descendants = descendants;
            }
            Parent = Parent;
            Visited = false;
        }
        public void AddDescendant (ASTNode node) {
            Descendants.Add (node);
            node.Parent = this;
        }
    }
}
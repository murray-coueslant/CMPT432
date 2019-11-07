using System;
using System.Collections.Generic;
using System.Linq;
namespace Illumi_CLI {
    class ASTNode {
        public string Text { get; set; }
        public List<ASTNode> Descendants { get; set; }
        public ASTNode Parent { get; set; }
        public ASTNode (string text = "", List<ASTNode> descendants = null, ASTNode parent = null) {
            Text = text;
            if (descendants == null) {
                Descendants = new List<ASTNode> ();
            } else {
                Descendants = descendants;
            }
            Parent = Parent;
        }
        public void AddDescendant (ASTNode node) {
            Descendants.Add (node);
            node.Parent = this;
        }
    }
}
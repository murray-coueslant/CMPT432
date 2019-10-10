using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Text;

namespace Illumi_CLI {
    class ConcreteSyntaxTree : Tree {
        public ConcreteSyntaxTree (CSTNode root = null) : base (root) { }

        public void DisplayCST () {
            foreach (CSTNode child in Root.Children) {
                DisplayChildren (child);
                if (!child.Leaf) {
                    System.Console.WriteLine (child.Type);
                } else {
                    System.Console.WriteLine (child.Token.Kind);
                }
            }
        }

        public void DisplayChildren (CSTNode node) {
            foreach (CSTNode child in node.GetChildren ()) {
                DisplayChildren (child);
                if (!child.Leaf) {
                    System.Console.WriteLine ($"{child.Type} [{child.Parent.Type}]");
                } else {
                    System.Console.WriteLine ($"{child.Token.Kind} [{child.Parent.Type}]");
                }
            }
        }
        public CSTNode currentNode { get; set; }
    }
}
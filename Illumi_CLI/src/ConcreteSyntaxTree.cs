using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Text;

namespace Illumi_CLI {
    class ConcreteSyntaxTree : Tree {
        public ConcreteSyntaxTree (CSTNode root = null) : base (root) { }

        public void DisplayCST () {
            PrintTree (Root, "", false);
        }

        // public void DisplayChildren (TreeNode node) {
        //     foreach (TreeNode child in node.Children) {
        //         DisplayChildren (child);
        //         if (!child.Leaf) {
        //             System.Console.WriteLine ($"{child.Type} [{child.Parent.Type}]");
        //         } else {
        //             System.Console.WriteLine ($"{child.Type} [{child.Parent.Type}]");
        //         }
        //     }
        // }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Text;

namespace Illumi_CLI {
    class ConcreteSyntaxTree : Tree {
        public ConcreteSyntaxTree (CSTNode root = null) : base (root) { }

        public void AddLeafNode (CSTNode newNode) {
            if (Root is null) {
                SetRoot (newNode);
                currentNode = (CSTNode) Root;
            } else {
                currentNode.AddChild (newNode);
            }
        }

        public void AddBranchNode (CSTNode newNode) {
            if (Root is null) {
                SetRoot (newNode);
                currentNode = (CSTNode) Root;
            } else {
                currentNode.AddChild (newNode);
                UpdateCurrentNode ();
            }

        }

        public void UpdateCurrentNode () {
            currentNode = (CSTNode) currentNode.mostRecentChild;
            return;
        }

        public void Ascend () {
            if (currentNode != Root) {
                System.Console.WriteLine ($"Ascending from node [{currentNode.Type}].");
                currentNode = (CSTNode) currentNode.Parent;
            } else {
                System.Console.WriteLine ("Reached root!");
            }
            return;
        }

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
            foreach (CSTNode child in node.Children) {
                DisplayChildren (child);
                if (!child.Leaf) {
                    System.Console.WriteLine ($"{child.Type} [{child.Parent}]");
                } else {
                    System.Console.WriteLine ($"{child.Token.Kind} [{child.Parent}]");
                }
            }
        }
        public CSTNode currentNode { get; set; }
    }
}
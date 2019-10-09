using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Microsoft.CodeAnalysis.Text;

namespace Illumi_CLI {
    class ConcreteSyntaxTree : Tree {
        public ConcreteSyntaxTree (CSTNode root = null) : base (root) { }

        public void AddLeafNode (TreeNode newNode) {
            if (Root is null) {
                SetRoot (newNode);
                currentNode = Root;
            } else {
                currentNode.AddChild (newNode);
            }
        }

        public void AddBranchNode (TreeNode newNode) {
            if (Root is null) {
                SetRoot (newNode);
                currentNode = Root;
            } else {
                currentNode.AddChild (newNode);
                UpdateCurrentNode ();
            }

        }

        public void UpdateCurrentNode () {
            currentNode = currentNode.mostRecentChild;
            return;
        }

        public void Ascend () {
            System.Console.WriteLine ($"Ascending from node [{currentNode.Token.Kind}] to [{currentNode.Parent.Token.Kind}].");
            currentNode = currentNode.Parent;
        }

        public CSTNode currentNode { get; set; }
    }
}
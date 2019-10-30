using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Illumi_CLI {
    internal class Tree {
        public Tree (TreeNode root = null) {
            Root = root;
            currentNode = Root;
        }
        public TreeNode Root { get; set; }
        public TreeNode currentNode { get; set; }
        public void SetRoot (TreeNode newRoot) {
            Root = newRoot;
            currentNode = Root;
        }
        public void AddLeafNode (TreeNode node) {
            if (Root is null) {
                SetRoot (node);
            } else {
                currentNode.AddChild (node);
            }

        }
        public void AddBranchNode (TreeNode node) {
            if (Root is null) {
                SetRoot (node);
            } else {
                currentNode.AddChild (node);
                UpdateCurrentNode ();
            }
        }
        public void UpdateCurrentNode () {
            currentNode = currentNode.MostRecentChild;
        }
        public void Ascend (Session session) {
            if (currentNode != Root) {
                if (session.debugMode) {
                    System.Console.WriteLine ($"[Debug] - [Tree] -> Ascending from node [ {currentNode.Type} ] to [ {currentNode.Parent.Type} ].");
                }
                currentNode = currentNode.Parent;
            } else {
                System.Console.WriteLine ("[Info] - [Parser] -> Reached root!");
            }
            return;
        }
        public static void PrintTree (TreeNode root, string indent, bool lastChild = true) {
            // ├──
            // └──
            // │
            string marker = lastChild ? "└── " : "├── ";

            System.Console.Write (indent);
            System.Console.Write (marker);
            System.Console.Write ($"{root.Type} {(root.NodeToken is null ? "" : $"[ {root.NodeToken.Text} ]")}");
            System.Console.WriteLine ();

            indent += lastChild ? "    " : "│   ";

            for (int i = 0; i < root.Children.Count; i++) {
                PrintTree (root.Children[i], indent, i == root.Children.Count - 1);
            }
        }

        public void Discard () {
            Root = null;
            return;
        }
    }
}
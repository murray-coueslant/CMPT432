using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Illumi_CLI {
    class SemanticAnalyser {
        public Parser Parser { get; }
        public Session CurrentSession { get; }
        public DiagnosticCollection Diagnostics { get; }
        public Tree AbstractSyntaxTree { get; set; }
        public SymbolTable Symbols { get; set; }
        public SemanticAnalyser (Parser parser, Session currentSession, DiagnosticCollection diagnostics) {
            Parser = parser;
            CurrentSession = currentSession;
            Diagnostics = diagnostics;
            AbstractSyntaxTree = new AbstractSyntaxTree ();
            Symbols = new SymbolTable (Diagnostics);
        }
        public void Analyse () {
            if (Parser.Failed) {
                Diagnostics.Semantic_ParserGaveNoTree ();
            } else {
                TraverseParseTreeAndBuildAST ();
            }
        }
        public void Traverse (TreeNode root) {
            CheckNode (root);

            for (int i = 0; i < root.Children.Count; i++) {
                Traverse (root.Children[i]);
            }
        }
        public void CheckNode (TreeNode node) {
            if (node.Type == TokenKind.IdentifierToken.ToString ()) {
                System.Console.WriteLine (node.NodeToken.Text);
                Symbols.AddSymbol (node.NodeToken.Text);
            }
        }
        public void TraverseParseTreeAndBuildAST () {
            Traverse (Parser.Tree.Root);
        }

    }
}
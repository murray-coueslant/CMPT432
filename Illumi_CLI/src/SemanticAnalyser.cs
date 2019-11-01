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
                TraverseParseTreeAndBuildASTAndSymbolTables ();
                if (Diagnostics.ErrorCount == 0) {
                    Diagnostics.Semantic_ReportDisplayingSymbolTables ();
                    Symbols.DisplaySymbolTables (Symbols.RootScope);
                }
            }
        }
        public void Traverse (TreeNode root, Action<TreeNode> checkFunction) {
            checkFunction (root);

            for (int i = 0; i < root.Children.Count; i++) {
                Traverse (root.Children[i], checkFunction);
            }
        }
        public void CheckSymbols (TreeNode node) {
            if (node.Type == "Block") {
                Symbols.NewScope ();
                Symbols.UpdateCurrentScope ();
            }
            if (node.Type == "RightBraceToken") {
                Symbols.AscendScope ();
            }
            if (Symbols.CurrentScope != null) {
                if (node.Type == "VariableDeclaration") {
                    Symbols.AddSymbol (node.Children[1].Children[0], node.Children[0].Children[0].NodeToken.Text);
                }
                if (node.Type == "AssignmentStatement") {
                    if (!FindSymbol (node.Children[0].Children[0].NodeToken.Text, Symbols.RootScope)) {
                        Diagnostics.Semantic_ReportUndeclaredIdentifier (node.Children[0].Children[0].NodeToken);
                    }
                }
            }
        }
        public void TraverseParseTreeAndBuildASTAndSymbolTables () {
            Traverse (Parser.Tree.Root, CheckSymbols);
        }

        public bool FindSymbol (string symbol, Scope rootScope) {
            if (rootScope.Symbols.ContainsKey (symbol)) {
                return true;
            } else {
                for (int i = 0; i < rootScope.DescendantScopes.Count; i++) {
                    FindSymbol (symbol, rootScope.DescendantScopes[i]);
                }
            }
            return false;
        }
    }
}
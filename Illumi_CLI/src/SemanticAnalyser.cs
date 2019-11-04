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
                    Console.WriteLine ();
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
        public bool CheckSymbolScope (TreeNode root) {
            Traverse (root, CheckScope);
            if (Diagnostics.ErrorCount == 0) {
                return true;
            }
            return false;
        }
        public bool CheckSymbolType (TreeNode root) {
            Traverse (root, CheckType);
            if (Diagnostics.ErrorCount == 0) {
                return true;
            }
            return false;
        }
        public void CheckScope (TreeNode node) {
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
                if (node.Type == "Identifier") {
                    Diagnostics.Semantic_ReportSymbolLookup (node.Children[0].NodeToken.Text);
                    bool success = SymbolExists (node.Children[0].NodeToken.Text, Symbols.CurrentScope);
                    if (!success) {
                        Diagnostics.Semantic_ReportUndeclaredIdentifier (node.Children[0].NodeToken, Symbols.CurrentScope.Level);
                    }
                }
            }
        }
        public void CheckType (TreeNode node) {
            switch (node.Type) {
                case "AssignmentStatement":
                    string leftIdentifierType = GetSymbolType (node.Children[0].Children[0].NodeToken.Text, Symbols.CurrentScope);
                    string rightExpressionType = GetExpressionType (node.Children[2]);
                    System.Console.WriteLine ($"Assignment type checked: {leftIdentifierType == rightExpressionType}");
                    break;
                default:
                    break;
            }

        }
        public string GetExpressionType (TreeNode expressionNode) {
            switch (expressionNode.Children[0].Type) {
                case "Identifier":
                    return GetSymbolType (expressionNode.Children[0].Children[0].NodeToken.Text, Symbols.CurrentScope);
                case "IntegerExpression":
                    return GetExpressionType (expressionNode.Children[0]);
                default:
                    return null;
            }
        }
        public void TraverseParseTreeAndBuildASTAndSymbolTables () {
            //TraverseAndBuildAST (Parser.Tree.Root, CheckSymbols);
            BuildAST (Parser);
            Diagnostics.Semantic_ReportCheckingScope ();
            if (CheckSymbolScope (Parser.Tree.Root)) {
                Diagnostics.Semantic_ReportCheckingType ();
                CheckSymbolType (Parser.Tree.Root);
            }
        }
        public void BuildAST (Parser parser) {
            Token currentToken = parser.TokenStream.First ();

            System.Console.WriteLine (currentToken.Text);
        }
        public bool SymbolExists (string symbol, Scope searchScope) {
            if (searchScope.Symbols.ContainsKey (symbol)) {
                Diagnostics.Semantic_ReportFoundSymbol (symbol, searchScope);
                return true;
            } else {
                if (searchScope.ParentScope != null) {
                    Diagnostics.Semantic_ReportSymbolNotFound (symbol, searchScope);
                    return SymbolExists (symbol, searchScope.ParentScope);
                }
                return false;
            }
        }
        public string GetSymbolType (string symbol, Scope searchScope) {
            if (searchScope.Symbols.ContainsKey (symbol)) {
                Diagnostics.Semantic_ReportFoundSymbol (symbol, searchScope);
                return searchScope.Symbols[symbol].ToString ();
            } else {
                if (searchScope.ParentScope != null) {
                    Diagnostics.Semantic_ReportSymbolNotFound (symbol, searchScope);
                    return GetSymbolType (symbol, searchScope.ParentScope);
                }
                return null;
            }
        }
    }
}
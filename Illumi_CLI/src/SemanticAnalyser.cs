using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;

namespace Illumi_CLI {
    class SemanticAnalyser {
        private const string String = "string";
        private const string Boolean = "boolean";
        private const string Integer = "int";

        public Parser Parser { get; set; }
        public Tree ConcreteSyntaxTree { get; }
        public List<Token> TokenStream { get; set; }
        public int TokenCounter { get; set; }
        public Token CurrentToken { get => TokenStream[TokenCounter]; }
        public Session CurrentSession { get; set; }
        public DiagnosticCollection Diagnostics { get; set; }
        public AbstractSyntaxTree AbstractSyntaxTree { get; set; }
        public SymbolTable Symbols { get; set; }
        public SemanticAnalyser (Parser parser, Session currentSession, DiagnosticCollection diagnostics) {
            Parser = parser;
            ConcreteSyntaxTree = parser.Tree;
            TokenStream = parser.TokenStream;
            CurrentSession = currentSession;
            Diagnostics = diagnostics;
            TokenCounter = 0;
            Symbols = new SymbolTable (Diagnostics);
        }
        public void Analyse () {
            if (Parser.Failed) {
                Diagnostics.Semantic_ParserGaveNoTree ();
            } else {
                AbstractSyntaxTree = BuildAST ();
                if (Diagnostics.ErrorCount == 0) {
                    Diagnostics.Semantic_ReportDisplayingAST ();
                    AbstractSyntaxTree.PrintTree (AbstractSyntaxTree.Root);
                    ScopeAndTypeCheck ();
                }
            }
        }
        public AbstractSyntaxTree BuildAST (AbstractSyntaxTree inputTree = null) {
            AbstractSyntaxTree tree;
            if (inputTree == null) {
                tree = new AbstractSyntaxTree (CurrentSession);
            } else {
                tree = inputTree;
            }
            while (TokenCounter < TokenStream.Count && CurrentToken.Kind != TokenKind.EndOfProgramToken) {
                HandleStatement (tree);
                NextToken ();
            }
            return tree;
        }
        public void ScopeAndTypeCheck () {
            Traverse (AbstractSyntaxTree.Root, CheckScope);
            if (Diagnostics.ErrorCount == 0) {
                Traverse (AbstractSyntaxTree.Root, CheckType);
            } else {
                Diagnostics.Semantic_ReportScopeError ();
            }

            if (Diagnostics.ErrorCount == 0) {
                Diagnostics.Semantic_ReportDisplayingSymbolTables ();
                Console.WriteLine ();
                Symbols.DisplaySymbolTables (Symbols.RootScope);
            } else {
                return;
            }
        }
        public void HandleBlock (AbstractSyntaxTree tree) {
            tree.AddBranchNode (new Token (TokenKind.Block, "Block", 0, 0));
            NextToken ();
            HandleStatement (tree);
            if (CurrentToken.Kind == TokenKind.RightBraceToken) {
                tree.Ascend (CurrentSession);
                return;
            }
        }
        public void HandleStatement (AbstractSyntaxTree tree) {
            switch (CurrentToken.Kind) {
                case TokenKind.AssignmentToken:
                    HandleAssignmentStatement (tree);
                    break;
                case TokenKind.Type_IntegerToken:
                case TokenKind.Type_StringToken:
                case TokenKind.Type_BooleanToken:
                    HandleVariableDeclaration (tree);
                    break;
                case TokenKind.PrintToken:
                    HandlePrintStatement (tree);
                    break;
                case TokenKind.IfToken:
                    HandleIfStatement (tree);
                    break;
                case TokenKind.WhileToken:
                    HandleWhileStatement (tree);
                    break;
                case TokenKind.LeftBraceToken:
                    HandleBlock (tree);
                    break;
                case TokenKind.RightBraceToken:
                    tree.Ascend (CurrentSession);
                    return;
                case TokenKind.EndOfProgramToken:
                    return;
                default:
                    break;
            }
        }
        public void HandleVariableDeclaration (AbstractSyntaxTree tree) {
            tree.AddBranchNode (new Token (TokenKind.VarDecl, "VarDecl", 0, 0));
            switch (CurrentToken.Kind) {
                case TokenKind.Type_StringToken:
                case TokenKind.Type_IntegerToken:
                case TokenKind.Type_BooleanToken:
                    tree.AddLeafNode (CurrentToken);
                    tree.AddLeafNode (TokenStream[TokenCounter + 1]);
                    break;
                default:
                    Diagnostics.Semantic_ReportInvalidType (CurrentToken);
                    return;
            }
            tree.Ascend (CurrentSession);
        }
        public void HandleAssignmentStatement (AbstractSyntaxTree tree) {
            tree.AddBranchNode (CurrentToken);
            tree.AddLeafNode (TokenStream[TokenCounter - 1]);
            NextToken ();
            HandleExpression (tree);
            tree.Ascend (CurrentSession);
        }
        public void HandleExpression (AbstractSyntaxTree tree) {
            switch (CurrentToken.Kind) {
                case TokenKind.StringToken:
                    HandleStringExpr (tree);
                    break;
                case TokenKind.DigitToken:
                    HandleIntExpr (tree);
                    break;
                case TokenKind.TrueToken:
                case TokenKind.FalseToken:
                case TokenKind.LeftParenthesisToken:
                    HandleBooleanExpr (tree);
                    break;
                case TokenKind.IdentifierToken:
                    HandleIdentifier (tree);
                    break;
                default:
                    break;
            }
        }
        public void HandlePrintStatement (AbstractSyntaxTree tree) {
            tree.AddBranchNode (CurrentToken);
            NextToken ();
            NextToken ();
            HandleExpression (tree);
            NextToken ();
            tree.Ascend (CurrentSession);
        }
        public void HandleIfStatement (AbstractSyntaxTree tree) {
            tree.AddBranchNode (CurrentToken);
            NextToken ();
            HandleBooleanExpr (tree);
        }
        public void HandleWhileStatement (AbstractSyntaxTree tree) {
            tree.AddBranchNode (CurrentToken);
            NextToken ();
            HandleBooleanExpr (tree);
        }
        public void HandleStringExpr (AbstractSyntaxTree tree) {
            tree.AddLeafNode (CurrentToken);
        }
        public void HandleIntExpr (AbstractSyntaxTree tree) {
            if (TokenStream[TokenCounter + 1].Kind == TokenKind.AdditionToken) {
                tree.AddBranchNode (TokenStream[TokenCounter + 1]);
                tree.AddLeafNode (CurrentToken);
                NextToken ();
                NextToken ();
                HandleIntExpr (tree);
            } else {
                tree.AddBranchNode (CurrentToken);
            }
            tree.Ascend (CurrentSession);
        }
        public void HandleBooleanExpr (AbstractSyntaxTree tree) {
            switch (CurrentToken.Kind) {
                case TokenKind.TrueToken:
                case TokenKind.FalseToken:
                    tree.AddLeafNode (CurrentToken);
                    break;
                case TokenKind.IdentifierToken:
                    tree.AddLeafNode (CurrentToken);
                    NextToken ();
                    break;
                default:
                    HandleParenthesisedExpression (tree);
                    tree.Ascend (CurrentSession);
                    break;
            }
        }
        public void HandleParenthesisedExpression (AbstractSyntaxTree tree) {
            NextToken ();
            AbstractSyntaxTree leftExprTree = HandleExprTree ();
            NextToken ();
            tree.AddBranchNode (CurrentToken);
            tree.AddLeafNode (leftExprTree.Root);
            NextToken ();
            AbstractSyntaxTree rightExprTree = HandleExprTree ();
            tree.AddLeafNode (rightExprTree.Root);
        }
        public void HandleIdentifier (AbstractSyntaxTree tree) {
            tree.AddLeafNode (CurrentToken);
        }
        public AbstractSyntaxTree HandleExprTree () {
            AbstractSyntaxTree tree = new AbstractSyntaxTree (CurrentSession);
            switch (CurrentToken.Kind) {
                case TokenKind.StringToken:
                    HandleStringExpr (tree);
                    break;
                case TokenKind.DigitToken:
                    HandleIntExpr (tree);
                    break;
                case TokenKind.TrueToken:
                case TokenKind.FalseToken:
                case TokenKind.LeftParenthesisToken:
                    HandleBooleanExpr (tree);
                    break;
                case TokenKind.IdentifierToken:
                    HandleIdentifier (tree);
                    break;
                default:
                    break;
            }
            return tree;
        }
        public void NextToken () {
            if (TokenCounter + 1 < TokenStream.Count) {
                TokenCounter++;
            }
        }
        public void Traverse (ASTNode root, Action<ASTNode> checkFunction) {
            checkFunction (root);

            for (int i = 0; i < root.Descendants.Count; i++) {
                Traverse (root.Descendants[i], checkFunction);
            }
        }
        public void CheckScope (ASTNode node) {
            if (node.Token.Text == "Block") {
                Symbols.NewScope ();
                Symbols.UpdateCurrentScope ();
            } else if (node.Parent != null && node.Parent.Token.Kind == TokenKind.Block && node == node.Parent.Descendants[node.Parent.Descendants.Count - 1]) {
                if (Symbols.CurrentScope != null) {
                    if (node.Token.Text == "VarDecl") {
                        Symbols.AddSymbol (node.Descendants[1], node.Descendants[0].Token.Text);
                    } else if (node.Token.Text.Length == 1 && char.IsLetter (node.Token.Text, 0) && node.Parent.Token.Text != "VarDecl") {
                        Diagnostics.Semantic_ReportSymbolLookup (node.Token.Text);
                        bool success = SymbolExists (node.Token.Text, Symbols.CurrentScope);
                        if (!success) {
                            Diagnostics.Semantic_ReportUndeclaredIdentifier (node.Token, Symbols.CurrentScope.Level);
                        }
                    }
                }
                Symbols.AscendScope ();
            } else {
                if (Symbols.CurrentScope != null) {
                    if (node.Token.Text == "VarDecl") {
                        Symbols.AddSymbol (node.Descendants[1], node.Descendants[0].Token.Text);
                    } else if (node.Token.Text.Length == 1 && char.IsLetter (node.Token.Text, 0) && node.Parent.Token.Text != "VarDecl") {
                        Diagnostics.Semantic_ReportSymbolLookup (node.Token.Text);
                        bool success = SymbolExists (node.Token.Text, Symbols.CurrentScope);
                        if (!success) {
                            Diagnostics.Semantic_ReportUndeclaredIdentifier (node.Token, Symbols.CurrentScope.Level);
                        }
                    }
                }
            }
        }
        public void CheckType (ASTNode node) {
            switch (node.Token.Kind) {
                case TokenKind.AssignmentToken:
                    CheckAssignmentTypes (node);
                    break;
                case TokenKind.EquivalenceToken:
                case TokenKind.NotEqualToken:
                    CheckBoolOpTypes (node);
                    break;
                case TokenKind.AdditionToken:
                    CheckAdditionTypes (node);
                    break;
            }
        }
        public void CheckAssignmentTypes (ASTNode node) {
            string leftIdentifierType = GetSymbolType (node.Descendants[0].Token.Text, Symbols.CurrentScope);
            string rightExprType = GetExpressionType (node.Descendants[node.Descendants.Count - 1]);
            if (leftIdentifierType == rightExprType) {
                Diagnostics.Semantic_ReportMatchedTypes (node);
            } else {
                Diagnostics.Semantic_ReportTypeMismatch (node);
            }
        }
        public bool CheckBoolOpTypes (ASTNode node) {
            string leftExprType = GetExpressionType (node.Descendants[0]);
            string rightExprType = GetExpressionType (node.Descendants[1]);
            if (leftExprType == rightExprType) {
                Diagnostics.Semantic_ReportMatchedTypes (node);
            } else {
                Diagnostics.Semantic_ReportTypeMismatch (node);
            }
            return leftExprType == rightExprType;
        }
        public bool CheckAdditionTypes (ASTNode node) {
            string leftExprType = GetExpressionType (node.Descendants[0]);
            string rightExprType = "";
            switch (node.Descendants[1].Token.Kind) {
                case TokenKind.DigitToken:
                    rightExprType = GetExpressionType (node.Descendants[1]);
                    break;
                case TokenKind.IdentifierToken:
                    rightExprType = GetSymbolType (node.Descendants[1].Token.Text, Symbols.CurrentScope);
                    break;
                default:
                    return CheckAdditionTypes (node.Descendants[1]);
            }
            if (leftExprType == rightExprType) {
                Diagnostics.Semantic_ReportMatchedTypes (node);
            } else {
                Diagnostics.Semantic_ReportTypeMismatch (node);
            }
            return leftExprType == rightExprType;
        }
        public bool HandleBooleanExprType (ASTNode node) {
            if (node.Token.Kind == TokenKind.TrueToken || node.Token.Kind == TokenKind.FalseToken) {
                return true;
            } else {
                return CheckBoolOpTypes (node);
            }
        }
        public string GetExpressionType (ASTNode node) {
            switch (node.Token.Kind) {
                case TokenKind.StringToken:
                    return String;
                case TokenKind.TrueToken:
                case TokenKind.FalseToken:
                case TokenKind.EquivalenceToken:
                case TokenKind.NotEqualToken:
                    if (HandleBooleanExprType (node)) {
                        return Boolean;
                    }
                    break;
                case TokenKind.DigitToken:
                case TokenKind.AdditionToken:
                    if (HandleIntExprType (node)) {
                        return Integer;
                    }
                    break;
                case TokenKind.IdentifierToken:
                    return GetSymbolType (node.Token.Text, Symbols.CurrentScope);
                default:
                    break;
            }
            return "";
        }
        public bool HandleIntExprType (ASTNode node) {
            if (node.Descendants.Count == 0 && node.Token.Kind == TokenKind.DigitToken) {
                return true;
            } else {
                return CheckAdditionTypes (node);
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
                Diagnostics.Semantic_ReportSymbolNotFound (symbol, searchScope);
                return "";
            }
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
    }
}

// class SemanticAnalyser {
//     public Parser Parser { get; set; }
//     public AbstractSyntaxTree AbstractSyntaxTree { get; set; }

//     public SemanticAnalyser (Parser parser, Session session, DiagnosticCollection diagnostics) {
//         Parser = parser;
//         AbstractSyntaxTree = new AbstractSyntaxTree ();
//     }

//     public void Analyse () {
//         TraverseCST (Parser.Tree);
//     }

//     public void TraverseCST (ConcreteSyntaxTree tree) {
//         Tree.PrintTree (tree.Root);
//     }

//     public void Traverse (ASTNode root, Action<ASTNode> checkFunction) {
//         checkFunction (root);

//         for (int i = 0; i < root.Descendants.Count; i++) {
//             Traverse (root.Descendants[i], checkFunction);
//         }
//     }
// }

// public enum SpecialNodes {

// }
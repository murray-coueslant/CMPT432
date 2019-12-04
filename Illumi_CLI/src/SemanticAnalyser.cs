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
        public bool Failed { get; set; }
        public VariableChecker VariableChecker { get; set; }

        public SemanticAnalyser (Parser parser, Session currentSession, DiagnosticCollection diagnostics) {
            Parser = parser;
            ConcreteSyntaxTree = parser.Tree;
            TokenStream = parser.TokenStream;
            CurrentSession = currentSession;
            Diagnostics = diagnostics;
            TokenCounter = 0;
            Symbols = new SymbolTable (Diagnostics);
            Failed = false;
        }
        public void Analyse () {
            if (Parser.Failed) {
                Diagnostics.Semantic_ParserGaveNoTree ();
            } else {
                AbstractSyntaxTree = BuildAST ();
                if (Diagnostics.ErrorCount == 0) {
                    Diagnostics.Semantic_ReportDisplayingAST ();
                    AbstractSyntaxTree.PrintTree (AbstractSyntaxTree.Root);
                    VariableChecker = new VariableChecker (AbstractSyntaxTree, Symbols);
                    ScopeAndTypeCheck ();
                }
            }
            if (Diagnostics.ErrorCount > 0) {
                Failed = true;
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
            Diagnostics.Semantic_ReportCheckingScope ();
            VariableChecker.CheckVariables ();
            if (VariableChecker.Passed) {
                Diagnostics.Semantic_ReportDisplayingSymbolTables ();
                Symbols.DisplaySymbolTables (Symbols.RootScope);
            }
        }
        public void HandleBlock (AbstractSyntaxTree tree) {
            tree.AddBranchNode (new Token (TokenKind.Block, "Block", 0, 0));
            NextToken ();
            HandleStatement (tree);
            if (CurrentToken.Kind == TokenKind.RightBraceToken) {
                tree.Ascend (CurrentSession);
                switch (tree.CurrentNode.Token.Kind) {
                    case TokenKind.IfToken:
                    case TokenKind.WhileToken:
                        tree.Ascend (CurrentSession);
                        break;
                    default:
                        break;
                }
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
                    switch (tree.CurrentNode.Token.Kind) {
                        case TokenKind.IfToken:
                        case TokenKind.WhileToken:
                            tree.Ascend (CurrentSession);
                            break;
                        default:
                            break;
                    }
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
            tree.AddBranchNode (new Token (TokenKind.AssignmentToken, "Assign", CurrentToken.LineNumber, CurrentToken.LinePosition));
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
            NextToken ();
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
    }
}
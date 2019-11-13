using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Illumi_CLI {
    class SemanticAnalyser {
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
                    // todo Diagnostics.Semantic_DisplayingAST();
                    AbstractSyntaxTree.PrintTree (AbstractSyntaxTree.Root);
                    ScopeAndTypeCheck ();
                    Diagnostics.Semantic_ReportDisplayingSymbolTables ();
                    Console.WriteLine ();
                    Symbols.DisplaySymbolTables (Symbols.RootScope);
                }
            }
        }
        public AbstractSyntaxTree BuildAST (AbstractSyntaxTree inputTree = null) {
            AbstractSyntaxTree tree;
            if (inputTree == null) {
                tree = new AbstractSyntaxTree ();
            } else {
                tree = inputTree;
            }
            while (TokenCounter < TokenStream.Count) {
                switch (CurrentToken.Kind) {
                    case TokenKind.LeftBraceToken:
                        HandleBlock (tree);
                        break;
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
                    case TokenKind.RightBraceToken:
                        tree.Ascend (CurrentSession);
                        NextToken ();
                        break;
                    default:
                        NextToken ();
                        break;
                }
            }
            return tree;
        }
        public void ScopeAndTypeCheck () {
            Traverse (AbstractSyntaxTree.Root, CheckScope);
        }
        public void HandleBlock (AbstractSyntaxTree tree) {
            // Diagnostics.Semantic_ReportAddingASTNode()
            tree.AddBranchNode (new Token (TokenKind.Block, "Block", 0, 0));
            NextToken ();
        }
        public void HandleVariableDeclaration (AbstractSyntaxTree tree) {
            tree.AddBranchNode (new Token (TokenKind.VarDecl, "VarDecl", 0, 0));
            if (CurrentToken.Kind == TokenKind.Type_BooleanToken) {
                tree.AddLeafNode (CurrentToken);
                NextToken ();
                tree.AddLeafNode (CurrentToken);
            } else if (CurrentToken.Kind == TokenKind.Type_IntegerToken) {
                tree.AddLeafNode (CurrentToken);
                NextToken ();
                tree.AddLeafNode (CurrentToken);
            } else if (CurrentToken.Kind == TokenKind.Type_StringToken) {
                tree.AddLeafNode (CurrentToken);
                NextToken ();
                tree.AddLeafNode (CurrentToken);
            } else {
                // TODO diagnostics.Semantic_ReportInvalidType();
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
            if (CurrentToken.Kind == TokenKind.StringToken) {
                HandleStringExpr (tree);
            } else if (CurrentToken.Kind == TokenKind.DigitToken) {
                HandleIntExpr (tree);
            } else if (CurrentToken.Kind == TokenKind.TrueToken ||
                CurrentToken.Kind == TokenKind.FalseToken ||
                CurrentToken.Kind == TokenKind.LeftParenthesisToken) {
                HandleBooleanExpr (tree);
            } else if (CurrentToken.Kind == TokenKind.IdentifierToken) {
                HandleIdentifier (tree);
            }
            // tree.Ascend (CurrentSession);
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
            NextToken ();
        }
        public void HandleWhileStatement (AbstractSyntaxTree tree) {
            tree.AddBranchNode (CurrentToken);
            NextToken ();
            HandleBooleanExpr (tree);
            NextToken ();
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
                    break;
            }
            tree.Ascend (CurrentSession);
        }
        public void HandleParenthesisedExpression (AbstractSyntaxTree tree) {
            NextToken ();
            AbstractSyntaxTree leftExprTree = HandleExprTree ();
            NextToken ();
            tree.AddBranchNode (CurrentToken);
            tree.AddLeafNode (leftExprTree.Root);
            NextToken ();
            AbstractSyntaxTree rightExprTree = HandleExprTree ();
            NextToken ();
            tree.AddLeafNode (rightExprTree.Root);
            // tree.Ascend (CurrentSession);
        }
        public void HandleIdentifier (AbstractSyntaxTree tree) {
            tree.AddBranchNode (new Token (TokenKind.IdentifierToken, "Identifier", 0, 0));
            tree.AddLeafNode (CurrentToken);
        }
        public AbstractSyntaxTree HandleExprTree () {
            AbstractSyntaxTree tree = new AbstractSyntaxTree ();
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
            TokenCounter++;
        }
        public void Traverse (ASTNode root, Action<ASTNode> checkFunction) {
            checkFunction (root);

            for (int i = 0; i < root.Descendants.Count; i++) {
                Traverse (root.Descendants[i], checkFunction);
            }
        }
        public bool CheckSymbolScope (ASTNode root) {
            Traverse (root, CheckScope);
            if (Diagnostics.ErrorCount == 0) {
                return true;
            }
            return false;
        }
        // public bool CheckSymbolType (ASTNode root) {
        //     Traverse (root, CheckType);
        //     if (Diagnostics.ErrorCount == 0) {
        //         return true;
        //     }
        //     return false;
        // }
        public void CheckScope (ASTNode node) {
            if (node.Token.Text == "Block") {
                Symbols.NewScope ();
                Symbols.UpdateCurrentScope ();
            }
            if (Symbols.CurrentScope != null) {
                if (node.Token.Text == "VarDecl") {
                    Symbols.AddSymbol (node.Descendants[1], node.Descendants[0].Token.Text);
                }
                if (node.Token.Text == "Identifier") {
                    Diagnostics.Semantic_ReportSymbolLookup (node.Descendants[0].Token.Text);
                    bool success = SymbolExists (node.Descendants[0].Token.Text, Symbols.CurrentScope);
                    if (!success) {
                        Diagnostics.Semantic_ReportUndeclaredIdentifier (node.Descendants[0].Token, Symbols.CurrentScope.Level);
                    }
                }
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

// namespace Illumi_CLI {
//     class SemanticAnalyser {
//         public Parser Parser { get; }
//         public Session CurrentSession { get; }
//         public DiagnosticCollection Diagnostics { get; }
//         public Tree AbstractSyntaxTree { get; set; }
//         public SymbolTable Symbols { get; set; }
//         public int tokenCounter { get; set; }
//         public Token currentToken { get => Parser.TokenStream[tokenCounter]; }
//         public SemanticAnalyser (Parser parser, Session currentSession, DiagnosticCollection diagnostics) {
//             Parser = parser;
//             CurrentSession = currentSession;
//             Diagnostics = diagnostics;
//             AbstractSyntaxTree = new AbstractSyntaxTree ();
//             Symbols = new SymbolTable (Diagnostics);
//         }
//         public void Analyse () {
//             if (Parser.Failed) {
//                 Diagnostics.Semantic_ParserGaveNoTree ();
//             } else {
//                 TraverseParseTreeAndBuildASTAndSymbolTables ();
//                 if (Diagnostics.ErrorCount == 0) {
//                     Diagnostics.Semantic_ReportDisplayingSymbolTables ();
//                     Console.WriteLine ();
//                     Symbols.DisplaySymbolTables (Symbols.RootScope);
//                 }
//             }
//         }
//         public void Traverse (TreeNode root, Action<TreeNode> checkFunction) {
//             checkFunction (root);

//             for (int i = 0; i < root.Children.Count; i++) {
//                 Traverse (root.Children[i], checkFunction);
//             }
//         }
//         public bool CheckSymbolScope (TreeNode root) {
//             Traverse (root, CheckScope);
//             if (Diagnostics.ErrorCount == 0) {
//                 return true;
//             }
//             return false;
//         }
//         public bool CheckSymbolType (TreeNode root) {
//             Traverse (root, CheckType);
//             if (Diagnostics.ErrorCount == 0) {
//                 return true;
//             }
//             return false;
//         }
//         public void CheckScope (TreeNode node) {
//             if (node.Type == "Block") {
//                 Symbols.NewScope ();
//                 Symbols.UpdateCurrentScope ();
//             }
//             if (node.Type == "RightBraceToken") {
//                 Symbols.AscendScope ();
//             }
//             if (Symbols.CurrentScope != null) {
//                 if (node.Type == "VariableDeclaration") {
//                     Symbols.AddSymbol (node.Children[1].Children[0], node.Children[0].Children[0].NodeToken.Text);
//                 }
//                 if (node.Type == "Identifier") {
//                     Diagnostics.Semantic_ReportSymbolLookup (node.Children[0].NodeToken.Text);
//                     bool success = SymbolExists (node.Children[0].NodeToken.Text, Symbols.CurrentScope);
//                     if (!success) {
//                         Diagnostics.Semantic_ReportUndeclaredIdentifier (node.Children[0].NodeToken, Symbols.CurrentScope.Level);
//                     }
//                 }
//             }
//         }
//         public void CheckType (TreeNode node) {
//             switch (node.Type) {
//                 case "AssignmentStatement":
//                     string leftIdentifierType = GetSymbolType (node.Children[0].Children[0].NodeToken.Text, Symbols.CurrentScope);
//                     string rightExpressionType = GetExpressionType (node.Children[2]);
//                     System.Console.WriteLine ($"Assignment type checked: {leftIdentifierType == rightExpressionType}");
//                     break;
//                 default:
//                     break;
//             }
//         }
//         public string GetExpressionType (TreeNode expressionNode) {
//             switch (expressionNode.Children[0].Type) {
//                 case "Identifier":
//                     return GetSymbolType (expressionNode.Children[0].Children[0].NodeToken.Text, Symbols.CurrentScope);
//                 case "IntegerExpression":
//                     return GetExpressionType (expressionNode.Children[0]);
//                 default:
//                     return null;
//             }
//         }
//         public void TraverseParseTreeAndBuildASTAndSymbolTables () {
//             //TraverseAndBuildAST (Parser.Tree.Root, CheckSymbols);
//             tokenCounter = 0;
//             BuildAST (Parser);
//             Diagnostics.Semantic_ReportCheckingScope ();
//             if (CheckSymbolScope (Parser.Tree.Root)) {
//                 Diagnostics.Semantic_ReportCheckingType ();
//                 CheckSymbolType (Parser.Tree.Root);
//             }
//         }
//         public void BuildAST (Parser parser) {
//             foreach (Token token in Parser.TokenStream) {
//                 System.Console.WriteLine (token.Text);
//             }
//         }
//         public bool SymbolExists (string symbol, Scope searchScope) {
//             if (searchScope.Symbols.ContainsKey (symbol)) {
//                 Diagnostics.Semantic_ReportFoundSymbol (symbol, searchScope);
//                 return true;
//             } else {
//                 if (searchScope.ParentScope != null) {
//                     Diagnostics.Semantic_ReportSymbolNotFound (symbol, searchScope);
//                     return SymbolExists (symbol, searchScope.ParentScope);
//                 }
//                 return false;
//             }
//         }
//         public string GetSymbolType (string symbol, Scope searchScope) {
//             if (searchScope.Symbols.ContainsKey (symbol)) {
//                 Diagnostics.Semantic_ReportFoundSymbol (symbol, searchScope);
//                 return searchScope.Symbols[symbol].ToString ();
//             } else {
//                 if (searchScope.ParentScope != null) {
//                     Diagnostics.Semantic_ReportSymbolNotFound (symbol, searchScope);
//                     return GetSymbolType (symbol, searchScope.ParentScope);
//                 }
//                 return null;
//             }
//         }
//         public void NextToken () {
//             if (tokenCounter < Parser.TokenStream.Count) {
//                 tokenCounter++;
//             }
//         }
//     }
// }
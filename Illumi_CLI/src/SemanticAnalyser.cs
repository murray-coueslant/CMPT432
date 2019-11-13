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
        public SemanticAnalyser (Parser parser, Session currentSession, DiagnosticCollection diagnostics) {
            Parser = parser;
            ConcreteSyntaxTree = parser.Tree;
            TokenStream = parser.TokenStream;
            CurrentSession = currentSession;
            Diagnostics = diagnostics;
            TokenCounter = 0;
        }
        public void Analyse () {
            AbstractSyntaxTree = BuildAST ();
            AbstractSyntaxTree.PrintTree (AbstractSyntaxTree.Root);
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
                    default:
                        NextToken ();
                        break;
                }
            }
            return tree;
        }
        public void HandleBlock (AbstractSyntaxTree tree) {
            // Diagnostics.Semantic_ReportAddingASTNode()
            tree.AddBranchNode ("Block");
            NextToken ();
        }
        public void HandleVariableDeclaration (AbstractSyntaxTree tree) {
            tree.AddBranchNode ("VariableDeclaration");
            if (CurrentToken.Kind == TokenKind.Type_BooleanToken) {
                tree.AddLeafNode ("Boolean");
                NextToken ();
                tree.AddLeafNode (CurrentToken.Text);
            } else if (CurrentToken.Kind == TokenKind.Type_IntegerToken) {
                tree.AddLeafNode ("Integer");
                NextToken ();
                tree.AddLeafNode (CurrentToken.Text);
            } else if (CurrentToken.Kind == TokenKind.Type_StringToken) {
                tree.AddLeafNode ("String");
                NextToken ();
                tree.AddLeafNode (CurrentToken.Text);
            } else {
                // TODO diagnostics.Semantic_ReportInvalidType();
                return;
            }
            tree.Ascend (CurrentSession);
        }
        public void HandleAssignmentStatement (AbstractSyntaxTree tree) {
            tree.AddBranchNode ("AssignmentStatement");
            tree.AddLeafNode (TokenStream[TokenCounter - 1].Text);
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
            tree.Ascend (CurrentSession);
        }
        public void HandlePrintStatement (AbstractSyntaxTree tree) {
            tree.AddBranchNode ("Print");
            NextToken ();
            NextToken ();
            HandleExpression (tree);
            NextToken ();
            tree.Ascend (CurrentSession);
        }
        public void HandleIfStatement (AbstractSyntaxTree tree) {
            tree.AddBranchNode ("IfStatement");
            NextToken ();
            HandleBooleanExpr (tree);
            HandleBlock (tree);
        }
        public void HandleStringExpr (AbstractSyntaxTree tree) {
            tree.AddLeafNode (CurrentToken.Text);
            NextToken ();
        }
        public void HandleIntExpr (AbstractSyntaxTree tree) {
            if (TokenStream[TokenCounter + 1].Kind == TokenKind.AdditionToken) {
                tree.AddBranchNode ("Add");
                tree.AddLeafNode (CurrentToken.Text);
                NextToken ();
                NextToken ();
                HandleIntExpr (tree);
            } else {
                tree.AddBranchNode (CurrentToken.Text);
                NextToken ();
            }
            tree.Ascend (CurrentSession);
        }
        public void HandleBooleanExpr (AbstractSyntaxTree tree) {
            switch (CurrentToken.Kind) {
                case TokenKind.TrueToken:
                case TokenKind.FalseToken:
                case TokenKind.IdentifierToken:
                    tree.AddLeafNode (CurrentToken.Text);
                    //NextToken ();
                    break;
                default:
                    HandleParenthesisedExpression (tree);
                    break;
            }
        }
        public void HandleParenthesisedExpression (AbstractSyntaxTree tree) {
            NextToken ();
            AbstractSyntaxTree leftExprTree = HandleExprTree ();
            NextToken ();
            tree.AddBranchNode (CurrentToken.Text);
            tree.AddLeafNode (leftExprTree.Root);
            NextToken ();
            AbstractSyntaxTree rightExprTree = HandleExprTree ();
            NextToken ();
            tree.AddLeafNode (rightExprTree.Root);
        }
        public void HandleIdentifier (AbstractSyntaxTree tree) {
            tree.AddLeafNode (CurrentToken.Text);
            NextToken ();
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
            // tree.Ascend (CurrentSession);
            System.Console.WriteLine ("Token after tree handling: " + CurrentToken.Text);
            return tree;

        }
        public void NextToken () {
            TokenCounter++;
        }
    }
}
//         public void Traverse (TreeNode root, Action<TreeNode> checkFunction) {
//             checkFunction (root);

//             for (int i = 0; i < root.Children.Count; i++) {
//                 Traverse (root.Children[i], checkFunction);
//             }
//         }
//         public void BuildAST () {
//             Traverse (ConcreteSyntaxTree.Root, HandleCSTNode);
//             AbstractSyntaxTree.PrintTree (AbstractSyntaxTree.Root);
//         }
//         public void HandleCSTNode (TreeNode node) {
//             switch (node.Type) {
//                 case "Block":
//                     HandleBlock (node);
//                     break;
//                 case "VariableDeclaration":
//                     HandleVariableDeclaration (node);
//                     break;
//                 case "AssignmentStatement":
//                     HandleAssignmentStatement (node);
//                     break;
//                 case "IfStatement":
//                     HandleIfStatement (node);
//                     break;
//                 case "WhileStatement":
//                     HandleWhileStatement (node);
//                     break;
//                 case "PrintStatement":
//                     HandlePrintStatement (node);
//                     break;
//                 case "RightBraceToken":
//                     AbstractSyntaxTree.Ascend (CurrentSession);
//                     break;
//             }
//             //AbstractSyntaxTree.Ascend (CurrentSession);
//         }
//         public void HandleBlock (TreeNode node) {
//             AbstractSyntaxTree.AddBranchNode (node.Type);
//             //Traverse (node, HandleCSTNode);
//             //AbstractSyntaxTree.Ascend (CurrentSession);
//         }
//         public void HandleVariableDeclaration (TreeNode node) {
//             AbstractSyntaxTree.AddBranchNode (node.Type);
//             AbstractSyntaxTree.AddLeafNode (node.Children[0].Children[0].NodeToken.Text);
//             AbstractSyntaxTree.AddLeafNode (node.Children[1].Children[0].NodeToken.Text);
//             AbstractSyntaxTree.Ascend (CurrentSession);
//         }
//         public void HandleAssignmentStatement (TreeNode node) {
//             AbstractSyntaxTree.AddBranchNode (node.Type);
//             AbstractSyntaxTree.AddLeafNode (node.Children[0].Children[0].NodeToken.Text);
//             HandleExpression (node.Children[2]);
//             AbstractSyntaxTree.Ascend (CurrentSession);
//         }
//         public void HandleIfStatement (TreeNode node) {
//             AbstractSyntaxTree.AddBranchNode (node.Type);
//             HandleExpression (node.Children[0]);
//             AbstractSyntaxTree.Ascend (CurrentSession);
//             if (node.Children[1].Children[1].Children[0].Type == "Identifier") {
//                 AbstractSyntaxTree.AddLeafNode (node.Children[1].Children[1].Children[0].Children[0].NodeToken.Text);
//             } else {
//                 HandleExpression (node.Children[1].Children[1].Children[0]);
//             }
//         }
//         public void HandleWhileStatement (TreeNode node) {
//             AbstractSyntaxTree.AddBranchNode (node.Type);
//             if (node.Children[1].Children[1].Children[0].Type == "Identifier") {
//                 AbstractSyntaxTree.AddLeafNode (node.Children[1].Children[1].Children[0].Children[0].NodeToken.Text);
//             } else {
//                 HandleExpression (node.Children[1].Children[1].Children[0]);
//                 AbstractSyntaxTree.Ascend (CurrentSession);
//             }
//         }
//         public void HandlePrintStatement (TreeNode node) {
//             AbstractSyntaxTree.AddBranchNode (node.Type);
//             HandleExpression (node.Children[2]);
//             AbstractSyntaxTree.Ascend (CurrentSession);
//         }
//         public void HandleExpression (TreeNode node) {
//             Traverse (node, HandleCSTNode);
//             AbstractSyntaxTree.Ascend (CurrentSession);
//         }
//     }
// }

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
using System;
namespace Illumi_CLI {
    class TypeChecker {
        public AbstractSyntaxTree Tree { get; set; }
        public SymbolTable Symbols { get; set; }
        public bool Passed { get; set; }
        public const string String = "string";
        public const string Boolean = "boolean";
        public const string Integer = "int";
        public TypeChecker (AbstractSyntaxTree tree, SymbolTable symbols) {
            Tree = tree;
            Symbols = symbols;
        }
        public void CheckTypes (ASTNode node) {
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
                Symbols.Diagnostics.Semantic_ReportMatchedTypes (node);
            } else {
                Symbols.Diagnostics.Semantic_ReportTypeMismatch (node);
            }
        }
        public bool CheckBoolOpTypes (ASTNode node) {
            string leftExprType = GetExpressionType (node.Descendants[0]);
            string rightExprType = GetExpressionType (node.Descendants[1]);
            if (leftExprType == rightExprType) {
                Symbols.Diagnostics.Semantic_ReportMatchedTypes (node);
            } else {
                Symbols.Diagnostics.Semantic_ReportTypeMismatch (node);
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
                Symbols.Diagnostics.Semantic_ReportMatchedTypes (node);
            } else {
                Symbols.Diagnostics.Semantic_ReportTypeMismatch (node);
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
                Symbols.Diagnostics.Semantic_ReportFoundSymbol (symbol, searchScope);
                return searchScope.Symbols[symbol].ToString ();
            } else {
                if (searchScope.ParentScope != null) {
                    Symbols.Diagnostics.Semantic_ReportSymbolNotFound (symbol, searchScope);
                    return GetSymbolType (symbol, searchScope.ParentScope);
                }
                Symbols.Diagnostics.Semantic_ReportSymbolNotFound (symbol, searchScope);
                return "";
            }
        }
    }
}
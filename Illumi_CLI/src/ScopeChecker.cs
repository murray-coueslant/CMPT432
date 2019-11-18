using System;
using System.Diagnostics;
using System.Linq;
namespace Illumi_CLI {
    class ScopeChecker {
        internal AbstractSyntaxTree Tree { get; set; }
        internal SymbolTable Symbols { get; set; }
        public bool Passed { get; set; }
        public ScopeChecker (AbstractSyntaxTree tree, SymbolTable symbols) {
            Tree = tree;
            Symbols = symbols;
            Passed = false;
        }

        public void ScopeCheck () {
            Traverse (Tree.Root, CheckASTScope);
            if (Symbols.Diagnostics.ErrorCount == 0) {
                Passed = true;
            }
        }

        public void CheckASTScope (ASTNode node) {
            switch (node.Token.Kind) {
                case TokenKind.Block:
                    Symbols.NewScope ();
                    break;
                case TokenKind.IdentifierToken:
                    if (node.Parent.Token.Kind == TokenKind.VarDecl) {
                        Symbols.AddSymbol (node, node.Parent.Descendants[0].Token.Text);
                    } else {
                        if (!FindSymbol (node, Symbols.CurrentScope)) {
                            Symbols.Diagnostics.Semantic_ReportUndeclaredIdentifier (node.Token, Symbols.CurrentScope.Level);
                        }
                    }
                    break;
            }
        }

        public bool FindSymbol (ASTNode node, Scope searchScope) {
            if (searchScope.Symbols.ContainsKey (node.Token.Text)) {
                Symbols.Diagnostics.Semantic_ReportFoundSymbol (node.Token.Text, searchScope);
                return true;
            } else {
                if (searchScope.ParentScope != null) {
                    Symbols.Diagnostics.Semantic_ReportSymbolNotFound (node.Token.Text, searchScope);
                    return FindSymbol (node, searchScope.ParentScope);
                }
                return false;
            }
        }

        public void Traverse (ASTNode node, Action<ASTNode> visitor) {
            visitor (node);

            foreach (ASTNode descendant in node.Descendants) {
                Traverse (descendant, visitor);
            }
            if (node.Parent != null && node.Parent.Token.Kind == TokenKind.Block && node == node.Parent.Descendants.Last ()) {
                Symbols.AscendScope ();
            } else if (node.Token.Kind == TokenKind.Block && node.Descendants.Count == 0) {
                Symbols.AscendScope ();
            }
        }
    }
}
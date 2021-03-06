using System;
using System.Diagnostics;
using System.Linq;
namespace Illumi_CLI {
    class VariableChecker {
        internal AbstractSyntaxTree Tree { get; set; }
        internal SymbolTable Symbols { get; set; }
        public bool Passed { get; set; }
        public TypeChecker TypeChecker { get; set; }
        public VariableChecker (AbstractSyntaxTree tree, SymbolTable symbols) {
            Tree = tree;
            Symbols = symbols;
            Passed = false;
            TypeChecker = new TypeChecker (Tree, Symbols);
        }

        public void CheckVariables () {
            Traverse (Tree.Root, CheckASTScopeAndType);
            if (Symbols.Diagnostics.ErrorCount == 0) {
                Passed = true;
            }
            GenerateWarnings (Symbols.RootScope);
        }

        public void CheckASTScopeAndType (ASTNode node) {
            node.AppearsInScope = Symbols.CurrentScope;
            switch (node.Token.Kind) {
                case TokenKind.Block:
                    Symbols.NewScope ();
                    break;
                case TokenKind.IdentifierToken:
                    if (node.Parent.Token.Kind == TokenKind.VarDecl) {
                        Symbols.AddSymbol (node, node.Parent.Descendants[0].Token.Text);
                    } else {
                        int symbolScope = FindSymbol (node, Symbols.CurrentScope);
                        if (symbolScope == -1) {
                            Symbols.Diagnostics.Semantic_ReportUndeclaredIdentifier (node.Token, Symbols.CurrentScope.Level);
                            return;
                        } else {
                            node.ReferenceScope = symbolScope;
                        }
                    }
                    break;
                case TokenKind.AssignmentToken:
                case TokenKind.EquivalenceToken:
                case TokenKind.NotEqualToken:
                case TokenKind.AdditionToken:
                    TypeChecker.CheckTypes (node);
                    break;
            }
        }
        public int FindSymbol (ASTNode node, Scope searchScope) {
            if (searchScope.Symbols.ContainsKey (node.Token.Text)) {
                Symbols.Diagnostics.Semantic_ReportFoundSymbol (node.Token.Text, searchScope);
                if (node.Parent.Token.Kind == TokenKind.AssignmentToken) {
                    searchScope.Symbols[node.Token.Text].Initialized = true;
                } else {
                    searchScope.Symbols[node.Token.Text].Used = true;
                }
                return searchScope.Level;
            } else {
                if (searchScope.ParentScope != null) {
                    Symbols.Diagnostics.Semantic_ReportSymbolNotFound (node.Token.Text, searchScope);
                    return FindSymbol (node, searchScope.ParentScope);
                }
                return -1;
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
        public void GenerateWarnings (Scope rootScope) {
            foreach (var variable in rootScope.Symbols) {
                if (variable.Value.Initialized == false && variable.Value.Used == false) {
                    Symbols.Diagnostics.Semantic_ReportUnusedUninitializedVariable (variable.Value);
                } else if (variable.Value.Used == false) {
                    Symbols.Diagnostics.Semantic_ReportUnusedVariable (variable.Value);
                } else if (variable.Value.Initialized == false) {
                    Symbols.Diagnostics.Semantic_ReportUninitializedVariable (variable.Value);
                }
            }
            foreach (Scope scope in rootScope.DescendantScopes) {
                GenerateWarnings (scope);
            }
        }
    }
}
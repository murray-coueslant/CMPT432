using System;
using System.Collections;
using Microsoft.CodeAnalysis.Text;
namespace Illumi_CLI {
    class SymbolTable {
        public Scope RootScope { get; set; }
        public Scope CurrentScope { get; set; }
        public DiagnosticCollection Diagnostics { get; set; }
        public int ScopeCounter { get; set; }
        public SymbolTable (DiagnosticCollection diagnostics) {
            Diagnostics = diagnostics;
        }
        public void AddSymbol (TreeNode symbol, string type) {
            if (!CurrentScope.AddSymbol (symbol.NodeToken.Text, type)) {
                TextSpan errorSpan = new TextSpan (symbol.NodeToken.LinePosition, 1);
                Diagnostics.Semantic_ReportSymbolAlreadyDeclared (symbol.NodeToken.Text, CurrentScope.Level, errorSpan, symbol.NodeToken.LineNumber, type);
                return;
            }
            Diagnostics.Semantic_ReportAddingSymbol (symbol.NodeToken.Text, type, CurrentScope.Level);
        }
        public void AscendScope () {
            if (CurrentScope.ParentScope != null) {
                Diagnostics.Semantic_ReportAscendingScope (CurrentScope);
                CurrentScope = CurrentScope.ParentScope;
            } else {
                Diagnostics.Semantic_ReportReachedRootScope ();
            }
        }
        public void NewScope () {
            if (RootScope is null) {
                RootScope = new Scope (0);
                CurrentScope = RootScope;
                ScopeCounter = 0;
            } else {
                CurrentScope.AddDescendant (ScopeCounter);
                ScopeCounter++;
            }
        }
        public void UpdateCurrentScope () {
            if (CurrentScope.MostRecentScope != null) {
                CurrentScope = CurrentScope.MostRecentScope;
            }
        }
        public void DisplaySymbolTables (Scope RootScope) {
            RootScope.DisplayScope ();
            Console.WriteLine ();

            for (int i = 0; i < RootScope.DescendantScopes.Count; i++) {
                DisplaySymbolTables (RootScope.DescendantScopes[i]);
            }
        }
    }
}
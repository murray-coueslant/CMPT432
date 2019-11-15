using System;
using System.Collections;
using Microsoft.CodeAnalysis.Text;
namespace Illumi_CLI {
    class SymbolTable {
        public Scope RootScope { get; set; }
        public Scope CurrentScope { get; set; }
        public Scope PreviousScope { get; set; }
        public DiagnosticCollection Diagnostics { get; set; }
        public int ScopeCounter { get; set; }
        public SymbolTable (DiagnosticCollection diagnostics) {
            Diagnostics = diagnostics;
        }
        public void AddSymbol (ASTNode symbol, string type) {
            if (!CurrentScope.AddSymbol (symbol.Token.Text, type)) {
                TextSpan errorSpan = new TextSpan (symbol.Token.LinePosition, 1);
                Diagnostics.Semantic_ReportSymbolAlreadyDeclared (symbol.Token.Text, CurrentScope.Level, errorSpan, symbol.Token.LineNumber, type);
                return;
            }
            Diagnostics.Semantic_ReportAddingSymbol (symbol.Token.Text, type, CurrentScope.Level);
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
                CurrentScope.AddDescendant (ScopeCounter, CurrentScope);
                ScopeCounter++;
            }
        }
        public void UpdateCurrentScope () {
            if (CurrentScope.MostRecentScope != null) {
                PreviousScope = CurrentScope;
                CurrentScope = CurrentScope.MostRecentScope;
            }
        }
        public void DisplaySymbolTables (Scope RootScope) {
            if (RootScope != null) {
                RootScope.DisplayScope ();
                Console.WriteLine ();

                for (int i = 0; i < RootScope.DescendantScopes.Count; i++) {
                    DisplaySymbolTables (RootScope.DescendantScopes[i]);
                }
            }
        }
    }
}
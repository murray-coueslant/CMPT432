using System.Collections;
using System.ComponentModel.Design;
namespace Illumi_CLI {
    class SymbolTable {
        public Scope RootScope { get; set; }
        public Scope CurrentScope { get; set; }
        public DiagnosticCollection Diagnostics { get; set; }
        public SymbolTable (DiagnosticCollection diagnostics) {
            RootScope = new Scope (0);
            CurrentScope = RootScope;
            Diagnostics = diagnostics;
        }
        public void AddSymbol (string symbol, object attributes = null) {
            if (!CurrentScope.AddSymbol (symbol, attributes)) {
                System.Console.WriteLine ($"clash on {symbol}");
                // TODO Diagnostics.Semantic_ReportSymbolAlreadyDeclared(symbol);
            }
        }
        public void AscendScope () {
            if (CurrentScope != RootScope) {
                // TODO Diagnostics.Semantic_ReportAscendingScope(CurrentScope);
                CurrentScope = CurrentScope.ParentScope;
            }
        }
        public void NewScope () {
            CurrentScope.AddDescendant ();
        }
    }
}
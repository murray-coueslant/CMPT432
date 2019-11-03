using System;
using System.Collections;
using System.Collections.Generic;

namespace Illumi_CLI {
    class Scope {
        public int Level { get; set; }
        public Hashtable Symbols { get; set; }
        public Scope ParentScope { get; set; }
        public IList<Scope> DescendantScopes { get; set; }
        public Scope MostRecentScope { get; set; }

        public Scope (int level, Scope parentScope = null) {
            Level = level;
            Symbols = new Hashtable ();
            DescendantScopes = new List<Scope> ();
            ParentScope = parentScope;
        }
        public void AddDescendant (int counter) {
            Scope newScope = new Scope (counter + 1, this);
            DescendantScopes.Add (newScope);
            MostRecentScope = newScope;
        }
        public bool AddSymbol (string symbol, string type) {
            try {
                Symbols.Add (symbol, type);
                return true;
            } catch {
                return false;
            }
        }
        public void UpdateParent (Scope newParent) {
            ParentScope = newParent;
        }
        public void DisplayScope () {
            Console.WriteLine (ParentScope == null ? $"Table for Scope: {Level}" : $"Table for Scope: {Level} [ Parent: {ParentScope.Level} ]");
            Console.WriteLine (String.Format ("+ {0, -10} + {0, -10} +", "----------"));
            Console.WriteLine (String.Format ("+ {0, -10} + {1, -10} +", "Symbol", "Type"));
            Console.WriteLine (String.Format ("+ {0, -10} + {0, -10} +", "----------"));
            foreach (DictionaryEntry symbol in Symbols) {
                Console.WriteLine (String.Format ("| {0, -10} | {1, -10} |", symbol.Key, symbol.Value));
                Console.WriteLine (String.Format ("+ {0, -10} + {0, -10} +", "----------"));
            }
        }
    }
}
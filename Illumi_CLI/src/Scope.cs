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

        public void AddDescendant () {
            Scope newScope = new Scope (Level + 1, this);
            DescendantScopes.Add (newScope);
            MostRecentScope = newScope;
        }

        public bool AddSymbol (string symbol, object attributes = null) {
            try {
                Symbols.Add (symbol, attributes);
                return true;
            } catch {
                return false;
            }
        }

        public void UpdateParent (Scope newParent) {
            ParentScope = newParent;
        }
    }
}
using System.Collections;
namespace Illumi_CLI.src {
    public class Scope {
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

        public static AddDescendant () {
            Scope newScope = new Scope (level + 1, this);
            DescendantScopes.Add (newScope);
            MostRecentScope = newScope;
        }

        public static boolean AddSymbol (string symbol, object attributes) {
            try {
                Symbols.Add (symbol, attributes);
                return true;
            } catch {
                return false;
            }
        }

        public static UpdateParent (Scope newParent) {
            ParentScope = newParent;
        }
    }
}
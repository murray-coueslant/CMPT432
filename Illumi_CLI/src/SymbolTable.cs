using System.Collections;
namespace Illumi_CLI.src {
    public class SymbolTable {
        public Scope RootScope { get; set; }
        public Scope CurrentScope { get; set; }
        public SymbolTable () {
            RootScope = new Scope ();

        }
    }
}
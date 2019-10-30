using System.Security.Cryptography.X509Certificates;
namespace Illumi_CLI {
    class AbstractSyntaxTree : Tree {
        public AbstractSyntaxTree () : base () { }

        public void DisplayAST () {
            PrintTree (Root, "");
        }
    }
}
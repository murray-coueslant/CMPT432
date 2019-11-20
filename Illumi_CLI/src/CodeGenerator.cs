using System;
using System.Text;
namespace Illumi_CLI {
    class CodeGenerator {
        public DiagnosticCollection Diagnostics { get; set; }
        public Session CurrentSession { get; set; }
        public SemanticAnalyser SemanticAnalyser { get; set; }
        public StringBuilder CodeString { get; set; }
        public RuntimeImage Image { get; set; }
        public TempTable Temp { get; set; }
        public CodeGenerator (SemanticAnalyser semanticAnalyser, DiagnosticCollection diagnostics, Session session) {
            SemanticAnalyser = semanticAnalyser;
            Diagnostics = diagnostics;
            CurrentSession = session;
            Temp = new TempTable ();
        }
        public void Generate () {
            Traverse (SemanticAnalyser.AbstractSyntaxTree.Root, HandleSubtree);
            foreach (var entry in Temp.Rows) {
                System.Console.WriteLine ($"{entry.Type}, {entry.Offset}");
            }
        }
        public void HandleSubtree (ASTNode node) {
            if (node.Visited == false) {
                switch (node.Token.Kind) {
                    case TokenKind.VarDecl:
                        HandleVarDecl (node);
                        break;
                }
                node.Visited = true;
            }
        }
        public void HandleVarDecl (ASTNode node) {
            string varType = node.Descendants[0].Token.Text;
            string varName = node.Descendants[1].Token.Text;
            node.Descendants[0].Visited = true;
            node.Descendants[1].Visited = true;
            Temp.NewEntry (varName, varType, 0);
        }
        public void Traverse (ASTNode root, Action<ASTNode> visitor) {
            visitor (root);

            foreach (var descendant in root.Descendants) {
                Traverse (descendant, visitor);
            }
        }
    }
}
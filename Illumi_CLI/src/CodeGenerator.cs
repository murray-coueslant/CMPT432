using System;
using System.Security.Cryptography;
using System.Text;
namespace Illumi_CLI {
    class CodeGenerator {
        public DiagnosticCollection Diagnostics { get; set; }
        public Session CurrentSession { get; set; }
        public SemanticAnalyser SemanticAnalyser { get; set; }
        public StringBuilder CodeString { get; set; }
        public RuntimeImage Image { get; set; }
        public TempTable StaticTemp { get; set; }
        public TempTable HeapTemp { get; set; }
        public CodeGenerator (SemanticAnalyser semanticAnalyser, DiagnosticCollection diagnostics, Session session) {
            SemanticAnalyser = semanticAnalyser;
            Diagnostics = diagnostics;
            CurrentSession = session;
            CodeString = new StringBuilder ();
            StaticTemp = new TempTable ();
            HeapTemp = new TempTable (null, true);
            Image = new RuntimeImage ();
        }
        public void Generate () {
            Traverse (SemanticAnalyser.AbstractSyntaxTree.Root, HandleSubtree);
            HandleStaticVariables ();
            HandleHeapVariables ();
            for (int i = 0; i < Image.Bytes.GetLength (0); i++) {
                for (int j = 0; j < Image.Bytes.GetLength (1); j++) {
                    Console.Write (Image.Bytes[i, j] + " ");
                }
                Console.WriteLine ();
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
            Image.WriteByte ("A9");
            string varType = node.Descendants[0].Token.Text;
            string varName = node.Descendants[1].Token.Text;
            int varScope = node.Descendants[1].Scope;
            node.Visited = true;
            node.Descendants[0].Visited = true;
            node.Descendants[1].Visited = true;
            string[] tempAddressBytes = new string[2];
            switch (varType) {
                case "int":
                case "boolean":
                    StaticTemp.NewEntry (varName, varType, varScope);
                    tempAddressBytes = StaticTemp.MostRecentEntry.TempAddress.Split (" ");
                    Image.WriteByte ("00");
                    break;
                default:
                    // todo work out logic for adding strings to the heap and their pointers to the temp table
                    HeapTemp.NewEntry (varName, varType, varScope);
                    tempAddressBytes = HeapTemp.MostRecentEntry.TempAddress.Split (" ");
                    Image.WriteByte ()
                    break;
            }
            Image.WriteByte ("8D");
            Image.WriteByte (tempAddressBytes[0]);
            Image.WriteByte (tempAddressBytes[1]);
        }
        public void HandleStaticVariables () {
            foreach (var entry in StaticTemp.Rows) {
                int counter = 0;
                do {
                    Image.WriteByte ("SS");
                    counter++;
                } while (counter < entry.Offset);
            }
        }
        public void HandleHeapVariables () {
            foreach (var entry in HeapTemp.Rows) {
                int counter = 0;
                do {
                    Image.WriteByte ("HH");
                    counter++;
                } while (counter < entry.Offset);
            }
        }
        public void Traverse (ASTNode root, Action<ASTNode> visitor) {
            visitor (root);

            foreach (var descendant in root.Descendants) {
                Traverse (descendant, visitor);
            }
        }
    }
}
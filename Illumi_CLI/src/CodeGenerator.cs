using System;
using System.Linq;
using System.Reflection;
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
            Image.WriteByte ("00");
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
                    case TokenKind.AssignmentToken:
                        HandleAssignment (node);
                        break;
                    case TokenKind.PrintToken:
                        HandlePrint (node);
                        break;
                    default:
                        break;
                }
                node.Visited = true;
            }
        }
        public void HandleVarDecl (ASTNode node) {
            Image.WriteByte ("A9");
            string varType = node.Descendants[0].Token.Text;
            string varName = node.Descendants[1].Token.Text;
            Scope varScope = node.Descendants[1].Scope;
            node.Visited = true;
            node.Descendants[0].Visited = true;
            node.Descendants[1].Visited = true;
            string[] tempAddressBytes = new string[2];
            switch (varType) {
                case "int":
                case "boolean":
                    StaticTemp.NewStaticEntry (varName, "00", varType, varScope.Level);
                    tempAddressBytes = StaticTemp.MostRecentEntry.Address.Split (" ");
                    Image.WriteByte ("00");
                    break;
                default:
                    // todo work out logic for adding strings to the heap and their pointers to the temp table
                    HeapTemp.NewHeapEntry (varName, varScope.Level);
                    // tempAddressBytes = HeapTemp.MostRecentEntry.TempAddress.Split (" ");
                    // Image.WriteByte ();
                    break;
            }
            Image.WriteByte ("8D");
            Image.WriteByte (tempAddressBytes[0]);
            Image.WriteByte (tempAddressBytes[1]);
        }
        public void HandleAssignment (ASTNode node) {
            string variableName = node.Descendants[0].Token.Text;
            string type = node.Scope.Symbols[variableName].Type;
            string[] tempAddressBytes = StaticTemp.GetTempTableEntry (variableName, node.Scope).Address.Split (" ");
            switch (type) {
                case "int":
                    HandleInteger (node, tempAddressBytes);
                    break;
                case "boolean":
                    HandleBoolean (node, tempAddressBytes);
                    break;
                case "string":
                    //HandleString (tempAddressBytes);
                    break;
            }
        }
        public void HandlePrint (ASTNode node) {
            switch (node.Descendants[0].Token.Kind) {
                case TokenKind.IdentifierToken:
                    HandlePrintIdentifier (node.Descendants[0]);
                    break;
                case TokenKind.AdditionToken:
                case TokenKind.DigitToken:
                    HandlePrintIntegerExpression (node.Descendants[0]);
                    break;
            }
        }
        public void HandlePrintIdentifier (ASTNode node) {
            string[] tempAddressBytes = StaticTemp.GetTempTableEntry (node.Token.Text, node.Scope).Address.Split (" ");
            Image.WriteByte ("AC");
            Image.WriteByte (tempAddressBytes[0]);
            Image.WriteByte (tempAddressBytes[1]);
            Image.WriteByte ("A2");
            Image.WriteByte ("01");
            Image.WriteByte ("FF");
        }
        public void HandlePrintIntegerExpression (ASTNode node) {
            int expressionValue = HandleIntegerExpression (node);
            string hexValue = expressionValue.ToString ("X2");
            Image.WriteByte ("A0");
            Image.WriteByte (hexValue);
            Image.WriteByte ("A2");
            Image.WriteByte ("01");
            Image.WriteByte ("FF");
        }
        public void HandleInteger (ASTNode node, string[] tempAddressBytes) {
            int value = HandleIntegerExpression (node.Descendants[1]);
            StaticTemp.GetTempTableEntry (node.Descendants[0].Token.Text, node.Scope).Value = value.ToString ();
            Image.WriteByte ("A9");
            Image.WriteByte (value.ToString ());
            Image.WriteByte ("8D");
            Image.WriteByte (tempAddressBytes[0]);
            Image.WriteByte (tempAddressBytes[1]);
        }
        public int HandleIntegerExpression (ASTNode node) {
            int outInteger;
            switch (node.Token.Kind) {
                case TokenKind.DigitToken:
                    int.TryParse (node.Token.Text, out outInteger);
                    return outInteger;
                case TokenKind.IdentifierToken:
                    int.TryParse (StaticTemp.GetTempTableEntry (node.Token.Text, node.Scope).Value, out outInteger);
                    return outInteger;
                case TokenKind.AdditionToken:
                    return HandleIntegerExpression (node.Descendants[0]) + HandleIntegerExpression (node.Descendants[1]);
                default:
                    return 0;
            }
        }
        public void HandleBoolean (ASTNode node, string[] tempAddressBytes) {
            int boolVal = EvaluateBooleanSubtree (node.Descendants[1]);
            StaticTemp.GetTempTableEntry (node.Descendants[0].Token.Text, node.Scope).Value = boolVal.ToString ();
            Image.WriteByte ("A9");
            if (boolVal == 1) {
                Image.WriteByte ("01");
            } else {
                Image.WriteByte ("00");
            }
            Image.WriteByte ("8D");
            Image.WriteByte (tempAddressBytes[0]);
            Image.WriteByte (tempAddressBytes[1]);
        }
        public int EvaluateBooleanSubtree (ASTNode node) {
            if (node.Descendants.Count == 0) {
                if (node.Token.Kind == TokenKind.TrueToken) {
                    return 1;
                } else if (node.Token.Kind == TokenKind.FalseToken) {
                    return 0;
                } else {
                    return GetBoolVarValue (node.Token.Text, node.Scope);
                }
            } else {
                if (node.Token.Kind == TokenKind.EquivalenceToken || node.Token.Kind == TokenKind.NotEqualToken) {
                    if (node.Token.Kind == TokenKind.EquivalenceToken) {
                        return HandleEquivalence (node);
                    } else {
                        return HandleNotEqual (node);
                    }
                } else {
                    return 0;
                }
            }
        }
        public int HandleEquivalence (ASTNode node) {
            if (EvaluateBooleanSubtree (node.Descendants[0]) == EvaluateBooleanSubtree (node.Descendants[1])) {
                return 1;
            } else {
                return 0;
            }
        }
        public int HandleNotEqual (ASTNode node) {
            if (EvaluateBooleanSubtree (node.Descendants[0]) != EvaluateBooleanSubtree (node.Descendants[1])) {
                return 1;
            } else {
                return 0;
            }
        }
        public int GetBoolVarValue (string variableName, Scope variableScope) {
            int outInteger;
            int.TryParse (StaticTemp.Rows.Where (r => r.Scope == variableScope.Level && r.Var == variableName).ToList ().FirstOrDefault ().Value, out outInteger);
            return outInteger;
        }
        public void HandleStaticVariables () {
            foreach (var entry in StaticTemp.Rows) {
                int counter = 0;
                do {
                    Image.BackPatch (entry, Image.GetCurrentAddress ());
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
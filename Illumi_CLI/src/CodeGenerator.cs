using System;
using System.Collections.Generic;
using System.Linq;
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
        public string TrueAddress { get; set; }
        public string FalseAddress { get; set; }
        public string AdditionAddress { get; set; }
        public string TermAddress { get; set; }
        public List<int> AdditionTreeStream { get; set; }
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
            InsertConstants ();
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
        public void InsertConstants () {
            TrueAddress = "F5 00";
            Image.WriteByte (char.ConvertToUtf32 ("t", 0).ToString ("X2"), "F5");
            Image.WriteByte (char.ConvertToUtf32 ("r", 0).ToString ("X2"), "F6");
            Image.WriteByte (char.ConvertToUtf32 ("u", 0).ToString ("X2"), "F7");
            Image.WriteByte (char.ConvertToUtf32 ("e", 0).ToString ("X2"), "F8");
            Image.WriteByte ("00", "F9");
            StaticTemp.NewStaticEntry ("true", TrueAddress, "pointer", 0);
            FalseAddress = "FA 00";
            Image.WriteByte (char.ConvertToUtf32 ("f", 0).ToString ("X2"), "FA");
            Image.WriteByte (char.ConvertToUtf32 ("a", 0).ToString ("X2"), "FB");
            Image.WriteByte (char.ConvertToUtf32 ("l", 0).ToString ("X2"), "FC");
            Image.WriteByte (char.ConvertToUtf32 ("s", 0).ToString ("X2"), "FD");
            Image.WriteByte (char.ConvertToUtf32 ("e", 0).ToString ("X2"), "FE");
            Image.WriteByte ("00", "FF");
            StaticTemp.NewStaticEntry ("false", FalseAddress, "pointer", 0);
            AdditionAddress = "F4 00";
            StaticTemp.NewStaticEntry ("addition", AdditionAddress, "pointer", 0);
            TermAddress = "F3 00";
            StaticTemp.NewStaticEntry ("term", TermAddress, "pointer", 0);
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
            string[] tempAddressBytes = StaticTemp.GetTempTableEntry (variableName, node.Scope.Level).Address.Split (" ");
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
                case TokenKind.TrueToken:
                case TokenKind.FalseToken:
                case TokenKind.EquivalenceToken:
                case TokenKind.NotEqualToken:
                    HandlePrintBoolean (node.Descendants[0]);
                    break;
                case TokenKind.StringToken:
                    // todo again this will need some heap memory that will have to be allocated at the end of compile time
                    // todo through backpatching
                    // HandlePrintString();
                    break;
            }
        }
        public void HandlePrintIdentifier (ASTNode node) {
            string[] tempAddressBytes = StaticTemp.GetTempTableEntry (node.Token.Text, node.Scope.Level).Address.Split (" ");
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
            Image.WriteByte ("02");
            Image.WriteByte ("FF");
        }
        public void HandlePrintBoolean (ASTNode node) {
            string[] valueAddress;
            int booleanValue = EvaluateBooleanSubtree (node);
            if (booleanValue == 1) {
                valueAddress = StaticTemp.GetTempTableEntry ("true", 0).Address.Split (" ");
            } else {
                valueAddress = StaticTemp.GetTempTableEntry ("false", 0).Address.Split (" ");
            }
            Image.WriteByte ("AC");
            Image.WriteByte (valueAddress[0]);
            Image.WriteByte (valueAddress[1]);
            Image.WriteByte ("A2");
            Image.WriteByte ("02");
            Image.WriteByte ("FF");
        }
        public void HandleInteger (ASTNode node, string[] tempAddressBytes) {
            int value = HandleIntegerExpression (node.Descendants[1]);
            if (value != -1) {
                StaticTemp.GetTempTableEntry (node.Descendants[0].Token.Text, node.Scope.Level).Value = value.ToString ("X2");
                Image.WriteByte ("A9");
                Image.WriteByte (value.ToString ("X2"));
                Image.WriteByte ("8D");
                Image.WriteByte (tempAddressBytes[0]);
                Image.WriteByte (tempAddressBytes[1]);
            }
        }
        public int HandleIntegerExpression (ASTNode node) {
            int outInteger;
            switch (node.Token.Kind) {
                case TokenKind.DigitToken:
                    int.TryParse (node.Token.Text, out outInteger);
                    return outInteger;
                case TokenKind.IdentifierToken:
                    int.TryParse (StaticTemp.GetTempTableEntry (node.Token.Text, node.Scope.Level).Value, out outInteger);
                    return outInteger;
                case TokenKind.AdditionToken:
                    HandleAddition (node);
                    return -1;
                default:
                    return -1;
            }
        }
        public void HandleAddition (ASTNode node) {
            AdditionTreeStream = new List<int> ();
            string[] splitAdditionAddress = AdditionAddress.Split (" ");
            string[] splitTermAddress = TermAddress.Split (" ");
            Traverse (node, CreateAdditionTreeStream);
            foreach (var item in AdditionTreeStream) {
                Image.WriteByte ("A9");
                Image.WriteByte (item.ToString ("X2"));
                Image.WriteByte ("8D");
                Image.WriteByte (splitTermAddress[0]);
                Image.WriteByte (splitTermAddress[1]);
                Image.WriteByte ("AD");
                Image.WriteByte (splitAdditionAddress[0]);
                Image.WriteByte (splitAdditionAddress[1]);
                Image.WriteByte ("6D");
                Image.WriteByte (splitTermAddress[0]);
                Image.WriteByte (splitTermAddress[1]);
                Image.WriteByte ("8D");
                Image.WriteByte (splitAdditionAddress[0]);
                Image.WriteByte (splitAdditionAddress[1]);
            }
        }
        public void CreateAdditionTreeStream (ASTNode node) {
            switch (node.Token.Kind) {
                case TokenKind.DigitToken:
                    AdditionTreeStream.Add (int.Parse (node.Token.Text));
                    break;
                case TokenKind.IdentifierToken:
                    string value = StaticTemp.GetTempTableEntry (node.Token.Text, node.Scope.Level).Value;
                    AdditionTreeStream.Add (int.Parse (value));
                    break;
            }
        }
        public void HandleBoolean (ASTNode node, string[] tempAddressBytes) {
            int boolVal = EvaluateBooleanSubtree (node.Descendants[1]);
            StaticTemp.GetTempTableEntry (node.Descendants[0].Token.Text, node.Scope.Level).Value = boolVal.ToString ();
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
                Image.BackPatch (entry, Image.GetCurrentAddress ());
                do {
                    Image.WriteByte (entry.Value.Split (" ") [counter]);
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
        public void FillEmptyBytes () {
            for (int i = 0; i < Image.Bytes.GetLength (0); i++) {
                for (int j = 0; j < Image.Bytes.GetLength (1); j++) {
                    if (Image.Bytes[i, j] == String.Empty) {
                        Image.Bytes[i, j] = "00";
                    }
                }
            }
        }
    }
}
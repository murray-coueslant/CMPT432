using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Win32.SafeHandles;
namespace Illumi_CLI {
    class CodeGenerator {
        public DiagnosticCollection Diagnostics { get; set; }
        public Session CurrentSession { get; set; }
        public SemanticAnalyser SemanticAnalyser { get; set; }
        public StringBuilder CodeString { get; set; }
        public RuntimeImage Image { get; set; }
        public TempTable StaticTemp { get; set; }
        public JumpTable JumpTable { get; set; }
        public string TrueAddress { get; set; }
        public string FalseAddress { get; set; }
        public string AdditionAddress { get; set; }
        public string TermAddress { get; set; }
        public string ConditionResultAddress { get; set; }
        public int JumpLength { get; set; }
        public List<int> AdditionTreeStream { get; set; }
        public CodeGenerator (SemanticAnalyser semanticAnalyser, DiagnosticCollection diagnostics, Session session) {
            SemanticAnalyser = semanticAnalyser;
            Diagnostics = diagnostics;
            CurrentSession = session;
            StaticTemp = new TempTable ();
            JumpTable = new JumpTable ();
            Image = new RuntimeImage ();
        }
        public void Generate () {
            InsertConstants ();
            Traverse (SemanticAnalyser.AbstractSyntaxTree.Root, HandleSubtree);
            Image.WriteByte ("00");
            HandleStaticVariables ();
            HandleJumps ();
            DisplayRuntimeImage ();
        }
        public void DisplayRuntimeImage () {
            for (int i = 0; i < Image.Bytes.GetLength (0); i++) {
                for (int j = 0; j < Image.Bytes.GetLength (1); j++) {
                    Console.Write (Image.Bytes[i, j] + " ");
                }
                Console.WriteLine ();
            }
        }
        public void InsertConstants () {
            InsertStringInHeap ("true");
            TrueAddress = Image.GetLastHeapAddress ();
            StaticTemp.NewStaticEntry ("true", TrueAddress, "pointer", 0);
            InsertStringInHeap ("false");
            FalseAddress = Image.GetLastHeapAddress ();
            StaticTemp.NewStaticEntry ("false", FalseAddress, "pointer", 0);
            Image.AllocateBytesInHeap (1);
            AdditionAddress = Image.GetLastHeapAddress ();
            StaticTemp.NewStaticEntry ("addition", AdditionAddress, "pointer", 0);
            Image.AllocateBytesInHeap (1);
            TermAddress = Image.GetLastHeapAddress ();
            StaticTemp.NewStaticEntry ("term", TermAddress, "pointer", 0);
            Image.AllocateBytesInHeap (1);
            ConditionResultAddress = Image.GetLastHeapAddress ();
            StaticTemp.NewStaticEntry ("conditional", ConditionResultAddress, "pointer", 0);
        }
        public void HandleSubtree (ASTNode node) {
            string startByte = Image.GetCurrentAddress ();
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
                    case TokenKind.IfToken:
                        HandleIfStatement (node);
                        break;
                    case TokenKind.Block:
                        HandleSubtree (node.Descendants[0]);
                        break;
                    default:
                        break;
                }
                node.Visited = true;
            }
            string endByte = Image.GetCurrentAddress ();
            int startNum = int.Parse (startByte.Split (" ") [0], NumberStyles.HexNumber);
            int endNum = int.Parse (endByte.Split (" ") [0], NumberStyles.HexNumber);
            JumpLength = endNum - startNum;
        }
        public void HandleVarDecl (ASTNode node) {
            Image.WriteByte ("A9");
            Image.WriteByte ("00");
            string varType = node.Descendants[0].Token.Text;
            string varName = node.Descendants[1].Token.Text;
            int varScope = SemanticAnalyser.VariableChecker.FindSymbol (node.Descendants[1], node.AppearsInScope);
            node.Visited = true;
            node.Descendants[0].Visited = true;
            node.Descendants[1].Visited = true;
            string[] tempAddressBytes = new string[2];
            StaticTemp.NewStaticEntry (varName, "00", varType, varScope);
            tempAddressBytes = StaticTemp.GetTempTableEntry (varName, varScope).Address.Split (" ");
            Image.WriteByte ("8D");
            Image.WriteByte (tempAddressBytes[0]);
            Image.WriteByte (tempAddressBytes[1]);
        }
        public void HandleAssignment (ASTNode node) {
            string variableName = node.Descendants[0].Token.Text;
            int variableScope = SemanticAnalyser.VariableChecker.FindSymbol (node.Descendants[0], node.AppearsInScope);
            TempTableEntry variableTemp = StaticTemp.GetTempTableEntry (variableName, variableScope);
            string type = variableTemp.Type;
            string[] tempAddressBytes = variableTemp.Address.Split (" ");
            switch (type) {
                case "int":
                    HandleIntegerAssignment (node, variableScope, tempAddressBytes);
                    break;
                case "boolean":
                    HandleBooleanAssignment (node, variableScope, tempAddressBytes);
                    break;
                case "string":
                    HandleStringAssignment (node, variableScope, tempAddressBytes);
                    break;
            }
            node.Descendants[0].Visited = true;
            node.Descendants[1].Visited = true;
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
                    HandlePrintString (node.Descendants[0]);
                    break;
            }
            node.Descendants[0].Visited = true;
        }
        public void HandleIfStatement (ASTNode node) {
            // handle condition with the bne command
            // add new entry to jump table
            // write code for associated block
            string[] splitConditionAddress = ConditionResultAddress.Split (" ");
            int conditionResult = EvaluateBooleanSubtree (node.Descendants[0]);
            Image.WriteByte ("A9");
            Image.WriteByte (conditionResult.ToString ("X2"));
            Image.WriteByte ("8D");
            Image.WriteByte (splitConditionAddress[0]);
            Image.WriteByte (splitConditionAddress[1]);
            Image.WriteByte ("A2");
            Image.WriteByte ("01");
            Image.WriteByte ("EC");
            Image.WriteByte (splitConditionAddress[0]);
            Image.WriteByte (splitConditionAddress[1]);

            JumpTableEntry jumpEntry = JumpTable.NewJumpEntry ();
            Image.WriteByte ("D0");
            Image.WriteByte (jumpEntry.Name);
            HandleSubtree (node.Descendants[1]);
            jumpEntry.JumpLength = JumpLength;
            JumpLength = 0;
        }
        public void HandlePrintIdentifier (ASTNode node) {
            TempTableEntry staticEntry = StaticTemp.GetTempTableEntry (node.Token.Text, node.ReferenceScope);
            string variableType = staticEntry.Type;
            string[] tempAddressBytes = staticEntry.Address.Split (" ");
            switch (variableType) {
                case "int":
                    Image.WriteByte ("AC");
                    Image.WriteByte (tempAddressBytes[0]);
                    Image.WriteByte (tempAddressBytes[1]);
                    Image.WriteByte ("A2");
                    Image.WriteByte ("01");
                    Image.WriteByte ("FF");
                    break;
                case "boolean":
                    HandlePrintBoolean (EvaluateBooleanSubtree (node));
                    break;
                case "string":
                    HandlePrintString (node);
                    break;
            }

        }
        public void HandlePrintIntegerExpression (ASTNode node) {
            int expressionValue = HandleIntegerExpression (node);
            if (expressionValue != -1) {
                string hexValue = expressionValue.ToString ("X2");
                Image.WriteByte ("A0");
                Image.WriteByte (hexValue);
                Image.WriteByte ("A2");
                Image.WriteByte ("02");
                Image.WriteByte ("FF");
            } else if (expressionValue == -1) {
                string[] splitAddress = AdditionAddress.Split (" ");
                Image.WriteByte ("AC");
                Image.WriteByte (splitAddress[0]);
                Image.WriteByte (splitAddress[1]);
                Image.WriteByte ("A2");
                Image.WriteByte ("01");
                Image.WriteByte ("FF");
            }
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
        public void HandlePrintBoolean (int booleanValue) {
            string[] valueAddress;
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
        public void HandlePrintString (ASTNode node) {
            switch (node.Token.Kind) {
                case TokenKind.IdentifierToken:
                    string[] valueAddress = StaticTemp.GetTempTableEntry (node.Token.Text, node.ReferenceScope).Address.Split (" ");
                    HandlePrintString (valueAddress);
                    break;
                case TokenKind.StringToken:
                    InsertStringInHeap (node.Token.Text);
                    string stringAddress = Image.GetLastHeapAddress ();
                    HandlePrintString (stringAddress);
                    break;
                default:
                    break;
            }
        }
        public void HandlePrintString (string startAddress) {
            string[] splitAddress = startAddress.Split (" ");
            Image.WriteByte ("A0");
            Image.WriteByte (splitAddress[0]);
            Image.WriteByte ("A2");
            Image.WriteByte ("02");
            Image.WriteByte ("FF");
        }
        public void HandlePrintString (string[] pointerAddress) {
            Image.WriteByte ("AC");
            Image.WriteByte (pointerAddress[0]);
            Image.WriteByte (pointerAddress[1]);
            Image.WriteByte ("A2");
            Image.WriteByte ("02");
            Image.WriteByte ("FF");
        }
        public void HandleIntegerAssignment (ASTNode node, int variableScope, string[] tempAddressBytes) {
            int value = HandleIntegerExpression (node.Descendants[1]);
            if (value != -1) {
                StaticTemp.GetTempTableEntry (node.Descendants[0].Token.Text, variableScope).Value = value.ToString ("X2");
                Image.WriteByte ("A9");
                Image.WriteByte (value.ToString ("X2"));
                Image.WriteByte ("8D");
                Image.WriteByte (tempAddressBytes[0]);
                Image.WriteByte (tempAddressBytes[1]);
            } else if (value == -1) {
                string[] splitAdditionAddress = AdditionAddress.Split (" ");
                Image.WriteByte ("AD");
                Image.WriteByte (splitAdditionAddress[0]);
                Image.WriteByte (splitAdditionAddress[1]);
                Image.WriteByte ("8D");
                Image.WriteByte (tempAddressBytes[0]);
                Image.WriteByte (tempAddressBytes[1]);
                ResetAdditionMemory ();
            }
            node.Descendants[0].Visited = true;
            node.Descendants[1].Visited = true;
        }
        public void HandleStringAssignment (ASTNode node, int variableScope, string[] tempAddressBytes) {
            // write string in heap
            // store heap address in temp pointer position
            InsertStringInHeap (node.Descendants[1].Token.Text);
            StaticTemp.GetTempTableEntry (tempAddressBytes[0] + " " + tempAddressBytes[1]).Value = Image.GetLastHeapAddress ();
            Image.WriteByte ("A9");
            Image.WriteByte (StaticTemp.GetTempTableEntry (tempAddressBytes[0] + " " + tempAddressBytes[1]).Value.Split (" ") [0]);
            Image.WriteByte ("8D");
            Image.WriteByte (tempAddressBytes[0]);
            Image.WriteByte (tempAddressBytes[1]);
            node.Descendants[1].Visited = true;
        }
        public void ResetAdditionMemory () {
            string[] splitAdditionAddress = AdditionAddress.Split (" ");
            string[] splitTermAddress = TermAddress.Split (" ");
            Image.WriteByte ("A9");
            Image.WriteByte ("00");
            Image.WriteByte ("8D");
            Image.WriteByte (splitAdditionAddress[0]);
            Image.WriteByte (splitAdditionAddress[1]);
            Image.WriteByte ("A9");
            Image.WriteByte ("00");
            Image.WriteByte ("8D");
            Image.WriteByte (splitTermAddress[0]);
            Image.WriteByte (splitTermAddress[1]);
        }
        public int HandleIntegerExpression (ASTNode node) {
            int outInteger;
            switch (node.Token.Kind) {
                case TokenKind.DigitToken:
                    int.TryParse (node.Token.Text, out outInteger);
                    return outInteger;
                case TokenKind.IdentifierToken:
                    int.TryParse (StaticTemp.GetTempTableEntry (node.Token.Text, node.ReferenceScope).Value, out outInteger);
                    return outInteger;
                case TokenKind.AdditionToken:
                    HandleAddition (node);
                    return -1;
                default:
                    return -2;
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
                    string value = StaticTemp.GetTempTableEntry (node.Token.Text, node.ReferenceScope).Value;
                    AdditionTreeStream.Add (int.Parse (value));
                    break;
            }
        }
        public void HandleBooleanAssignment (ASTNode node, int variableScope, string[] tempAddressBytes) {
            int boolVal = EvaluateBooleanSubtree (node.Descendants[1]);
            StaticTemp.GetTempTableEntry (node.Descendants[0].Token.Text, variableScope).Value = boolVal.ToString ("X2");
            Image.WriteByte ("A9");
            if (boolVal == 1) {
                Image.WriteByte ("01");
            } else {
                Image.WriteByte ("00");
            }
            Image.WriteByte ("8D");
            Image.WriteByte (tempAddressBytes[0]);
            Image.WriteByte (tempAddressBytes[1]);
            node.Descendants[0].Visited = true;
            node.Descendants[1].Visited = true;
        }
        public int EvaluateBooleanSubtree (ASTNode node) {
            if (node.Descendants.Count == 0) {
                if (node.Token.Kind == TokenKind.TrueToken) {
                    return 1;
                } else if (node.Token.Kind == TokenKind.FalseToken) {
                    return 0;
                } else if (node.Token.Kind == TokenKind.IdentifierToken) {
                    return GetBoolVarValue (node, node.AppearsInScope);
                } else if (node.Token.Kind == TokenKind.DigitToken) {
                    return int.Parse (node.Token.Text);
                } else {
                    return 0;
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
            node.Descendants[0].Visited = true;
            node.Descendants[1].Visited = true;
            if (EvaluateBooleanSubtree (node.Descendants[0]) == EvaluateBooleanSubtree (node.Descendants[1])) {
                return 1;
            } else {
                return 0;
            }
        }
        public int HandleNotEqual (ASTNode node) {
            node.Descendants[0].Visited = true;
            node.Descendants[1].Visited = true;
            if (EvaluateBooleanSubtree (node.Descendants[0]) != EvaluateBooleanSubtree (node.Descendants[1])) {
                return 1;
            } else {
                return 0;
            }
        }
        public int GetBoolVarValue (ASTNode variableNode, Scope variableScope) {
            int outInteger;
            int varScope = SemanticAnalyser.VariableChecker.FindSymbol (variableNode, variableScope);
            int.TryParse (StaticTemp.GetTempTableEntry (variableNode.Token.Text, varScope).Value, out outInteger);
            return outInteger;
        }
        public void HandleStaticVariables () {
            foreach (var entry in StaticTemp.Rows) {
                Image.BackPatch (entry, Image.GetCurrentAddress ());
            }
        }
        public void HandleJumps () {
            foreach (var entry in JumpTable.Jumps) {
                Image.BackPatchJump (entry);
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
        public void InsertStringInHeap (string value) {
            Image.WriteHeapByte ("00");
            foreach (char c in value.Reverse ()) {
                Image.WriteHeapByte (char.ConvertToUtf32 (c.ToString (), 0).ToString ("X2"));
            }
        }
    }
}
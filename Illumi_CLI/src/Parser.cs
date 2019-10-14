using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.Text;

namespace Illumi_CLI {
    class Parser {
        public Lexer Lexer { get; }

        private DiagnosticCollection _diagnostics { get; }

        public List<Token> TokenStream { get; set; }

        public Token currentToken { get; set; }

        public ConcreteSyntaxTree Tree { get; }

        public Session CurrentSession { get; set; }

        public int ErrorCount { get; set; }

        public int WarningCount { get; set; }

        public List<TokenKind> recoverySet = new List<TokenKind> () {
            TokenKind.IdentifierToken,
            TokenKind.Type_IntegerToken,
            TokenKind.Type_StringToken,
            TokenKind.Type_BooleanToken,
            TokenKind.IfToken,
            TokenKind.WhileToken,
            TokenKind.PrintToken,
            TokenKind.LeftBraceToken,
            TokenKind.RightBraceToken,
            TokenKind.EndOfProgramToken
        };

        public Parser (Lexer lexer, Session currentSession) {
            Lexer = lexer;
            Tree = new ConcreteSyntaxTree ();
            CurrentSession = currentSession;
            _diagnostics = new DiagnosticCollection ();
            ErrorCount = 0;
            WarningCount = 0;
        }

        public void Parse () {
            if (CurrentSession.debugMode) {
                _diagnostics.Parser_ReportParseBeginning ();
                _diagnostics.DisplayDiagnostics ();
            }
            TokenStream = Lexer.GetTokens ();
            if (TokenStream.Count != 0) {
                ParseProgram ();
                if (ErrorCount > 0) {
                    _diagnostics.Parser_ParseEndedWithErrors ();
                    _diagnostics.DisplayDiagnostics ();
                    Tree.Discard ();
                } else {
                    DisplayCST ();
                }

            } else {
                _diagnostics.Parser_ReportNoTokens ();
                _diagnostics.DisplayDiagnostics ();
                ErrorCount++;
            }
            _diagnostics.Parser_ReportEndOfParse ();
            _diagnostics.DisplayDiagnostics ();
            return;

        }

        public void ParseProgram () {
            if (CurrentSession.debugMode) {
                _diagnostics.Parser_EnteredParseStage ("program");
                _diagnostics.DisplayDiagnostics ();
            }

            AddBranchNode ("Program");
            Next ();
            ParseBlock ();
            MatchAndConsume (TokenKind.EndOfProgramToken);
            //Ascend ();
            return;
        }

        public void ParseBlock () {
            if (CurrentSession.debugMode) {
                _diagnostics.Parser_EnteredParseStage ("block");
                _diagnostics.DisplayDiagnostics ();
            }
            AddBranchNode ("Block");
            MatchAndConsume (TokenKind.LeftBraceToken);
            ParseStatementList ();
            MatchAndConsume (TokenKind.RightBraceToken);
            Ascend ();
            return;
        }

        public void ParseStatementList () {
            if (CurrentSession.debugMode) {
                _diagnostics.Parser_EnteredParseStage ("statement list");
                _diagnostics.DisplayDiagnostics ();
            }

            AddBranchNode ("StatementList");
            ParseStatement ();
            Next ();
            if (currentToken.Kind != TokenKind.RightBraceToken && currentToken.Kind != TokenKind.EndOfProgramToken) {
                ParseStatementList ();
            }
            Ascend ();
            return;
        }

        public void ParseStatement () {
            if (CurrentSession.debugMode) {
                _diagnostics.Parser_EnteredParseStage ("statement");
                _diagnostics.DisplayDiagnostics ();
            }
            AddBranchNode ("Statement");
            switch (currentToken.Kind) {
                case TokenKind.PrintToken:
                    ParsePrintStatement ();
                    break;
                case TokenKind.IdentifierToken:
                    ParseAssignmentStatement ();
                    break;
                case TokenKind.WhileToken:
                    ParseWhileStatement ();
                    break;
                case TokenKind.IfToken:
                    ParseIfStatement ();
                    break;
                case TokenKind.Type_IntegerToken:
                case TokenKind.Type_StringToken:
                case TokenKind.Type_BooleanToken:
                    ParseVariableDeclaration ();
                    break;
                case TokenKind.LeftBraceToken:
                    ParseBlock ();
                    break;
                case TokenKind.RightBraceToken:
                    // this is the case in which a block is empty / statment is null
                    break;
                case TokenKind.EndOfProgramToken:
                    break;
                default:
                    _diagnostics.Parser_ReportIncorrectStatement (currentToken);
                    ErrorCount++;
                    Panic ();
                    break;
            }
            Ascend ();
            return;
        }

        public void ParsePrintStatement () {
            if (CurrentSession.debugMode) {
                _diagnostics.Parser_EnteredParseStage ("print statement");
                _diagnostics.DisplayDiagnostics ();
            }
            AddBranchNode (TokenKind.PrintToken.ToString ());
            MatchAndConsume (TokenKind.PrintToken);
            MatchAndConsume (TokenKind.LeftParenthesisToken);
            ParseExpression ();
            MatchAndConsume (TokenKind.RightParenthesisToken);
            Ascend ();
            return;
        }

        public void ParseAssignmentStatement () {
            if (CurrentSession.debugMode) {
                _diagnostics.Parser_EnteredParseStage ("assignment statement");
                _diagnostics.DisplayDiagnostics ();
            }
            AddBranchNode (TokenKind.AssignmentToken.ToString ());
            ParseIdentifier ();
            MatchAndConsume (TokenKind.AssignmentToken);
            ParseExpression ();
            Ascend ();
            return;
        }

        public void ParseVariableDeclaration () {
            if (CurrentSession.debugMode) {
                _diagnostics.Parser_EnteredParseStage ("variable declaration");
                _diagnostics.DisplayDiagnostics ();
            }
            AddBranchNode ("VariableDeclaration");
            ParseTypeDefinition ();
            ParseIdentifier ();
            Ascend ();
            return;
        }

        public void ParseTypeDefinition () {
            if (CurrentSession.debugMode) {
                _diagnostics.Parser_EnteredParseStage ("type definition");
                _diagnostics.DisplayDiagnostics ();
            }
            AddBranchNode ("Type");
            switch (currentToken.Kind) {
                case TokenKind.Type_IntegerToken:
                    MatchAndConsume (TokenKind.Type_IntegerToken);
                    break;
                case TokenKind.Type_BooleanToken:
                    MatchAndConsume (TokenKind.Type_BooleanToken);
                    break;
                case TokenKind.Type_StringToken:
                    MatchAndConsume (TokenKind.Type_StringToken);
                    break;
                default:
                    break;
            }
            Ascend ();
            return;
        }

        public void ParseWhileStatement () {
            if (CurrentSession.debugMode) {
                _diagnostics.Parser_EnteredParseStage ("while statement");
                _diagnostics.DisplayDiagnostics ();
            }
            AddBranchNode (TokenKind.WhileToken.ToString ());
            MatchAndConsume (TokenKind.WhileToken);
            ParseExpression ();
            Ascend ();
            return;
        }

        public void ParseIfStatement () {
            if (CurrentSession.debugMode) {
                _diagnostics.Parser_EnteredParseStage ("if statement");
                _diagnostics.DisplayDiagnostics ();
            }
            AddBranchNode (TokenKind.IfToken.ToString ());
            MatchAndConsume (TokenKind.IfToken);
            ParseExpression ();
            Ascend ();
            return;
        }

        public void ParseExpression () {
            if (CurrentSession.debugMode) {
                _diagnostics.Parser_EnteredParseStage ("expression");
                _diagnostics.DisplayDiagnostics ();
            }
            AddBranchNode ("Expression");
            switch (currentToken.Kind) {
                case TokenKind.IdentifierToken:
                    ParseIdentifier ();
                    break;
                case TokenKind.DigitToken:
                    ParseIntExpression ();
                    break;
                case TokenKind.StringToken:
                    ParseStringExpression ();
                    break;
                case TokenKind.LeftParenthesisToken:
                    ParseBooleanExpression ();
                    break;
                case TokenKind.TrueToken:
                case TokenKind.FalseToken:
                    ParseBooleanExpression ();
                    break;
                default:
                    break;
            }
            Ascend ();
            return;
        }

        public void ParseIntExpression () {
            if (CurrentSession.debugMode) {
                _diagnostics.Parser_EnteredParseStage ("integer expression");
                _diagnostics.DisplayDiagnostics ();
            }
            AddBranchNode (TokenKind.DigitToken.ToString ());
            MatchAndConsume (TokenKind.DigitToken);
            if (currentToken.Kind == TokenKind.AdditionToken) {
                MatchAndConsume (TokenKind.AdditionToken);
                ParseExpression ();
            }
            Ascend ();
            return;
        }

        public void ParseStringExpression () {
            if (CurrentSession.debugMode) {
                _diagnostics.Parser_EnteredParseStage ("string expression");
                _diagnostics.DisplayDiagnostics ();
            }
            AddBranchNode (TokenKind.StringToken.ToString ());
            MatchAndConsume (TokenKind.StringToken);
            Ascend ();
            return;
        }

        public void ParseBooleanExpression () {
            if (CurrentSession.debugMode) {
                _diagnostics.Parser_EnteredParseStage ("boolean expression");
                _diagnostics.DisplayDiagnostics ();
            }
            AddBranchNode (TokenKind.Type_BooleanToken.ToString ());
            if (currentToken.Kind == TokenKind.LeftParenthesisToken) {
                MatchAndConsume (TokenKind.LeftParenthesisToken);
                ParseExpression ();
                ParseBooleanOperator ();
                ParseExpression ();
                MatchAndConsume (TokenKind.RightParenthesisToken);
            } else {
                switch (currentToken.Kind) {
                    case TokenKind.TrueToken:
                        MatchAndConsume (TokenKind.TrueToken);
                        break;
                    case TokenKind.FalseToken:
                        MatchAndConsume (TokenKind.FalseToken);
                        break;
                    default:
                        break;
                }

                switch (currentToken.Kind) {
                    case TokenKind.EquivalenceToken:
                    case TokenKind.NotEqualToken:
                        ParseBooleanOperator ();
                        ParseExpression ();
                        break;
                    default:
                        break;
                }
            }
            Ascend ();
            return;
        }

        public void ParseBooleanOperator () {
            if (CurrentSession.debugMode) {
                _diagnostics.Parser_EnteredParseStage ("boolean operator");
                _diagnostics.DisplayDiagnostics ();
            }
            switch (currentToken.Kind) {
                case TokenKind.EquivalenceToken:
                    AddBranchNode (TokenKind.EquivalenceToken.ToString ());
                    MatchAndConsume (TokenKind.EquivalenceToken);
                    break;
                case TokenKind.NotEqualToken:
                    AddBranchNode (TokenKind.NotEqualToken.ToString ());
                    MatchAndConsume (TokenKind.NotEqualToken);
                    break;
                default:
                    break;
            }
            Ascend ();
            return;
        }

        public void ParseIdentifier () {
            if (CurrentSession.debugMode) {
                _diagnostics.Parser_EnteredParseStage ("identifier");
                _diagnostics.DisplayDiagnostics ();
            }
            AddBranchNode ("Identifier");
            MatchAndConsume (TokenKind.IdentifierToken);
            Ascend ();
            return;
        }

        public bool MatchAndConsume (TokenKind expectedKind) {
            if (CurrentSession.debugMode) {
                _diagnostics.Parser_ReportMatchingToken (expectedKind);
                _diagnostics.DisplayDiagnostics ();
            }
            bool success = false;
            if (currentToken.Kind == expectedKind) {
                success = ConsumeToken ();
            } else {
                _diagnostics.Parser_ReportUnexpectedToken (currentToken, expectedKind);
                _diagnostics.DisplayDiagnostics ();
            }
            return success;
        }

        public bool ConsumeToken () {
            if (TokenStream.Count >= 0) {
                if (CurrentSession.debugMode) {
                    _diagnostics.Parser_ReportConsumingToken (currentToken);
                    _diagnostics.DisplayDiagnostics ();
                }
                TokenStream.RemoveAt (0);
                AddLeafNode (currentToken);
                Next ();
                return true;
            } else {
                _diagnostics.Parser_ReportNoRemainingTokens ();
            }
            return false;
        }

        public void Next () {
            if (TokenStream.Count > 0) {
                currentToken = TokenStream.First ();
            }
        }

        public void Panic () {
            System.Console.WriteLine ("[ERROR] - [Parser] -> Entering panic recovery mode.");
            while (!recoverySet.Contains (currentToken.Kind)) {
                _diagnostics.Parser_ReportPanickedToken (currentToken);
                _diagnostics.DisplayDiagnostics ();
                TokenStream.RemoveAt (0);
                Next ();
            }
            System.Console.WriteLine ("[ERROR] - [Parser] -> Leaving panic recovery mode.");
            return;
        }
        public void Ascend () {
            Tree.Ascend (CurrentSession);
        }

        public void AddLeafNode (Token token) {
            TreeNode node = new TreeNode (null, null, true, token, token.Kind.ToString ());
            Tree.AddLeafNode (node);
            return;
        }

        public void AddBranchNode (string type) {
            TreeNode node = new TreeNode (null, null, false, null, type);
            Tree.AddBranchNode (node);
            return;
        }

        public void DisplayCST () {
            if (CurrentSession.debugMode) {
                System.Console.WriteLine ("[Debug] - [Parser] -> Displaying CST.");
                Tree.DisplayCST ();
            } else {
                System.Console.Write ("Would you like to view the concrete syntax tree? (Y/N): ");
                string input = Console.ReadLine ();
                switch (input.ToLower ()) {
                    case "y":
                    case "yes":
                    case "ok":
                    case "okay":
                    case "yeah":
                        Tree.DisplayCST ();
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.Text;

namespace Illumi_CLI {
    class Parser {
        public Lexer Lexer { get; }

        public DiagnosticCollection diagnostics { get; }

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
            diagnostics = new DiagnosticCollection ();
            ErrorCount = 0;
            WarningCount = 0;
        }

        public void Parse () {
            if (CurrentSession.debugMode) {
                diagnostics.Parser_ReportParseBeginning ();
            }
            TokenStream = Lexer.GetTokens ();
            if (TokenStream.Count != 0) {
                ParseProgram ();
                if (diagnostics.ErrorCount > 0) {
                    diagnostics.Parser_ParseEndedWithErrors ();
                    Tree.Discard ();
                } else {
                    DisplayCST ();
                }

            } else {
                diagnostics.Parser_ReportNoTokens ();
                ErrorCount++;
            }
            return;

        }

        public void ParseProgram () {
            if (CurrentSession.debugMode) {
                diagnostics.Parser_EnteredParseStage ("program");
            }

            AddBranchNode ("Program");
            Next ();
            ParseBlock ();
            MatchAndConsume (TokenKind.EndOfProgramToken);
            return;
        }

        public void ParseBlock () {
            if (CurrentSession.debugMode) {
                diagnostics.Parser_EnteredParseStage ("block");
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
                diagnostics.Parser_EnteredParseStage ("statement list");
            }

            AddBranchNode ("Statement List");
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
                diagnostics.Parser_EnteredParseStage ("statement");
            }

            switch (currentToken.Kind) {
                case TokenKind.PrintToken:
                    AddBranchNode ("Statement");
                    ParsePrintStatement ();
                    break;
                case TokenKind.IdentifierToken:
                    AddBranchNode ("Statement");
                    ParseAssignmentStatement ();
                    break;
                case TokenKind.WhileToken:
                    AddBranchNode ("Statement");
                    ParseWhileStatement ();
                    break;
                case TokenKind.IfToken:
                    AddBranchNode ("Statement");
                    ParseIfStatement ();
                    break;
                case TokenKind.Type_IntegerToken:
                case TokenKind.Type_StringToken:
                case TokenKind.Type_BooleanToken:
                    AddBranchNode ("Statement");
                    ParseVariableDeclaration ();
                    break;
                case TokenKind.LeftBraceToken:
                    AddBranchNode ("Statement");
                    ParseBlock ();
                    break;
                case TokenKind.RightBraceToken:
                    // this is the case in which a block is empty / statment is null
                    return;
                case TokenKind.EndOfProgramToken:
                    break;
                default:
                    diagnostics.Parser_ReportIncorrectStatement (currentToken);
                    ErrorCount++;
                    Panic ();
                    break;
            }
            Ascend ();
            return;
        }

        public void ParsePrintStatement () {
            if (CurrentSession.debugMode) {
                diagnostics.Parser_EnteredParseStage ("print statement");
            }
            AddBranchNode ("Print Statement");
            MatchAndConsume (TokenKind.PrintToken);
            MatchAndConsume (TokenKind.LeftParenthesisToken);
            ParseExpression ();
            MatchAndConsume (TokenKind.RightParenthesisToken);
            Ascend ();
            return;
        }

        public void ParseAssignmentStatement () {
            if (CurrentSession.debugMode) {
                diagnostics.Parser_EnteredParseStage ("assignment statement");
            }
            AddBranchNode ("Assignment Statement");
            ParseIdentifier ();
            MatchAndConsume (TokenKind.AssignmentToken);
            ParseExpression ();
            Ascend ();
            return;
        }

        public void ParseVariableDeclaration () {
            if (CurrentSession.debugMode) {
                diagnostics.Parser_EnteredParseStage ("variable declaration");
            }
            AddBranchNode ("VariableDeclaration");
            ParseTypeDefinition ();
            ParseIdentifier ();
            Ascend ();
            return;
        }

        public void ParseTypeDefinition () {
            if (CurrentSession.debugMode) {
                diagnostics.Parser_EnteredParseStage ("type definition");
            }

            switch (currentToken.Kind) {
                case TokenKind.Type_IntegerToken:
                    AddBranchNode ("Type");
                    MatchAndConsume (TokenKind.Type_IntegerToken);
                    break;
                case TokenKind.Type_BooleanToken:
                    AddBranchNode ("Type");
                    MatchAndConsume (TokenKind.Type_BooleanToken);
                    break;
                case TokenKind.Type_StringToken:
                    AddBranchNode ("Type");
                    MatchAndConsume (TokenKind.Type_StringToken);
                    break;
                default:
                    return;
            }
            Ascend ();
            return;
        }

        public void ParseWhileStatement () {
            if (CurrentSession.debugMode) {
                diagnostics.Parser_EnteredParseStage ("while statement");
            }
            AddBranchNode (TokenKind.WhileToken.ToString ());
            MatchAndConsume (TokenKind.WhileToken);
            MatchAndConsume (TokenKind.LeftParenthesisToken);
            ParseExpression ();
            MatchAndConsume (TokenKind.RightParenthesisToken);
            Ascend ();
            return;
        }

        public void ParseIfStatement () {
            if (CurrentSession.debugMode) {
                diagnostics.Parser_EnteredParseStage ("if statement");
            }
            AddBranchNode (TokenKind.IfToken.ToString ());
            MatchAndConsume (TokenKind.IfToken);
            MatchAndConsume (TokenKind.LeftParenthesisToken);
            ParseExpression ();
            MatchAndConsume (TokenKind.RightParenthesisToken);
            Ascend ();
            return;
        }

        public void ParseExpression () {
            if (CurrentSession.debugMode) {
                diagnostics.Parser_EnteredParseStage ("expression");
            }
            switch (currentToken.Kind) {
                case TokenKind.IdentifierToken:
                    AddBranchNode ("Expression");
                    ParseIdentifier ();
                    break;
                case TokenKind.DigitToken:
                    AddBranchNode ("Expression");
                    ParseIntExpression ();
                    break;
                case TokenKind.StringToken:
                    AddBranchNode ("Expression");
                    ParseStringExpression ();
                    break;
                case TokenKind.LeftParenthesisToken:
                    AddBranchNode ("Expression");
                    ParseBooleanExpression ();
                    break;
                case TokenKind.TrueToken:
                case TokenKind.FalseToken:
                    AddBranchNode ("Expression");
                    ParseBooleanExpression ();
                    break;
                default:
                    diagnostics.Parser_ReportMissingExpression (currentToken);
                    return;
            }
            Ascend ();
            return;
        }

        public void ParseIntExpression () {
            if (CurrentSession.debugMode) {
                diagnostics.Parser_EnteredParseStage ("integer expression");
            }
            AddBranchNode ("Integer Expression");
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
                diagnostics.Parser_EnteredParseStage ("string expression");
            }
            AddBranchNode (TokenKind.StringToken.ToString ());
            MatchAndConsume (TokenKind.StringToken);
            Ascend ();
            return;
        }

        public void ParseBooleanExpression () {
            if (CurrentSession.debugMode) {
                diagnostics.Parser_EnteredParseStage ("boolean expression");
            }

            if (currentToken.Kind == TokenKind.LeftParenthesisToken) {
                AddBranchNode (TokenKind.Type_BooleanToken.ToString ());
                MatchAndConsume (TokenKind.LeftParenthesisToken);
                ParseExpression ();
                ParseBooleanOperator ();
                ParseExpression ();
                MatchAndConsume (TokenKind.RightParenthesisToken);
            } else {
                switch (currentToken.Kind) {
                    case TokenKind.TrueToken:
                        AddBranchNode (TokenKind.Type_BooleanToken.ToString ());
                        MatchAndConsume (TokenKind.TrueToken);
                        break;
                    case TokenKind.FalseToken:
                        AddBranchNode (TokenKind.Type_BooleanToken.ToString ());
                        MatchAndConsume (TokenKind.FalseToken);
                        break;
                    default:
                        return;
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
                diagnostics.Parser_EnteredParseStage ("boolean operator");
            }
            switch (currentToken.Kind) {
                case TokenKind.EquivalenceToken:
                    AddBranchNode ("Boolean Operator");
                    MatchAndConsume (TokenKind.EquivalenceToken);
                    break;
                case TokenKind.NotEqualToken:
                    AddBranchNode ("Boolean Operator");
                    MatchAndConsume (TokenKind.NotEqualToken);
                    break;
                default:
                    return;
            }
            Ascend ();
            return;
        }

        public void ParseIdentifier () {
            if (CurrentSession.debugMode) {
                diagnostics.Parser_EnteredParseStage ("identifier");
            }
            AddBranchNode ("Identifier");
            MatchAndConsume (TokenKind.IdentifierToken);
            Ascend ();
            return;
        }

        public bool MatchAndConsume (TokenKind expectedKind) {
            if (CurrentSession.debugMode) {
                diagnostics.Parser_ReportMatchingToken (expectedKind);
            }
            bool success = false;
            if (currentToken.Kind == expectedKind) {
                success = ConsumeToken ();
            } else {
                diagnostics.Parser_ReportUnexpectedToken (currentToken, expectedKind);
            }
            return success;
        }

        public bool ConsumeToken () {
            if (TokenStream.Count >= 0) {
                if (CurrentSession.debugMode) {
                    diagnostics.Parser_ReportConsumingToken (currentToken);
                }
                TokenStream.RemoveAt (0);
                AddLeafNode (currentToken);
                Next ();
                return true;
            } else {
                diagnostics.Parser_ReportNoRemainingTokens ();
            }
            return false;
        }

        public void Next () {
            if (TokenStream.Count > 0) {
                currentToken = TokenStream.First ();
            }
        }

        public void Panic () {
            while (!recoverySet.Contains (currentToken.Kind)) {
                diagnostics.Parser_ReportPanickedToken (currentToken);
                TokenStream.RemoveAt (0);
                Next ();
            }
            diagnostics.Parser_ReportExitedPanicMode ();
            return;
        }
        public void Ascend () {
            if (Tree.currentNode.Parent != null) {
                switch (Tree.currentNode.Parent.Type) {
                    case "Program":
                        if (currentToken.Kind == TokenKind.EndOfProgramToken) {
                            Tree.Ascend (CurrentSession);
                        }
                        break;
                    default:
                        Tree.Ascend (CurrentSession);
                        break;
                }
            }
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
                        return;
                    default:
                        return;
                }
            }
        }
    }
}
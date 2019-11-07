using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.Text;

namespace Illumi_CLI {
    class Parser {
        public Lexer Lexer { get; }

        public DiagnosticCollection Diagnostics { get; }

        public List<Token> TokenStream { get; set; }

        public Token currentToken { get => TokenStream[tokenCounter]; }

        public int tokenCounter { get; set; }

        public ConcreteSyntaxTree Tree { get; set; }

        public Session CurrentSession { get; set; }

        public Boolean Failed { get; set; }

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

        public Parser (Lexer lexer, Session currentSession, DiagnosticCollection diagnostics) {
            Lexer = lexer;
            Tree = new ConcreteSyntaxTree ();
            CurrentSession = currentSession;
            Diagnostics = diagnostics;
        }

        public void Parse () {
            TokenStream = Lexer.GetTokens ();
            tokenCounter = 0;

            if (TokenStream.Count != 0) {
                ParseProgram ();
                if (Diagnostics.ErrorCount > 0) {
                    Failed = true;
                    Diagnostics.Parser_ParseEndedWithErrors ();
                    Tree.Discard ();
                } else {
                    DisplayCST ();
                }
            } else {
                Diagnostics.Parser_ReportNoTokens ();
                Failed = true;
                Diagnostics.Parser_ParseEndedWithErrors ();
            }

            return;
        }

        public void ParseProgram () {
            if (CurrentSession.debugMode) {
                Diagnostics.Parser_EnteredParseStage ("program");
            }

            AddBranchNode ("Program");
            ParseBlock ();
            MatchAndConsume (TokenKind.EndOfProgramToken);
            return;
        }

        public void ParseBlock () {
            if (CurrentSession.debugMode) {
                Diagnostics.Parser_EnteredParseStage ("block");
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
                Diagnostics.Parser_EnteredParseStage ("statement list");
            }

            AddBranchNode ("StatementList");
            ParseStatement ();
            //Next ();
            if (currentToken.Kind != TokenKind.RightBraceToken && currentToken.Kind != TokenKind.EndOfProgramToken) {
                ParseStatementList ();
            }
            Ascend ();
            return;
        }

        public void ParseStatement () {
            if (CurrentSession.debugMode) {
                Diagnostics.Parser_EnteredParseStage ("statement");
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
                case TokenKind.LeftParenthesisToken:
                    AddBranchNode ("BooleanExpression");
                    ParseBooleanExpression ();
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
                    Diagnostics.Parser_ReportIncorrectStatement (currentToken);
                    Panic ();
                    break;
            }
            Ascend ();
            return;
        }

        public void ParsePrintStatement () {
            if (CurrentSession.debugMode) {
                Diagnostics.Parser_EnteredParseStage ("print statement");
            }
            AddBranchNode ("PrintStatement");
            MatchAndConsume (TokenKind.PrintToken);
            MatchAndConsume (TokenKind.LeftParenthesisToken);
            ParseExpression ();
            MatchAndConsume (TokenKind.RightParenthesisToken);
            Ascend ();
            return;
        }

        public void ParseAssignmentStatement () {
            if (CurrentSession.debugMode) {
                Diagnostics.Parser_EnteredParseStage ("assignment statement");
            }
            AddBranchNode ("AssignmentStatement");
            ParseIdentifier ();
            MatchAndConsume (TokenKind.AssignmentToken);
            ParseExpression ();
            Ascend ();
            return;
        }

        public void ParseVariableDeclaration () {
            if (CurrentSession.debugMode) {
                Diagnostics.Parser_EnteredParseStage ("variable declaration");
            }
            AddBranchNode ("VariableDeclaration");
            ParseTypeDefinition ();
            ParseIdentifier ();
            Ascend ();
            return;
        }

        public void ParseTypeDefinition () {
            if (CurrentSession.debugMode) {
                Diagnostics.Parser_EnteredParseStage ("type definition");
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
                Diagnostics.Parser_EnteredParseStage ("while statement");
            }
            AddBranchNode ("WhileStatement");
            MatchAndConsume (TokenKind.WhileToken);
            //MatchAndConsume (TokenKind.LeftParenthesisToken);
            ParseBooleanExpression ();
            //MatchAndConsume (TokenKind.RightParenthesisToken);
            ParseBlock ();
            Ascend ();
            return;
        }

        public void ParseIfStatement () {
            if (CurrentSession.debugMode) {
                Diagnostics.Parser_EnteredParseStage ("if statement");
            }
            AddBranchNode ("IfStatement");
            MatchAndConsume (TokenKind.IfToken);
            //MatchAndConsume (TokenKind.LeftParenthesisToken);
            ParseBooleanExpression ();
            //MatchAndConsume (TokenKind.RightParenthesisToken);
            ParseBlock ();
            Ascend ();
            return;
        }

        public void ParseExpression () {
            if (CurrentSession.debugMode) {
                Diagnostics.Parser_EnteredParseStage ("expression");
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
                    Diagnostics.Parser_ReportMissingExpression (currentToken);
                    return;
            }
            Ascend ();
            return;
        }

        public void ParseIntExpression () {
            if (CurrentSession.debugMode) {
                Diagnostics.Parser_EnteredParseStage ("integer expression");
            }
            AddBranchNode ("IntegerExpression");
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
                Diagnostics.Parser_EnteredParseStage ("string expression");
            }
            AddBranchNode ("StringExpression");
            MatchAndConsume (TokenKind.StringToken);
            Ascend ();
            return;
        }

        public void ParseBooleanExpression () {
            if (CurrentSession.debugMode) {
                Diagnostics.Parser_EnteredParseStage ("boolean expression");
            }
            AddBranchNode ("BooleanExpression");

            if (currentToken.Kind == TokenKind.LeftParenthesisToken) {
                MatchAndConsume (TokenKind.LeftParenthesisToken);
                ParseExpression ();
                if (currentToken.Kind == TokenKind.EquivalenceToken || currentToken.Kind == TokenKind.NotEqualToken) {
                    ParseBooleanOperator ();
                    ParseExpression ();
                }
                MatchAndConsume (TokenKind.RightParenthesisToken);
            } else {
                switch (currentToken.Kind) {
                    case TokenKind.TrueToken:
                        MatchAndConsume (TokenKind.TrueToken);
                        break;
                    case TokenKind.FalseToken:
                        MatchAndConsume (TokenKind.FalseToken);
                        break;
                    case TokenKind.IdentifierToken:
                        ParseIdentifier ();
                        break;
                    default:
                        return;
                }
            }
            Ascend ();
            return;
        }

        public void ParseBooleanOperator () {
            if (CurrentSession.debugMode) {
                Diagnostics.Parser_EnteredParseStage ("boolean operator");
            }
            switch (currentToken.Kind) {
                case TokenKind.EquivalenceToken:
                    AddBranchNode ("BooleanOperator");
                    MatchAndConsume (TokenKind.EquivalenceToken);
                    break;
                case TokenKind.NotEqualToken:
                    AddBranchNode ("BooleanOperator");
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
                Diagnostics.Parser_EnteredParseStage ("identifier");
            }
            AddBranchNode ("Identifier");
            MatchAndConsume (TokenKind.IdentifierToken);
            Ascend ();
            return;
        }

        public bool MatchAndConsume (TokenKind expectedKind) {
            if (CurrentSession.debugMode) {
                Diagnostics.Parser_ReportMatchingToken (expectedKind);
            }
            bool success = false;
            if (currentToken.Kind == expectedKind) {
                success = ConsumeToken ();
            } else {
                Diagnostics.Parser_ReportUnexpectedToken (currentToken, expectedKind);
            }
            return success;
        }

        public bool ConsumeToken () {
            if (TokenStream.Count >= 0) {
                if (CurrentSession.debugMode) {
                    Diagnostics.Parser_ReportConsumingToken (currentToken);
                }
                //TokenStream.RemoveAt (0);
                AddLeafNode (currentToken);
                Next ();
                return true;
            } else {
                Diagnostics.Parser_ReportNoRemainingTokens ();
            }
            return false;
        }

        public void Next () {
            if (tokenCounter != TokenStream.Count) {
                tokenCounter++;
            }
            // if (TokenStream.Count > 0) {
            //     currentToken = TokenStream.First ();
            // }
        }

        public void Panic () {
            while (!recoverySet.Contains (currentToken.Kind)) {
                Diagnostics.Parser_ReportPanickedToken (currentToken);
                //TokenStream.RemoveAt (0);
                Next ();
            }
            Diagnostics.Parser_ReportExitedPanicMode ();
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
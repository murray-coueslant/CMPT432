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

        public List<TokenKind> recoverySet = new List<TokenKind> () {
            TokenKind.IdentifierToken,
            TokenKind.Type_IntegerToken,
            TokenKind.Type_StringToken,
            TokenKind.Type_BooleanToken,
            TokenKind.IfToken,
            TokenKind.WhileToken,
            TokenKind.PrintToken,
            TokenKind.LeftBraceToken,
            TokenKind.EndOfProgramToken
        };

        public Parser (Lexer lexer) {
            Lexer = lexer;
            Tree = new ConcreteSyntaxTree ();
            _diagnostics = new DiagnosticCollection ();
        }

        public void Parse () {
            System.Console.WriteLine ("Beginning parse");
            TokenStream = Lexer.GetTokens ();
            if (TokenStream.Count != 0) {
                ParseProgram ();
                _diagnostics.DisplayDiagnostics ();
                DisplayCST ();
                return;
            } else {
                //_diagnostics.Parser_ReportNoTokens();
                _diagnostics.DisplayDiagnostics ();
                return;
            }

        }

        public void ParseProgram () {
            System.Console.WriteLine ("Entered parse program.");
            Next ();
            ParseBlock ();
            MatchAndConsume (TokenKind.EndOfProgramToken);
            Ascend ();
            return;
        }

        public void ParseBlock () {
            System.Console.WriteLine ("Entered parse block.");
            MatchAndConsume (TokenKind.LeftBraceToken);
            ParseStatementList ();
            MatchAndConsume (TokenKind.RightBraceToken);
            Ascend ();
            return;
        }

        public void ParseStatementList () {
            Console.WriteLine ("Entered parse statement list.");
            ParseStatement ();
            Next ();
            if (currentToken.Kind != TokenKind.RightBraceToken && currentToken.Kind != TokenKind.EndOfProgramToken) {
                ParseStatementList ();
            }
            Ascend ();
            return;
        }

        public void ParseStatement () {
            Console.WriteLine ("Entered parse statement.");
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
                    //_diagnostics.Parser_ReportIncorrectStatement (currentToken);
                    System.Console.WriteLine ("Incorrect statement encountered, entering panic recovery mode.");
                    Panic ();
                    break;
            }
            Ascend ();
            return;
        }

        public void ParsePrintStatement () {
            Console.WriteLine ("Entered parse print statement.");
            MatchAndConsume (TokenKind.PrintToken);
            MatchAndConsume (TokenKind.LeftParenthesisToken);
            ParseExpression ();
            MatchAndConsume (TokenKind.RightParenthesisToken);
            Ascend ();
            return;
        }

        public void ParseAssignmentStatement () {
            Console.WriteLine ("Entered parse assignment statement.");
            ParseIdentifier ();
            MatchAndConsume (TokenKind.AssignmentToken);
            ParseExpression ();
            Ascend ();
            return;
        }

        public void ParseVariableDeclaration () {
            Console.WriteLine ("Entered parse variable declaration.");
            ParseTypeDefinition ();
            ParseIdentifier ();
            Ascend ();
            return;
        }

        public void ParseTypeDefinition () {
            System.Console.WriteLine ("Entered parse type definition.");
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
            Console.WriteLine ("Entered parse while statement.");
            MatchAndConsume (TokenKind.WhileToken);
            ParseExpression ();
            Ascend ();
            return;
        }

        public void ParseIfStatement () {
            Console.WriteLine ("Entered parse if statement.");
            MatchAndConsume (TokenKind.IfToken);
            ParseExpression ();
            Ascend ();
            return;
        }

        public void ParseExpression () {
            Console.WriteLine ("Entered parse expression.");
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
            Console.WriteLine ("Entered parse int expression.");
            MatchAndConsume (TokenKind.DigitToken);
            if (currentToken.Kind == TokenKind.AdditionToken) {
                MatchAndConsume (TokenKind.AdditionToken);
                ParseExpression ();
            }
            Ascend ();
            return;
        }

        public void ParseStringExpression () {
            Console.WriteLine ("Entered parse string expression.");
            MatchAndConsume (TokenKind.StringToken);
            Ascend ();
            return;
        }

        public void ParseBooleanExpression () {
            Console.WriteLine ("Entered parse boolean expression.");
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
            Console.WriteLine ("Entered parse bool op.");
            switch (currentToken.Kind) {
                case TokenKind.EquivalenceToken:
                    MatchAndConsume (TokenKind.EquivalenceToken);
                    break;
                case TokenKind.NotEqualToken:
                    MatchAndConsume (TokenKind.NotEqualToken);
                    break;
                default:
                    break;
            }
            Ascend ();
            return;
        }

        public void ParseIdentifier () {
            Console.WriteLine ("Entered parse identifier.");
            MatchAndConsume (TokenKind.IdentifierToken);
            Ascend ();
            return;
        }

        public bool MatchAndConsume (TokenKind expectedKind) {
            System.Console.WriteLine ($"Matching token {expectedKind}.");
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
                System.Console.WriteLine ($"Consuming token {currentToken.Kind} [{currentToken.Text}].");
                TokenStream.RemoveAt (0);
                Next ();
                return true;
            } else {
                //_diagnostics.Parser_ReportNoRemainingTokens();
            }
            return false;
        }

        public void Next () {
            if (TokenStream.Count > 0) {
                currentToken = TokenStream.First ();
            }
        }

        public void Panic () {
            System.Console.WriteLine ("Entering panic recovery mode.");
            while (!recoverySet.Contains (currentToken.Kind)) {
                _diagnostics.Parser_ReportPanickedToken (currentToken);
                _diagnostics.DisplayDiagnostics ();
                ConsumeToken ();
            }
            System.Console.WriteLine ("Leaving panic recovery mode.");
            return;
        }
        public void Ascend () {
            System.Console.WriteLine ("Ascending tree");
        }

        public void DisplayCST () {
            System.Console.WriteLine ("Displaying CST.");
        }
    }
}
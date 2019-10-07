using System.Text.RegularExpressions;
using System;
using Microsoft.CodeAnalysis.Text;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Illumi_CLI
{
    class Parser
    {
        public Lexer Lexer { get; }

        private DiagnosticCollection _diagnostics { get; }

        public List<Token> TokenStream { get; set; }

        public Token currentToken { get; set; }

        public Parser(Lexer lexer)
        {
            Lexer = lexer;
            _diagnostics = new DiagnosticCollection();
        }

        public void Parse()
        {
            System.Console.WriteLine("Beginning parse");
            TokenStream = Lexer.GetTokens();
            Console.WriteLine(TokenStream.Count);
            if (TokenStream.Count != 0)
            {
                ParseProgram();
                _diagnostics.DisplayDiagnostics();
                DisplayCST();
                return;
            }
            else
            {
                //_diagnostics.Parser_ReportNoTokens();
                _diagnostics.DisplayDiagnostics();
                return;
            }

        }

        public void ParseProgram()
        {
            System.Console.WriteLine("Entered parse program.");
            Next();
            ParseBlock();
            MatchAndConsume(TokenKind.EndOfProgramToken);
            Ascend();
            return;
        }

        public void ParseBlock()
        {
            System.Console.WriteLine("Entered parse block.");
            MatchAndConsume(TokenKind.LeftBraceToken);
            ParseStatementList();
            MatchAndConsume(TokenKind.RightBraceToken);
            Ascend();
            return;
        }

        public void ParseStatementList()
        {
            Console.WriteLine("Entered parse statement list.");
            ParseStatement();
            Ascend();
            return;
        }

        public void ParseStatement()
        {
            Console.WriteLine("Entered parse statement.");
            switch (currentToken.Kind)
            {
                case TokenKind.PrintToken:
                    ParsePrintStatement();
                    break;
                case TokenKind.WhileToken:
                    ParseWhileStatement();
                    break;
                case TokenKind.IfToken:
                    ParseIfStatement();
                    break;
                case TokenKind.Type_IntegerToken:
                case TokenKind.Type_StringToken:
                case TokenKind.Type_BooleanToken:
                    ParseVariableDeclaration();
                    break;
                case TokenKind.LeftBraceToken:
                    ParseBlock();
                    break;
                case TokenKind.IdentifierToken:
                    ParseAssignmentStatement();
                    break;
                default:
                    break;
            }
            Ascend();
            return;
        }

        public void ParsePrintStatement()
        {
            Console.WriteLine("Entered parse print statement.");
            MatchAndConsume(TokenKind.PrintToken);
            ParseParenthesisedExpression();
            Ascend();
            return;
        }

        public void ParseAssignmentStatement()
        {
            Console.WriteLine("Entered parse assignment statement.");
            ParseIdentifier();
            MatchAndConsume(TokenKind.AssignmentToken);
            ParseExpression();
            Ascend();
            return;
        }

        public void ParseVariableDeclaration()
        {
            Console.WriteLine("Entered parse variable declaration.");
            ParseTypeDefinition();
            ParseIdentifier();
            Ascend();
            return;
        }

        public void ParseTypeDefinition()
        {
            switch (currentToken.Kind)
            {
                case TokenKind.Type_IntegerToken:
                    MatchAndConsume(TokenKind.Type_IntegerToken);
                    break;
                case TokenKind.Type_BooleanToken:
                    MatchAndConsume(TokenKind.Type_BooleanToken);
                    break;
                case TokenKind.Type_StringToken:
                    MatchAndConsume(TokenKind.Type_StringToken);
                    break;
                default:
                    break;
            }
            Ascend();
            return;
        }

        public void ParseWhileStatement()
        {
            Console.WriteLine("Entered parse while statement.");
            MatchAndConsume(TokenKind.WhileToken);
            ParseParenthesisedExpression();
            Ascend();
            return;
        }

        public void ParseIfStatement()
        {
            Console.WriteLine("Entered parse if statement.");
            MatchAndConsume(TokenKind.IfToken);
            ParseParenthesisedExpression();
            Ascend();
            return;
        }

        public void ParseExpression()
        {
            Console.WriteLine("Entered parse expression.");
            switch (currentToken.Kind)
            {
                case TokenKind.DigitToken:
                    ParseIntExpression();
                    break;
                case TokenKind.StringToken:
                    ParseStringExpression();
                    break;
                case TokenKind.TrueToken:
                case TokenKind.FalseToken:
                    ParseBooleanExpression();
                    break;
                default:
                    break;
            }
            Ascend();
            return;
        }

        public void ParseParenthesisedExpression()
        {
            MatchAndConsume(TokenKind.LeftParenthesisToken);
            ParseExpression();
            MatchAndConsume(TokenKind.RightParenthesisToken);
            Ascend();
            return;
        }

        public void ParseIntExpression()
        {
            Console.WriteLine("Entered parse int expression.");
            MatchAndConsume(TokenKind.DigitToken);
            if (currentToken.Kind == TokenKind.AdditionToken)
            {
                MatchAndConsume(TokenKind.AdditionToken);
                ParseExpression();
            }
            Ascend();
            return;
        }

        public void ParseStringExpression()
        {
            Console.WriteLine("Entered parse string expression.");
            MatchAndConsume(TokenKind.StringToken);
            Ascend();
            return;
        }

        public void ParseBooleanExpression()
        {
            Console.WriteLine("Entered parse boolean expression.");
            if (currentToken.Kind == TokenKind.LeftParenthesisToken)
            {
                ParseParenthesisedBooleanExpression();
            }
            else
            {
                switch (currentToken.Kind)
                {
                    case TokenKind.TrueToken:
                        MatchAndConsume(TokenKind.TrueToken);
                        break;
                    case TokenKind.FalseToken:
                        MatchAndConsume(TokenKind.FalseToken);
                        break;
                    default:
                        break;
                }
            }
            Ascend();
            return;
        }

        public void ParseParenthesisedBooleanExpression()
        {
            Console.WriteLine("Entered parse paren. boolean expr.");
            MatchAndConsume(TokenKind.LeftParenthesisToken);
            ParseExpression();
            ParseBooleanOperator();
            ParseExpression();
            MatchAndConsume(TokenKind.RightParenthesisToken);
            Ascend();
            return;
        }

        public void ParseBooleanOperator()
        {
            Console.WriteLine("Entered parse bool op.");
            switch (currentToken.Kind)
            {
                case TokenKind.EquivalenceToken:
                    MatchAndConsume(TokenKind.EquivalenceToken);
                    break;
                case TokenKind.NotEqualToken:
                    MatchAndConsume(TokenKind.NotEqualToken);
                    break;
                default:
                    break;
            }
            Ascend();
            return;
        }

        public void ParseIdentifier()
        {
            Console.WriteLine("Entered parse identifier.");
            MatchAndConsume(TokenKind.IdentifierToken);
            Ascend();
        }

        public bool MatchAndConsume(TokenKind expectedKind)
        {
            System.Console.WriteLine($"Matching token {expectedKind}.");
            bool success = false;
            if (currentToken.Kind == expectedKind)
            {
                ConsumeToken();
            }
            else
            {
                _diagnostics.Parser_ReportUnexpectedToken(currentToken, expectedKind);
            }
            return success;
        }

        public void ConsumeToken()
        {
            if (TokenStream.Count >= 0)
            {
                System.Console.WriteLine($"Consuming token {currentToken.Kind}.");
                TokenStream.RemoveAt(0);
                Next();
            }
            else
            {
                //_diagnostics.Parser_ReportNoRemainingTokens();
            }
        }

        public void Next()
        {
            if (TokenStream.Count > 0)
            {
                currentToken = TokenStream.First();
            }
        }
        public void Ascend()
        {
            System.Console.WriteLine("Ascending tree");
        }

        public void DisplayCST()
        {
            System.Console.WriteLine("Displaying CST.");
        }
    }
}
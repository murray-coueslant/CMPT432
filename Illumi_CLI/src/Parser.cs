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

        public List<Token> TokenStream { get; }

        public Token currentToken { get; }

        public Parser(Lexer lexer)
        {
            Lexer = lexer;
            TokenStream = Lexer.GetParseTokens();
            currentToken = TokenStream.First();
            _diagnostics = new DiagnosticCollection();
        }

        public void Parse()
        {
            System.Console.WriteLine("Beginning parse");
            if (TokenStream.Count != 0)
            {
                ParseProgram();
                _diagnostics.DisplayDiagnostics();
                return;
            }
            else
            {
                _diagnostics.Parser_ReportNoTokens();
                _diagnostics.DisplayDiagnostics();
                return;
            }

        }

        public void ParseProgram()
        {
            System.Console.WriteLine("Entered parse program.");
            ParseBlock();
            MatchAndConsume(TokenKind.EndOfProgramToken);
            Ascend();
            return;
        }

        public void ParseBlock()
        {
            MatchAndConsume(TokenKind.LeftBraceToken);
            ParseStatementList();
            MatchAndConsume(TokenKind.RightBraceToken);
            Ascend();
            return;
        }

        public void ParseStatementList()
        {
            ParseStatement();

            Ascend();
            return;
        }

        public void ParseStatement()
        {
            switch (currentToken.Kind)
            {
                case TokenKind.PrintToken:
                    ParsePrintStatement();
                    break;
                case TokenKind.WhileToken:
                    ParseWhileStatement();
                    break;
            }

        }

        public void ParsePrintStatement() { }

        public void ParseAssignmentStatement() { }

        public void ParseVariableDeclaration() { }

        public void ParseWhileStatement() { }

        public void ParseIfStatement() { }

        public void ParseExpression() { }

        public void ParseIntExpression() { }

        public void ParseStringExpression() { }

        public void ParseBooleanExpression() { }

        public void ParseIdentifier() { }

        public boolean MatchAndConsume(TokenKind expectedKind)
        {
            if (currentToken.Kind == expectedKind)
            {
                ConsumeToken();
                return true;
            }
            else
            {
                _diagnostics.Parser_ReportUnexpectedToken(currentToken, expectedKind);
            }
        }

        public void ConsumeToken()
        {
            currentToken = TokenStream.
        }

        public void Ascend() { }
    }
}
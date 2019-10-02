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

        public Parser(Lexer lexer)
        {
            Lexer = lexer;
            _diagnostics = new DiagnosticCollection();
        }

        public void Parse()
        {
            if (Lexer.GetTokens().First().Kind == TokenKind.LeftBraceToken)
            {
                System.Console.WriteLine("Beginning parse");
                ParseProgram();
            }
            else
            {
                _diagnostics.Parser_ReportUnexpectedToken(Lexer.GetTokens().First(), TokenKind.LeftBraceToken);
            }
            _diagnostics.DisplayDiagnostics();
        }

        public void ParseProgram()
        {
            System.Console.WriteLine("Entered parse program.");
        }

        public void ParseBlock() { }

        public void ParseStatementList() { }

        public void ParseStatement() { }

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

        public void Match() { }

        public void ConsumeToken() { }
    }

    class ShiftReduceParser
    {
        private Stack<Token> symbols;
        private Stack<Token> input;

        public ShiftReduceParser()
        {
            symbols = new Stack<Token>();

            symbols.First();
        }


    }
}
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Text;

namespace Illumi_CLI
{
    class IllumiLexer
    {
        private const char programSeparator = '$';

        public static List<TokenStream> Lex(string fileText)
        {
            List<TokenStream> fileTokens = new List<TokenStream>();

            foreach (string program in extractPrograms(fileText))
            {
                if (program != string.Empty)
                {
                    System.Console.WriteLine(program);
                    Lexer programLexer = new Lexer(program);
                    TokenStream programTokens = programLexer.GetTokenStream();
                    fileTokens.Add(programTokens);
                    programLexer.diagnostics.DisplayDiagnostics();
                }
            }

            return fileTokens;
        }

        public static List<string> extractPrograms(string text)
        {
            return text.Split(programSeparator, StringSplitOptions.RemoveEmptyEntries)
                       .ToList();
        }
    }
    class Lexer
    {
        private int _position;
        public DiagnosticCollection diagnostics;
        public string Program { get; }
        public Lexer(string program)
        {
            Program = program;
            diagnostics = new DiagnosticCollection();
        }
        private char Current
        {
            get
            {
                if (_position >= Program.Length)
                {
                    return '\0';
                }
                return Program[_position];
            }
        }

        private void Next()
        {
            _position++;
        }
        private Token GetNextToken()
        {
            // the order of tokens in our grammar is
            //    - Keywords
            //    - IDs
            //    - Symbols
            //    - Digits
            //    - Characters
            // recognise and record whitespace, then create a token for it.
            if (char.IsWhiteSpace(Current))
            {
                int spanStart = _position;

                Next();

                while (char.IsWhiteSpace(Current))
                {
                    Next();
                }

                TextSpan span = new TextSpan(spanStart, _position - spanStart);

                return new Token(TokenKind.WhitespaceToken, span, Program.Substring(span.Start, span.Length));
            }

            if (Current == '{')
            {
                int tokenStart = _position;

                Next();

                TextSpan span = new TextSpan(tokenStart, 1);

                return new Token(TokenKind.LeftBrace, span, Program.Substring(span.Start, span.Length));
            }

            if (Current == '}')
            {
                int tokenStart = _position;

                Next();

                TextSpan span = new TextSpan(tokenStart, 1);

                return new Token(TokenKind.RightBrace, span, Program.Substring(span.Start, span.Length));
            }

            Next();

            return new Token(TokenKind.UnrecognisedToken, new TextSpan(_position, 1), Program.Substring(_position - 1, 1));

        }
        public TokenStream GetTokenStream()
        {
            int lineNumber = 0;

            TokenStream programTokens = new TokenStream();

            while (Current != '\0')
            {
                if (Current == '\n')
                {
                    lineNumber++;
                }

                int positionBeforeToken = _position;

                Token token = GetNextToken();

                if (token.Type == TokenKind.UnrecognisedToken)
                {
                    diagnostics.Lexer_ReportUnrecognisedToken(new TextSpan(positionBeforeToken, _position - positionBeforeToken), lineNumber);
                }
                else
                {
                    programTokens.AddToken(token);
                    positionBeforeToken = _position;
                }

            }

            programTokens.AddToken(new Token(TokenKind.EndOfFileToken, new TextSpan(_position, 1)));

            return programTokens;
        }
    }
    class Token
    {
        public Token(TokenKind type, TextSpan span, string text = null)
        {
            Type = type;
            Span = span;
            Text = text;
        }

        public TokenKind Type { get; }
        public TextSpan Span { get; }
        public string Text { get; }
    }
    class TokenStream
    {
        public List<Token> Tokens { get; }
        public TokenStream()
        {
            Tokens = new List<Token>();
        }

        public void AddToken(Token token)
        {
            Tokens.Add(token);
        }

        public void AddTokens(TokenStream tokens)
        {
            foreach (Token token in tokens.Tokens)
            {
                AddToken(token);
            }
        }
    }
    public enum TokenKind
    {
        EndOfFileToken,
        WhitespaceToken,
        UnrecognisedToken,
        LeftBrace,
        RightBrace
    }
}

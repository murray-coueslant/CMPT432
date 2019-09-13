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

            TokenStream endStream = new TokenStream();

            fileTokens.LastOrDefault().AddToken(new Token(TokenKind.EndOfFileToken, new TextSpan(fileText.Length - 1, 1)));

            return fileTokens;
        }

        public static List<string> extractPrograms(string text)
        {
            return text.Split(programSeparator, StringSplitOptions.RemoveEmptyEntries)
                                           .Select(prog => prog.Trim())
                                           .Select(prog => prog.Replace("\r", string.Empty))
                                           .ToList();
        }
    }
    class Lexer
    {
        private int position;
        private int linePosition;
        private int lineNumber;
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
                if (position >= Program.Length)
                {
                    return '\0';
                }
                return Program[position];
            }
        }
        private void Next()
        {
            position++;
            linePosition++;
        }
        private Token GetNextToken()
        {
            // the order of tokens in our grammar is
            //    - Keywords
            //    - IDs
            //    - Symbols
            //    - Digits
            //    - Characters

            // increment counters etc... if a newline is encountered
            if (Current == '\n')
            {
                lineNumber++;
                linePosition = 0;
            }


            // recognise and record whitespace, then create a token for it.
            if (char.IsWhiteSpace(Current))
            {
                int tokenStart = linePosition;

                Next();

                while (char.IsWhiteSpace(Current))
                {
                    Next();
                }

                TextSpan span = new TextSpan(tokenStart, linePosition - tokenStart);

                return new Token(TokenKind.WhitespaceToken, span, Program.Substring(span.Start, span.Length));
            }


            // recognise braces and tokenize, simple since braces are single characters always
            if (Current == '{')
            {
                int tokenStart = linePosition;

                Next();

                TextSpan span = new TextSpan(tokenStart, 1);

                return new Token(TokenKind.LeftBrace, span, Program.Substring(span.Start, span.Length));
            }

            if (Current == '}')
            {
                int tokenStart = linePosition;

                Next();

                TextSpan span = new TextSpan(tokenStart, 1);

                return new Token(TokenKind.RightBrace, span, Program.Substring(span.Start, span.Length));
            }

            Next();

            return new Token(TokenKind.UnrecognisedToken, new TextSpan(linePosition, 1), Program.Substring(linePosition - 1, 1));

        }
        public TokenStream GetTokenStream()
        {

            TokenStream programTokens = new TokenStream();

            while (Current != '\0')
            {
                int positionBeforeToken = linePosition;

                Token token = GetNextToken();

                if (token.Type == TokenKind.UnrecognisedToken)
                {
                    diagnostics.Lexer_ReportUnrecognisedToken(new TextSpan(positionBeforeToken, linePosition - positionBeforeToken), lineNumber);
                }
                else
                {
                    programTokens.AddToken(token);
                    positionBeforeToken = linePosition;
                }

            }

            programTokens.AddToken(new Token(TokenKind.EndOfProgramToken, new TextSpan(position, 1)));

            lineNumber = 0;

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

    public enum KeywordRegexes
    {


    }
    public enum TokenKind
    {
        KeywordToken,
        EndOfFileToken,
        WhitespaceToken,
        UnrecognisedToken,
        LeftBrace,
        RightBrace,
        EndOfProgramToken
    }
}

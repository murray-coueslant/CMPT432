using System.Linq;
using System;
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
                Lexer programLexer = new Lexer(program);
                TokenStream programTokens = programLexer.GetTokenStream();
                fileTokens.Add(programTokens);
            }

            return fileTokens;
        }

        public static List<string> extractPrograms(string text)
        {
            return text.Split(programSeparator)
                       .ToList();
        }
    }
    class Lexer
    {
        private int _position;
        public string Program { get; }
        public Lexer(string program)
        {
            Program = program;
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
            // recognise and record whitespace, then create a token for it.
            if (!char.IsWhiteSpace(Current))
            {
                Next();
                return null;
            }
            else
            {
                int spanStart = _position;

                Next();

                while (char.IsWhiteSpace(Current))
                {
                    Next();
                }

                TextSpan span = new TextSpan(spanStart, _position - spanStart);

                return new Token(TokenKind.WhitespaceToken, span);
            }
        }
        public TokenStream GetTokenStream()
        {
            TokenStream programTokens = new TokenStream();

            while (Current != '\0')
            {
                programTokens.AddToken(GetNextToken());
            }

            programTokens.AddToken(new Token(TokenKind.EndOfFileToken, new TextSpan(_position, 1)));

            return programTokens;
        }
    }
    class Token
    {
        public Token(TokenKind type, TextSpan span)
        {
            Type = type;
            Span = span;
        }

        public TokenKind Type { get; }
        public TextSpan Span { get; }
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
    }
    public enum TokenKind
    {
        EndOfFileToken,
        WhitespaceToken
    }
}

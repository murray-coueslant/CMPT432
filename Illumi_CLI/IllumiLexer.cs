using System;
using System.Collections.Generic;

namespace Illumi_CLI
{
    class IllumiLexer
    {
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
                if (_position > Program.Length)
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

        }

        private TokenStream GetTokenStream()
        {
            TokenStream programTokens = new TokenStream();

            while (Current != '\0')
            {
                programTokens.AddToken(GetNextToken());
            }
            return programTokens;
        }
    }

    class Token
    {
        public Token(TokenKind type)
        {
            Type = type;
        }

        public TokenKind Type { get; }
    }

    class TokenStream
    {
        public List<Token> Tokens { get; }
        public TokenStream() { }

        public void AddToken(Token token)
        {
            Tokens.Add(token);
        }
    }

    public enum TokenKind
    {
        EndOfFileToken
    }
}

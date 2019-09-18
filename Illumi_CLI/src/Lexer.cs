using System;
using System.Text;

namespace Illumi_CLI
{
    class Lexer
    {
        private DiagnosticCollection _diagnostics = new DiagnosticCollection();
        private string _text;

        private int _position;
        private int _lineNumber;

        private int _tokenStart;
        private TokenKind _kind;
        private object _value;
        private TokenStream Tokens;

        public Lexer(string text)
        {
            _text = text;
        }

        public DiagnosticCollection Diagnostics => _diagnostics;

        private char CurrentChar => lookChar(0);
        private char LookaheadChar => lookChar(1);

        private char lookChar(int offset)
        {
            var charPosition = _position + offset;

            if (charPosition < _text.Length)
            {
                return _text[charPosition];
            }

            return '\0';
        }



        public Token Lex()
        {

        }
    }
}
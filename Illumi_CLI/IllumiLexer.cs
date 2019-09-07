using System;
using System.Collections.Generic;
using System.IO;

namespace Illumi_CLI
{
    internal class IllumiLexer
    {
        internal static void Lex(string programText)
        {
            int programCount = 0;

            int[] warningsErrors;

            IList<string> programs = ExtractProgramSubstrings(programText);

            foreach (string program in programs)
            {
                Console.WriteLine($"Lexing program {programCount}");
                warningsErrors = LexProgram(program);
                Console.WriteLine($"Program {programCount} lex finished with {warningsErrors[0]} warnings and {warningsErrors[1]} errors.");
                programCount++;
            }
        }

        private static IList<string> ExtractProgramSubstrings(string programText)
        {
            int start = 0;
            int position = start;

            IList<string> programs = new List<string>();

            while (position < programText.Length)
            {
                while(programText[position] != '$')
                {
                    position++;
                }

                programs.Add(programText.Substring(start, position - start + 1));

                position++;
                start = position; 
            }

            return programs;
        }

        private static int[] LexProgram(string program)
        {
            int[] warningsErrors = new int[2];
            int warnings = 0;
            int errors = 0;

            Lexer lexer = new Lexer(program);

            Token token = lexer.GetNextToken();

            while(token.Kind != TokenKind.NullToken)
            {
                Console.WriteLine(token.Kind);
                Console.WriteLine(token.Value);
                token = lexer.GetNextToken();
            }         

            warningsErrors[0] = warnings;
            warningsErrors[1] = errors;

            return warningsErrors;
        }
    }

    class Lexer
    {
        private string _program;
        private int _position;

        public Lexer(string program)
        {
            _program = program;
        }

        private char Current
        {
            get
            {
                if(_position >= _program.Length)
                {
                    return '\0';
                }

                return _program[_position];
            }
        }

        private void Next()
        {
            _position++;
        }

        public Token GetNextToken()
        {
            // The tokens in our grammar are:
            // - Keywords (print, while, if)
            // - Types (int, string, boolean)
            // - Braces ({, })
            // - Operators (=, ==, !=)
            // - Arithmetic (+)
            // - Boolean Values (true, false)
            // - Identifiers (single char, a-z)
            // - Digits (1 - 9)
            // - Parenthesis ((,))

            // let's start by recognising the digits
            if (char.IsDigit(Current))
            {
                int tokenStart = _position;

                while (char.IsDigit(Current))
                {
                    Next();
                }

                int length = _position - tokenStart;
                string text = _program.Substring(tokenStart, length);
                int.TryParse(text, out var integerValue);
                return new Token(TokenKind.DigitToken, _position, text, integerValue);
            }

            if(Current == '$')
            {
                Next();
                return new Token(TokenKind.EndOfProgramToken, _position, "$", null);
            }

            if (char.IsWhiteSpace(Current))
            {
                int tokenStart = _position;

                while (char.IsWhiteSpace(Current))
                {
                    Next();
                }

                int length = _position - tokenStart;
                string text = _program.Substring(tokenStart, length);

                return new Token(TokenKind.WhitespaceToken, _position, text, null);
            }

            return new Token(TokenKind.NullToken, 0, String.Empty, null);
        }
    }

    enum TokenKind {
        DigitToken,
        NullToken,
        EndOfProgramToken,
        WhitespaceToken
    }

    class Token
    {
        public Token(TokenKind kind, int position, string text, object value)
        {
            Kind = kind;
            Position = position;
            Text = text;
            Value = value;
        }

        public TokenKind Kind { get; }
        public int Position { get; }
        public string Text { get; }
        public object Value { get; }
    }
}
// this is an utter mess at the moment, I'm just throwing ideas at the screen to see what sticks. About to make the interface much nicer.

// ideas:
// - Digest all tokens into a token stream
//   - Keywords
//   - Types
//   - Identifiers
//   - Digits
//   - Operators
//   - Arithmetic
//   - Parenthesis
//   - Booleans
//   - Strings
//   - Parenthesis
//   - Braces
//   - Whitespace
//     - See below for whitespace tokenization explanation
// - Match parenthesis and brace tokens, ensure no hanging or erroneous braces
// - Add line information to each token
//   - Could also create a 'lookup table' of sorts, with position ranges which correspond to line numbers
//     which can be created when the program is first digested. This would allow the compiler to discard all
//     whitespace etc... right at the beginning when the program is being read from the file

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
                while (programText[position] != '$')
                {
                    position++;
                }

                TextProgram program = new TextProgram(programText.Substring(start, position - start + 1));

                // programs.Add(programText.Substring(start, position - start + 1));

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

            while (token.Kind != TokenKind.ZeroTerminatorToken)
            {
                Console.WriteLine(token.Kind);
                if (token.Kind == TokenKind.ParenthesizedExpressionToken)
                {
                    Console.WriteLine(token.Text);
                }
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
                if (_position >= _program.Length)
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
            // - End of Program ($)
            // - Whitespace
            //   - I decided that I should tokenize whitespace as it allows the compiler
            //     to preserve position information, and it is easy to discard whitespace
            //     tokens at parse time or at the end of lex

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

            if (Current == '$')
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

            if (Current == '(')
            {
                string parenthesizedExpression = Match();

                return new Token(TokenKind.ParenthesizedExpressionToken, _position, parenthesizedExpression, null);
            }


            return new Token(TokenKind.ZeroTerminatorToken, 0, String.Empty, null);

        }

        private string Match()
        {
            Next();

            int expressionStart = _position;

            while (Current != ')')
            {
                Next();
            }

            int length = _position - expressionStart;
            string text = _program.Substring(expressionStart, length);

            return text;
        }
    }

    enum TokenKind
    {
        DigitToken,
        ZeroTerminatorToken,
        EndOfProgramToken,
        WhitespaceToken,
        ParenthesisToken,
        ParenthesizedExpressionToken
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

class TextProgram
{
    public TextProgram(string text)
    {
        Text = text;
        generateLineNumbers();
        foreach (List<int> line in Lines)
        {
            foreach (int number in line)
            {
                Console.WriteLine(number);
            }
        }
    }

    public string Text { get; private set; }

    private void generateLineNumbers()
    {
        int startPosition = 0;
        int endPosition = startPosition;
        int lineNumber = 0;

        List<List<int>> lineList = new List<List<int>>();

        foreach (char c in Text)
        {
            if (c == '\n')
            {
                List<int> lineInfoList = new List<int>();

                lineInfoList.Add(startPosition);
                lineInfoList.Add(endPosition);
                lineInfoList.Add(lineNumber);

                lineList.Add(lineInfoList);

                lineNumber++;
                startPosition = 0;
                endPosition = startPosition;
            }

            endPosition++;
        }

        Lines = lineList;
    }

    public List<List<int>> Lines { get; private set; }
}
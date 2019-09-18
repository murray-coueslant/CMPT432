using System;
using System.Text;
using Microsoft.CodeAnalysis.Text;

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
        private TokenStream _tokens;

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

        private void Next()
        {
            _position++;
        }

        public Token Lex()
        {
            /*
                Initialise the variables about the token to be found to describe a bad
                token, and change them if we find anything else
            */
            _tokenStart = _position;
            _kind = TokenKind.UnrecognisedToken;
            _value = null;

            switch (CurrentChar)
            {
                // deal with the 0 terminator character first, this tells us we
                // have reached the end of our program
                case '\0':
                    _kind = TokenKind.EndOfProgramToken;
                    break;

                // now we can start handling some symbols
                case '$':
                    _kind = TokenKind.EndOfProgramToken;
                    break;

                case '{':
                    _kind = TokenKind.LeftBraceToken;
                    Next();
                    break;

                case '}':
                    _kind = TokenKind.RightBraceToken;
                    Next();
                    break;

                case '(':
                    _kind = TokenKind.LeftParenthesisToken;
                    Next();
                    break;

                case ')':
                    _kind = TokenKind.RightParenthesisToken;
                    Next();
                    break;

                case '+':
                    _kind = TokenKind.AdditionToken;
                    Next();
                    break;

                case '=':
                    Next();
                    if (CurrentChar == '=')
                    {
                        _kind = TokenKind.EquivalenceToken;
                        Next();
                    }
                    else
                    {
                        _kind = TokenKind.AssignmentToken;
                    }
                    break;

                case '!':
                    Next();
                    if (CurrentChar == '=')
                    {
                        _kind = TokenKind.NotEqualToken;
                        Next();
                    }
                    break;

                case '"':
                    HandleString();
                    break;

                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    _kind = TokenKind.DigitToken;
                    break;

                case '/':
                    Next();
                    if (CurrentChar == '*')
                    {
                        HandleComment();
                    }
                    break;

                // now we look out for whitespace, newlines and returns bump up
                // the line number, spaces and tabs don't
                case '\n':
                case '\r':
                    HandleWhitespace();
                    _lineNumber++;
                    break;

                case ' ':
                case '\t':
                    HandleWhitespace();
                    break;

                // if none of the prior cases have handled the character, then
                // we have either an identifier or a keyword, or a bad character
                default:
                    if (char.IsLetter(CurrentChar))
                    {
                        HandleKeywordOrIdentifier();
                    }
                    else if (char.IsWhiteSpace(CurrentChar))
                    {
                        HandleWhitespace();
                    }
                    else
                    {
                        _diagnostics.Lexer_ReportInvalidCharacter(new TextSpan(), _lineNumber);
                        Next();
                    }
                    break;
            }

            // calculate the length of the token found, and then grab the
            // text of that token from the program text
            int tokenLength = _position - _tokenStart;
            string text = _text.Substring(_tokenStart, tokenLength);

            return new Token(_kind, text);
        }

        private void HandleKeywordOrIdentifier()
        {
            throw new NotImplementedException();
        }

        /*
            When we come across a whitespace character, simply continue onwards
            until we encounter the next non whitespace character
        */
        private void HandleWhitespace()
        {
            while (char.IsWhiteSpace(CurrentChar))
            {
                Next();
            }

            _kind = TokenKind.WhitespaceToken;
        }

        private void HandleComment()
        {
            throw new NotImplementedException();
        }

        // in the special case where we encounter a string, take anything between
        // the quotes 
        private void HandleString()
        {
            Next();

            StringBuilder stringText = new StringBuilder();
            bool finishedString = false;

            TextSpan erroneousSpan;

            while (!finishedString)
            {
                switch (CurrentChar)
                {
                    // handle the cases in which we find a string to be unterminated
                    // we don't allow for multiline strings
                    case '\0':
                    case '\r':
                    case '\n':
                        erroneousSpan = new TextSpan(_tokenStart, 1);
                        _diagnostics.Lexer_ReportUnterminatedString(erroneousSpan, _lineNumber);
                        finishedString = true;
                        break;

                    // handle the ending case, where we find another quote character
                    case '"':
                        if (LookaheadChar == '"')
                        {
                            stringText.Append(CurrentChar);
                            Next();
                            Next();
                        }
                        else
                        {
                            Next();
                            finishedString = true;
                        }
                        break;

                    // handle the case of the string containing spaces
                    case ' ':
                        stringText.Append(CurrentChar);
                        Next();
                        break;

                    // the default case, where we continue to add to the string,
                    // so long as the character is allowable
                    default:
                        if (char.IsLetter(CurrentChar))
                        {
                            stringText.Append(CurrentChar);
                            Next();
                        }
                        else
                        {
                            erroneousSpan = new TextSpan(_tokenStart, 1);
                            _diagnostics.Lexer_ReportInvalidCharacterInString(erroneousSpan, _lineNumber);
                            finishedString = true;
                        }
                        break;
                }
            }

            _kind = TokenKind.StringToken;
            _value = stringText.ToString();
        }
    }
}
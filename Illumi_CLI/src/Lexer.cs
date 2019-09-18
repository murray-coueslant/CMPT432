using System.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis.Text;

namespace Illumi_CLI
{
    class Lexer
    {
        private DiagnosticCollection _diagnostics = new DiagnosticCollection();
        private Session _lexerSession;
        private List<Token> _tokens;
        private string _text;
        private int _position;
        private int _lineNumber;
        private int _linePosition;
        private int _tokenStart;
        private TokenKind _kind;
        private object _value;
        private char[] allowableChars = { '-', ':', ';', ',', '.' };

        public Lexer(string text, Session session)
        {
            _text = text;
            _lexerSession = session;
            _tokens = new List<Token>();
        }

        public DiagnosticCollection Diagnostics => _diagnostics;

        private char CurrentChar => lookChar(0);
        private char LookaheadChar => lookChar(1);

        public int ErrorCount { get; internal set; }
        public object WarningCount { get; internal set; }

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
            _linePosition++;
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
                    Next();
                    break;

                case '/':
                    Next();
                    if (CurrentChar == '*')
                    {
                        HandleComment();
                    }
                    break;

                // now we look out for whitespace, newlines bump up
                // the line number, spaces, tabs, and returns don't
                case '\n':
                case '\r':
                case ' ':
                case '\t':
                    HandleWhitespace();
                    break;

                // if none of the prior cases have handled the character, then
                // we have either an identifier or a keyword, or a bad character,
                // or another form of whitespace
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
                        _diagnostics.Lexer_ReportInvalidCharacter(new TextSpan(_position, 1), _lineNumber, CurrentChar);
                        _diagnostics.ErrorCount++;
                    }
                    break;
            }

            _diagnostics.DisplayDiagnostics();

            // calculate the length of the token found, and then grab the
            // text of that token from the program text
            int tokenLength = _position - _tokenStart;
            string text = _text.Substring(_tokenStart, tokenLength);

            if (_lexerSession.debugMode)
            {
                if (_kind != TokenKind.UnrecognisedToken)
                {
                    _diagnostics.Lexer_ReportToken(_kind, text, _linePosition - tokenLength, _lineNumber);
                }
            }

            Token token = new Token(_kind, text);
            _tokens.Add(token);
            return token;
        }

        internal void ClearTokens()
        {
            _tokens = new List<Token>();
        }

        internal List<Token> GetTokens()
        {
            return _tokens;
        }

        private void HandleKeywordOrIdentifier()
        {
            int _bufferStartPosition = _position;

            StringBuilder buffer = new StringBuilder();

            switch (CurrentChar)
            {
                case 'w':
                case 'i':
                case 'p':
                case 'b':
                case 's':
                    HandleKeyword();
                    break;
                default:
                    _kind = TokenKind.IdentifierToken;
                    Next();
                    break;

            }

            int length = _position - _tokenStart;
            string text = _text.Substring(_tokenStart, length);
        }

        private void HandleKeyword()
        {
            StringBuilder buffer = new StringBuilder();

            switch (LookaheadChar)
            {
                case 'h':
                case 'n':
                case 'f':
                case 'r':
                case 'o':
                case 't':
                    buffer.Append(CurrentChar);
                    Next();
                    break;
                default:
                    _kind = TokenKind.IdentifierToken;
                    Next();
                    break;
            }
        }

        /*
            Match a given text to the selection of possible keywords, if it does not match then
            assume it is an identifier.
        */
        private TokenKind MatchKeywordKind(string text)
        {
            switch (text)
            {
                case "while":
                    return TokenKind.WhileToken;
                case "print":
                    return TokenKind.PrintToken;
                case "if":
                    return TokenKind.IfToken;
                case "boolean":
                    return TokenKind.Type_BooleanToken;
                case "string":
                    return TokenKind.Type_StringToken;
                case "int":
                    return TokenKind.Type_IntegerToken;
                default:
                    if (text.Length == 1)
                    {
                        return TokenKind.IdentifierToken;
                    }
                    return TokenKind.UnrecognisedToken;
            }
        }

        /*
            When we come across a whitespace character, simply continue onwards
            until we encounter the next non whitespace character
        */
        private void HandleWhitespace()
        {
            while (char.IsWhiteSpace(CurrentChar))
            {
                if (CurrentChar == '\n')
                {
                    _lineNumber++;
                    _linePosition = 0;
                }
                Next();
            }

            _kind = TokenKind.WhitespaceToken;
        }

        private void HandleComment()
        {
            // to handle a comment, we will continure through the text updating positions until we find
            // the terminator. similar to how we handle strings
            Next();

            bool finishedComment = false;

            TextSpan erroneousSpan;

            while (!finishedComment)
            {
                switch (CurrentChar)
                {
                    // handle the end cases (no multiline comments, unfinished comment etc...)
                    case '\0':
                    case '\r':
                    case '\n':
                    case '$':
                        _diagnostics.Lexer_ReportMalformedComment(_tokenStart, _lineNumber);
                        _diagnostics.ErrorCount++;
                        finishedComment = true;
                        return;

                    case '*':
                        if (LookaheadChar == '/')
                        {
                            Next();
                            Next();
                            finishedComment = true;
                        }
                        break;

                    case ' ':
                        Next();
                        break;

                    default:
                        if (char.IsLetter(CurrentChar) || allowableChars.Contains(CurrentChar))
                        {
                            Next();
                        }
                        else
                        {
                            erroneousSpan = new TextSpan(_tokenStart, 1);
                            _diagnostics.Lexer_ReportInvalidCharacterInComment(erroneousSpan, _lineNumber, CurrentChar);
                            _diagnostics.ErrorCount++;
                            finishedComment = true;
                            return;
                        }
                        break;
                }
            }

            _kind = TokenKind.CommentToken;
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
                    case '$':
                        erroneousSpan = new TextSpan(_tokenStart, 1);
                        _diagnostics.Lexer_ReportUnterminatedString(erroneousSpan, _lineNumber);
                        _diagnostics.ErrorCount++;
                        finishedString = true;
                        return;

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
                            _diagnostics.Lexer_ReportInvalidCharacterInString(erroneousSpan, _lineNumber, CurrentChar);
                            _diagnostics.ErrorCount++;
                            finishedString = true;
                            return;
                        }
                        break;
                }
            }

            _kind = TokenKind.StringToken;
            _value = stringText.ToString();
        }
    }
}
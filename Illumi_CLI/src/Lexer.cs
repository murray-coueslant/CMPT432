using System.Threading.Tasks.Dataflow;
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
        private int _tokenLength;
        private string _tokenText;
        private TokenKind _kind;
        private object _value;
        private char[] _allowablePunctuation = { '-', ':', ';', ',', '.' };
        private char[] _keywordFirstCharacters = { 'i', 'w', 'b', 'p', 's' };

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

        private void Next(int offset)
        {
            _position += offset;
            _linePosition += offset;
        }

        public void Lex()
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
                // have reached the end of our file
                case '\0':
                    _kind = TokenKind.EndOfFileToken;
                    break;

                // now we can start handling some symbols
                case '$':
                    _kind = TokenKind.EndOfProgramToken;
                    // calculate the length of the token found, and then grab the
                    // text of that token from the program text
                    _tokenLength = _position - _tokenStart;
                    _tokenText = _text.Substring(_tokenStart, _tokenLength);
                    break;

                case '{':
                    _kind = TokenKind.LeftBraceToken;
                    Next();
                    // calculate the length of the token found, and then grab the
                    // text of that token from the program text
                    _tokenLength = _position - _tokenStart;
                    _tokenText = _text.Substring(_tokenStart, _tokenLength);
                    break;

                case '}':
                    _kind = TokenKind.RightBraceToken;
                    Next();
                    // calculate the length of the token found, and then grab the
                    // text of that token from the program text
                    _tokenLength = _position - _tokenStart;
                    _tokenText = _text.Substring(_tokenStart, _tokenLength);
                    break;

                case '(':
                    _kind = TokenKind.LeftParenthesisToken;
                    Next();
                    // calculate the length of the token found, and then grab the
                    // text of that token from the program text
                    _tokenLength = _position - _tokenStart;
                    _tokenText = _text.Substring(_tokenStart, _tokenLength);
                    break;

                case ')':
                    _kind = TokenKind.RightParenthesisToken;
                    Next();
                    // calculate the length of the token found, and then grab the
                    // text of that token from the program text
                    _tokenLength = _position - _tokenStart;
                    _tokenText = _text.Substring(_tokenStart, _tokenLength);
                    break;

                case '+':
                    _kind = TokenKind.AdditionToken;
                    Next();
                    // calculate the length of the token found, and then grab the
                    // text of that token from the program text
                    _tokenLength = _position - _tokenStart;
                    _tokenText = _text.Substring(_tokenStart, _tokenLength);
                    break;

                case '=':
                    Next();
                    if (CurrentChar == '=')
                    {
                        _kind = TokenKind.EquivalenceToken;
                        Next();
                        // calculate the length of the token found, and then grab the
                        // text of that token from the program text
                        _tokenLength = _position - _tokenStart;
                        _tokenText = _text.Substring(_tokenStart, _tokenLength);
                    }
                    else
                    {
                        _kind = TokenKind.AssignmentToken;
                        // calculate the length of the token found, and then grab the
                        // text of that token from the program text
                        _tokenLength = _position - _tokenStart;
                        _tokenText = _text.Substring(_tokenStart, _tokenLength);
                    }
                    break;

                case '!':
                    Next();
                    if (CurrentChar == '=')
                    {
                        _kind = TokenKind.NotEqualToken;
                        Next();
                        // calculate the length of the token found, and then grab the
                        // text of that token from the program text
                        _tokenLength = _position - _tokenStart;
                        _tokenText = _text.Substring(_tokenStart, _tokenLength);
                    }
                    break;

                case '"':
                    HandleString();
                    return;

                // now we handle digits
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
                    // calculate the length of the token found, and then grab the
                    // text of that token from the program text
                    _tokenLength = _position - _tokenStart;
                    _tokenText = _text.Substring(_tokenStart, _tokenLength);
                    break;

                // comments are complex, and need their own handler function. but we can also report basic
                // format errors here and avoid further complications
                case '/':
                    Next();
                    if (CurrentChar == '*')
                    {
                        HandleComment();
                    }
                    else
                    {
                        _diagnostics.Lexer_ReportMalformedComment(_tokenStart, _lineNumber);
                    }
                    return;

                // now we look out for whitespace, newlines bump up
                // the line number, spaces, tabs, and returns don't
                case '\n':
                case ' ':
                case '\t':
                    HandleWhitespace();
                    return;

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
                    }
                    return;
            }

            EmitToken(_kind, _tokenText);
        }

        private void EmitToken(TokenKind kind, string text)
        {
            Token token = new Token(kind, text);
            _tokens.Add(token);

            if (_lexerSession.debugMode)
            {
                if (kind != TokenKind.UnrecognisedToken)
                {
                    _diagnostics.Lexer_ReportToken(kind, text, _linePosition - _tokenLength, _lineNumber);
                }
            }
        }

        public void ClearTokens()
        {
            _tokens = new List<Token>();
        }

        public List<Token> GetTokens()
        {
            return _tokens;
        }

        private void HandleKeywordOrIdentifier()
        {
            switch (CurrentChar)
            {
                case 'w':
                case 'i':
                case 'p':
                case 'b':
                case 's':
                    HandleKeyword();
                    return;
                default:
                    _kind = TokenKind.IdentifierToken;
                    Next();
                    break;

            }

            _tokenLength = _position - _tokenStart;
            _tokenText = _text.Substring(_tokenStart, _tokenLength);

            EmitToken(_kind, _tokenText);
        }

        private void HandleKeyword()
        {
            StringBuilder buffer = new StringBuilder();
            int offset = 0;

            buffer.Append(lookChar(offset));

            do
            {
                TokenKind matchKind = MatchKeywordKind(buffer.ToString());
                if (matchKind != TokenKind.UnrecognisedToken)
                {
                    _tokenLength = ++offset;
                    _tokenText = _text.Substring(_tokenStart, _tokenLength);
                    EmitToken(matchKind, _tokenText);
                    for (int i = 0; i < offset; i++)
                    {
                        Next();
                    }
                    return;
                }
                else if (char.IsWhiteSpace(lookChar(offset + 1))
                         || char.IsPunctuation(lookChar(offset + 1))
                         || char.IsSymbol(lookChar(offset + 1))
                         )
                {
                    if (_keywordFirstCharacters.Contains(buffer.ToString()[0]))
                    {
                        if (!TestKeywords())
                        {
                            EmitIdentifiers(buffer.ToString());
                            buffer.Clear();
                        }
                    }
                }

                else
                {
                    offset++;
                    buffer.Append(lookChar(offset));
                }
            } while (!char.IsPunctuation(lookChar(offset))
                         && !char.IsWhiteSpace(lookChar(offset))
                         && _position + offset < _text.Length);
        }

        private void EmitIdentifiers(string buffer)
        {
            foreach (char c in buffer)
            {
                EmitToken(TokenKind.IdentifierToken, c.ToString());
                Next();
            }
            return;
        }


        private bool TestKeywords()
        {
            Dictionary<TokenKind, string> keywords = new Dictionary<TokenKind, string>()
            {
                {TokenKind.WhileToken, "while"},
                {TokenKind.IfToken, "if"},
                {TokenKind.PrintToken, "print"},
                {TokenKind.Type_BooleanToken, "boolean"},
                {TokenKind.Type_StringToken, "string"},
                {TokenKind.Type_IntegerToken, "int"},
                {TokenKind.TrueToken, "true"},
                {TokenKind.FalseToken, "false"}
            };

            foreach (var entry in keywords)
            {
                if (_text.Substring(_position, entry.Value.Length) == entry.Value)
                {
                    EmitToken(entry.Key, entry.Value);
                    return true;
                }
            }

            return false;

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
            _tokenLength = _position - _tokenStart;
            _tokenText = _text.Substring(_tokenStart, _tokenLength);
            EmitToken(_kind, _tokenText);
        }

        private void HandleComment()
        {
            // to handle a comment, we will continue through the text updating positions until we find
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
                    case '\n':
                    case '$':
                        _diagnostics.Lexer_ReportMalformedComment(_tokenStart, _lineNumber);
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
                        if (char.IsLetter(CurrentChar) || _allowablePunctuation.Contains(CurrentChar))
                        {
                            Next();
                        }
                        else
                        {
                            erroneousSpan = new TextSpan(_tokenStart, 1);
                            _diagnostics.Lexer_ReportInvalidCharacterInComment(erroneousSpan, _lineNumber, CurrentChar);
                            finishedComment = true;
                            return;
                        }
                        break;
                }
            }

            _kind = TokenKind.CommentToken;
            _tokenLength = _position - _tokenStart;
            _tokenText = _text.Substring(_tokenStart, _tokenLength);
            EmitToken(_kind, _tokenText);
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
                            finishedString = true;
                            return;
                        }
                        break;
                }
            }

            _kind = TokenKind.StringToken;
            _tokenText = stringText.ToString();
            EmitToken(_kind, _tokenText);
        }
    }
}
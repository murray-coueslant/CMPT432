using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Collections.ObjectModel;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Illumi_CLI
{
    /*
        The lexer for the Illumi compiler.

        A lexer takes raw text that it assumes to be source code, and attempts to create a stream of tokens from it. The tokens
        which a lexer creates must be perfect according to the grammar of a language. Once the lex phase is complete the tokens are
        then consumed by the semantic analysis phase.

        Currently, the illumi compiler recognises patterns in the source and associcates them with their tokens using regular expressions.
    */
    class AcProgram
    {
        public AcProgram(string text, int sourceFileStartPosition, int length)
        {
            Text = text;
            SourceFileStartPosition = sourceFileStartPosition;
            Length = length;
        }

        public string Text { get; }
        public int SourceFileStartPosition { get; }
        public int Length { get; }
    }
    class Token
    {
        public Token(TokenKind kind, string text)
        {
            Kind = kind;
            Text = text;
        }

        public TokenKind Kind { get; }
        public string Text { get; }
    }

    class TokenStream
    {
        public TokenStream()
        {
            Tokens = new Stack<Token>();
        }

        public static Stack<Token> Tokens { get; private set; }

        public void PushToken(Token token)
        {
            Tokens.Push(token);
        }

        public Token PopToken()
        {
            return Tokens.Pop();
        }
    }

    public enum TokenKind
    {
        WhitespaceToken,
        LeftBraceToken,
        RightBraceToken,
        Type_IntegerToken,
        Type_StringToken,
        Type_BooleanToken,
        IdentifierToken,
        UnrecognisedToken,
        WhileToken,
        PrintToken,
        IfToken,
        DigitToken,
        StringToken,
        LeftParenthesisToken,
        RightParenthesisToken,
        AssignmentToken,
        TrueToken,
        FalseToken,
        EquivalenceToken
    }

    class TokenRegularExpression
    {
        public TokenRegularExpression(string pattern, TokenKind kind)
        {
            Pattern = pattern;
            Kind = kind;
        }

        public string Pattern { get; }
        public TokenKind Kind { get; }
    }

    class Lexer
    {
        /*
            The variables required for treatment of comments, strings etc... as well as positions in the file
        */
        private int _position;
        private int _sourceLineNumber;
        private int _programLineNumber;
        private int _programNumber = 0;
        private bool _insideComment;
        private bool _insideString;

        /*
            More specific properties which are used by the lexer, such as diagnostics and the source text the lexer is provided
        */
        public string SourceText { get; }
        public DiagnosticCollection Diagnostics { get; }
        public Session LexerSession { get; private set; }
        public IEnumerable<AcProgram> Programs { get; }
        public Dictionary<TokenKind, Regex> TokenRegularExpressions { get; private set; }
        public TokenStream LexerTokenStream = new TokenStream();

        public Lexer(string sourceText, Session currentSession)
        {
            Console.WriteLine("Entering the Illumi lexer.");

            SourceText = sourceText;
            Diagnostics = currentSession.Diagnostics;
            LexerSession = currentSession;
            Programs = ExtractPrograms();
            TokenRegularExpressions = GenerateRegularExpressions();

            Lex();
        }

        private Dictionary<TokenKind, Regex> GenerateRegularExpressions()
        {
            Dictionary<TokenKind, Regex> regularExpressionDictionary = new Dictionary<TokenKind, Regex>();

            // whitespace expression
            Regex whitespaceExpression = new Regex(@"\s");
            regularExpressionDictionary.Add(TokenKind.WhitespaceToken, whitespaceExpression);

            // brace expressions
            Regex leftBraceExpression = new Regex("{");
            regularExpressionDictionary.Add(TokenKind.LeftBraceToken, leftBraceExpression);
            Regex rightBraceExpression = new Regex("}");
            regularExpressionDictionary.Add(TokenKind.RightBraceToken, rightBraceExpression);

            // keyword expressions
            // type expressions
            Regex intExpression = new Regex(@"\bint\b");
            regularExpressionDictionary.Add(TokenKind.Type_IntegerToken, intExpression);
            Regex stringExpression = new Regex(@"\bstring\b");
            regularExpressionDictionary.Add(TokenKind.Type_StringToken, stringExpression);
            Regex boolExpression = new Regex(@"\bboolean\b");
            regularExpressionDictionary.Add(TokenKind.Type_BooleanToken, boolExpression);

            // conditional expressions
            Regex ifExpression = new Regex(@"\bif\b");
            regularExpressionDictionary.Add(TokenKind.IfToken, ifExpression);

            // loop expressions
            Regex whileExpression = new Regex(@"\bwhile\b");
            regularExpressionDictionary.Add(TokenKind.WhileToken, whileExpression);

            // other expressions
            Regex printExpression = new Regex(@"\bprint\b");
            regularExpressionDictionary.Add(TokenKind.PrintToken, printExpression);

            // id expression
            Regex idExpression = new Regex(@"\b[a-z]\b");
            regularExpressionDictionary.Add(TokenKind.IdentifierToken, idExpression);

            // symbol expressions
            Regex leftParenExpression = new Regex(@"[\(]");
            regularExpressionDictionary.Add(TokenKind.LeftParenthesisToken, leftParenExpression);

            Regex rightParenExpression = new Regex(@"[\)]");
            regularExpressionDictionary.Add(TokenKind.RightParenthesisToken, rightParenExpression);

            Regex assignmentExpression = new Regex("=");
            regularExpressionDictionary.Add(TokenKind.AssignmentToken, assignmentExpression);

            Regex equivalenceExpression = new Regex("==");
            regularExpressionDictionary.Add(TokenKind.EquivalenceToken, equivalenceExpression);

            // digit expression
            Regex digitExpression = new Regex(@"\b[1-9]\b");
            regularExpressionDictionary.Add(TokenKind.DigitToken, digitExpression);

            //boolean expressions
            Regex trueExpression = new Regex(@"true");
            regularExpressionDictionary.Add(TokenKind.TrueToken, trueExpression);

            Regex falseExpression = new Regex(@"false");
            regularExpressionDictionary.Add(TokenKind.FalseToken, falseExpression);

            return regularExpressionDictionary;
        }
        public void Lex()
        {
            foreach (AcProgram program in Programs)
            {
                LexicalAnalysis(program);
            }
        }

        private void LexicalAnalysis(AcProgram program)
        {
            int programPosition = 0;
            int sourcePosition = program.SourceFileStartPosition;

            StringBuilder sourceBuffer = new StringBuilder();

            char currentChar = program.Text[programPosition];

            while (currentChar != '$')
            {
                if (currentChar == '/' && program.Text[programPosition + 1] == '*')
                {
                    _insideComment = true;
                    int commentLength = HandleComment(program, programPosition);
                    if (commentLength == -1)
                    {
                        Diagnostics.DisplayDiagnostics();
                        break;
                    }
                    else
                    {
                        programPosition += commentLength;
                        sourcePosition += commentLength;
                        currentChar = program.Text[programPosition];
                    }
                }
                else
                {
                    sourceBuffer.Append(currentChar);
                    MatchPatterns(sourceBuffer);
                    programPosition++;
                    sourcePosition++;
                    currentChar = program.Text[programPosition];
                }

            }
        }

        private int HandleComment(AcProgram program, int programPosition)
        {
            int commentStartPosition = programPosition;
            int bufferPosition = commentStartPosition;

            while (program.Text.Substring(bufferPosition, 2) != "*/")
            {
                if (program.Text.Substring(bufferPosition, 2).Contains('$'))
                {
                    Diagnostics.Lexer_ReportUnclosedComment(commentStartPosition, _programLineNumber);
                    return -1;
                }

                bufferPosition++;
            }


            System.Console.WriteLine($"Comment starting at {commentStartPosition} ends at {bufferPosition + 3}");
            System.Console.WriteLine($"Comment text: {program.Text.Substring(commentStartPosition + 2, bufferPosition - commentStartPosition - 2).Trim()}");

            return bufferPosition - commentStartPosition + 2; // calculates the length of the comment span
        }

        private void MatchPatterns(StringBuilder sourceBuffer)
        {
            // if the token found allows us to make a decision, go back through the source buffer and check for matches
            // tokens which allow us to make choices
            // - whitespace
            // - symbols
            if (char.IsWhiteSpace(sourceBuffer.ToString().LastOrDefault()))
            {
                MatchBuffer(sourceBuffer.Remove(sourceBuffer.Length - 1, 1));
            }
            else
            {
                foreach (var RegularExpression in TokenRegularExpressions)
                {
                    if (RegularExpression.Value.Match(sourceBuffer.ToString().LastOrDefault().ToString()).Success)
                    {
                        Console.WriteLine($"Token found {RegularExpression.Key}");
                        Token pushToken = new Token(RegularExpression.Key, sourceBuffer.ToString().LastOrDefault().ToString());
                        LexerTokenStream.PushToken(pushToken);
                    };
                }
            }
        }


        private bool MatchBuffer(StringBuilder sourceBuffer)
        {
            foreach (var RegularExpression in TokenRegularExpressions)
            {
                if (RegularExpression.Value.Match(sourceBuffer.ToString()).Success)
                {
                    Console.WriteLine($"token found in buffer {RegularExpression.Key}");
                    Console.WriteLine($"Removing {sourceBuffer.Length} tokens from the stream");

                    // here I need to handle the clearing of the stack of the tokens created in error (identifiers before keyword etc...)
                    for (int i = 0; i < sourceBuffer.Length; i++)
                    {
                        LexerTokenStream.PopToken();
                    }

                    LexerTokenStream.PushToken(new Token(RegularExpression.Key, sourceBuffer.ToString()));

                    sourceBuffer.Clear();
                    return true;
                }
            }

            return false;
        }

        private IEnumerable<AcProgram> ExtractPrograms()
        {
            IList<AcProgram> programs = new List<AcProgram>();

            int currentPosition = 0;
            int programStartPosition = currentPosition;

            int length = 0;

            while (currentPosition < SourceText.Length)
            {
                char currentChar = SourceText[currentPosition];

                if (currentChar != '$')
                {
                    length++;
                }
                else
                {
                    length++;
                    string programSubstring = SourceText.Substring(programStartPosition, length);
                    programs.Add(new AcProgram(programSubstring, programStartPosition, programSubstring.Length));
                    length = 0;
                    programStartPosition = currentPosition + 1;
                }

                currentPosition++;
            }

            return programs;
        }
    }

    // /* Writing a custom regular expression type, because I want to label things. */
    // class RegularExpression : Regex
    // {
    //     public RegularExpression(string pattern, TokenKind kind) : base(pattern)
    //     {
    //         Kind = kind;
    //     }
    //     public TokenKind Kind { get; }
    // }
    // class IllumiLexer
    // {
    //     /*
    //         An enum of all the token types in our grammar
    //     */
    //     public enum TokenKind
    //     {
    //         KeywordToken,
    //         EndOfFileToken,
    //         WhitespaceToken,
    //         UnrecognisedToken,
    //         LeftBrace,
    //         RightBrace,
    //         EndOfProgramToken
    //     }

    //     /*
    //         A collection containing all of the regular expressions for our grammar.
    //     */
    //     private static IList<RegularExpression> RegularExpressions = new List<RegularExpression>();

    //     private static string[] TokenRegexes =
    //     {
    //         // braces
    //         "{",
    //         "}",
    //         // keywords
    //         "print",
    //         "while",
    //         "if",
    //         // assignment
    //         "=",
    //         // 
    //         ""

    //     };
    //     private const char programSeparator = '$';

    //     public static List<TokenStream> Lex(string fileText)
    //     {
    //         foreach (string TokenRegex in TokenRegexes)
    //         {
    //             RegularExpressions.Add(new Regex(TokenRegex));
    //         }

    //         foreach (Regex rgx in RegularExpressions)
    //         {
    //             System.Console.WriteLine(rgx);
    //         }

    //         List<TokenStream> fileTokens = new List<TokenStream>();

    //         foreach (string program in extractPrograms(fileText))
    //         {
    //             if (program != string.Empty)
    //             {
    //                 System.Console.WriteLine(program);
    //                 Lexer programLexer = new Lexer(program);
    //                 TokenStream programTokens = programLexer.GetTokenStream();
    //                 fileTokens.Add(programTokens);
    //                 programLexer.diagnostics.DisplayDiagnostics();
    //             }
    //         }

    //         fileTokens.LastOrDefault().AddToken(new Token(TokenKind.EndOfFileToken, new TextSpan(fileText.Length - 1, 1)));

    //         return fileTokens;
    //     }

    //     public static List<string> extractPrograms(string text)
    //     {
    //         return text.Split(programSeparator, StringSplitOptions.RemoveEmptyEntries)
    //                                        .Select(prog => prog.Trim())
    //                                        .Select(prog => prog.Replace("\r", string.Empty))
    //                                        .ToList();
    //     }
    // }
    // class Lexer
    // {
    //     private int position;
    //     private int linePosition;
    //     private int lineNumber;
    //     public DiagnosticCollection diagnostics;
    //     public string Program { get; }
    //     public Lexer(string program)
    //     {
    //         Program = program;
    //         diagnostics = new DiagnosticCollection();
    //     }
    //     private char Current
    //     {
    //         get
    //         {
    //             if (position >= Program.Length)
    //             {
    //                 return '\0';
    //             }
    //             return Program[position];
    //         }
    //     }
    //     private void Next()
    //     {
    //         position++;
    //         linePosition++;
    //     }
    //     private Token GetNextToken()
    //     {
    //         // the order of tokens in our grammar is
    //         //    - Keywords
    //         //    - IDs
    //         //    - Symbols
    //         //    - Digits
    //         //    - Characters

    //         // increment counters etc... if a newline is encountered
    //         if (Current == '\n')
    //         {
    //             lineNumber++;
    //             linePosition = 0;
    //         }


    //         // recognise and record whitespace, then create a token for it.
    //         if (char.IsWhiteSpace(Current))
    //         {
    //             int tokenStart = linePosition;

    //             Next();

    //             while (char.IsWhiteSpace(Current))
    //             {
    //                 Next();
    //             }

    //             TextSpan span = new TextSpan(tokenStart, linePosition - tokenStart);

    //             return new Token(TokenKind.WhitespaceToken, span, Program.Substring(span.Start, span.Length));
    //         }


    //         // recognise braces and tokenize, simple since braces are single characters always
    //         if (Current == '{')
    //         {
    //             int tokenStart = linePosition;

    //             Next();

    //             TextSpan span = new TextSpan(tokenStart, 1);

    //             return new Token(TokenKind.LeftBrace, span, Program.Substring(span.Start, span.Length));
    //         }

    //         if (Current == '}')
    //         {
    //             int tokenStart = linePosition;

    //             Next();

    //             TextSpan span = new TextSpan(tokenStart, 1);

    //             return new Token(TokenKind.RightBrace, span, Program.Substring(span.Start, span.Length));
    //         }

    //         Next();

    //         return new Token(TokenKind.UnrecognisedToken, new TextSpan(linePosition, 1), Program.Substring(linePosition - 1, 1));

    //     }
    //     public TokenStream GetTokenStream()
    //     {

    //         TokenStream programTokens = new TokenStream();

    //         while (Current != '\0')
    //         {
    //             int positionBeforeToken = linePosition;

    //             Token token = GetNextToken();

    //             if (token.Type == TokenKind.UnrecognisedToken)
    //             {
    //                 diagnostics.Lexer_ReportUnrecognisedToken(new TextSpan(positionBeforeToken, linePosition - positionBeforeToken), lineNumber);
    //             }
    //             else
    //             {
    //                 programTokens.AddToken(token);
    //                 positionBeforeToken = linePosition;
    //             }

    //         }

    //         programTokens.AddToken(new Token(TokenKind.EndOfProgramToken, new TextSpan(position, 1)));

    //         lineNumber = 0;

    //         return programTokens;
    //     }
    // }
    // class Token
    // {
    //     public Token(TokenKind type, TextSpan span, string text = null)
    //     {
    //         Type = type;
    //         Span = span;
    //         Text = text;
    //     }

    //     public TokenKind Type { get; }
    //     public TextSpan Span { get; }
    //     public string Text { get; }
    // }
    // class TokenStream
    // {
    //     public List<Token> Tokens { get; }
    //     public TokenStream()
    //     {
    //         Tokens = new List<Token>();
    //     }

    //     public void AddToken(Token token)
    //     {
    //         Tokens.Add(token);
    //     }

    //     public void AddTokens(TokenStream tokens)
    //     {
    //         foreach (Token token in tokens.Tokens)
    //         {
    //             AddToken(token);
    //         }
    //     }
    // }
}

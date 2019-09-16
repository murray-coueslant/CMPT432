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

        public Stack<Token> Tokens { get; private set; }

        public void PushToken(Token token)
        {
            Tokens.Push(token);
        }

        public Token PopToken(Token token)
        {
            return Tokens.Pop();
        }
    }

    public enum TokenKind
    {
        WhitespaceToken,
        LeftBrace,
        RightBrace,
        Type_Integer,
        Type_String,
        Type_Boolean
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
        private int _lineNumber;
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
            Regex whitespaceRegex = new Regex(@"\s");
            regularExpressionDictionary.Add(TokenKind.WhitespaceToken, whitespaceRegex);

            // brace expressions
            Regex leftBraceRegex = new Regex("{");
            regularExpressionDictionary.Add(TokenKind.LeftBrace, leftBraceRegex);
            Regex rightBraceRegex = new Regex("}");
            regularExpressionDictionary.Add(TokenKind.RightBrace, rightBraceRegex);

            // keyword expressions
            // type expressions
            Regex intExpression = new Regex("int");
            regularExpressionDictionary.Add(TokenKind.Type_Integer, intExpression);
            Regex stringExpression = new Regex("string");
            regularExpressionDictionary.Add(TokenKind.Type_String, stringExpression);
            Regex boolExpression = new Regex("boolean");
            regularExpressionDictionary.Add(TokenKind.Type_Boolean, boolExpression);

            // id expression

            // symbol expressions

            // digit expression

            // character expression

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

            int programLineNumber = 0;

            char currentChar = program.Text[programPosition];

            sourceBuffer.Append(currentChar);

            while (currentChar != '$')
            {
                MatchPatterns(sourceBuffer);
                System.Console.WriteLine(currentChar);
                programPosition++;
                sourcePosition++;
                currentChar = program.Text[programPosition];
            }
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
                    programs.Add(new AcProgram(programSubstring, programStartPosition, length));
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

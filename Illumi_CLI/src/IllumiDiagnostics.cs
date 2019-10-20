using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis.Text;

namespace Illumi_CLI {
    internal class Diagnostic {
        public Diagnostic (string type, string originated, string message) {
            Type = type;
            Originated = originated;
            Message = message;
            shortDiagnostic = true;
        }

        public Diagnostic (string type, TextSpan span, string message, string originated, int lineNumber) {
            Type = type;
            Span = span;
            Message = message;
            Originated = originated;
            LineNumber = lineNumber;
            shortDiagnostic = false;
        }

        public string Type { get; }
        public TextSpan Span { get; }
        public string Message { get; }
        public bool shortDiagnostic { get; private set; }
        public string Originated { get; }
        public int LineNumber { get; }

        public override string ToString () {
            if (shortDiagnostic) {
                return $"[{Type}] - [{Originated}] -> {Message}";
            } else {
                return $"[{Type}] - [{Originated}] ({Span.Start}:{LineNumber}) -> {Message}";
            }
        }
    }

    internal class DiagnosticCollection : IEnumerable {
        private const string Error = "ERROR";
        private const string Warning = "WARNING";
        private const string Debug = "Debug";
        private const string Information = "Info";
        private const string EntryPoint = "Entry Point";
        private const string Lexer = "Lexer";
        private const string Parser = "Parser";
        private const string FileReader = "File Reader";
        private List<Diagnostic> _diagnostics = new List<Diagnostic> ();
        public int ErrorCount { get; internal set; }
        public int WarningCount { get; internal set; }
        public IEnumerator GetEnumerator () => _diagnostics.GetEnumerator ();
        IEnumerator IEnumerable.GetEnumerator () => GetEnumerator ();
        public void DisplayDiagnostics () {
            foreach (Diagnostic diag in _diagnostics) {
                if (diag.Type == Error) {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine (diag.ToString ());
                    Console.ForegroundColor = ConsoleColor.Green;
                } else if (diag.Type == Warning) {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine (diag.ToString ());
                    Console.ForegroundColor = ConsoleColor.Green;
                } else {
                    Console.WriteLine (diag.ToString ());
                }
            }

            _diagnostics = new List<Diagnostic> ();
        }
        public void AddMultiple (DiagnosticCollection diagnostics) {
            _diagnostics.AddRange (diagnostics._diagnostics);
            DisplayDiagnostics ();
        }
        public void ReportDiagnostic (string type, TextSpan span, string message, string originated, int lineNumber) {
            Diagnostic diagnostic = new Diagnostic (type, span, message, originated, lineNumber);
            _diagnostics.Add (diagnostic);
            DisplayDiagnostics ();
        }
        private void ReportDiagnostic (string type, string originated, string message) {
            Diagnostic diagnostic = new Diagnostic (type, originated, message);
            _diagnostics.Add (diagnostic);
            DisplayDiagnostics ();
        }
        internal void EntryPoint_ReportInvalidCommand () {
            string type = Error;
            string originated = EntryPoint;
            string message = "Invalid command entered. Enter 'help' or '?' to see all the available commands.";
            ReportDiagnostic (type, originated, message);
        }
        internal void Lexer_ReportLexStart (int programCounter) {
            string type = Information;
            string originated = Lexer;
            string message = $"Lexing program [ {programCounter} ].";
            ReportDiagnostic (type, originated, message);
        }
        internal void Lexer_ReportLexEnd (int programCounter) {
            string type = Information;
            string originated = Lexer;
            string message = $"Finished lexing program [ {programCounter} ]. Lex ended with [ {ErrorCount} ] error(s) and [ {WarningCount} ] warnings.";
            ReportDiagnostic (type, originated, message);
            ErrorCount = 0;
            WarningCount = 0;
        }
        internal void Lexer_LexerFindsNoTokens () {
            string type = Error;
            ErrorCount++;
            string originated = Lexer;
            string message = "No tokens found in stack.Ending lex.";
            ReportDiagnostic (type, originated, message);
        }
        public void Lexer_ReportInvalidCharacter (TextSpan span, int lineNumber, char character) {
            string type = Error;
            ErrorCount++;
            string originated = Lexer;
            string message = $"Invalid character[{ character }] found in input.See the grammar sheet for alanC to view the permitted characters";
            ReportDiagnostic (type, span, message, originated, lineNumber);
        }
        public void FileReader_ReportNoFinalEndOfProgramToken (TextSpan span, int lineNumber) {
            string type = Warning;
            WarningCount++;
            string originated = FileReader;
            string message = "Did not find a final '$' character in file. Inserting one at the last position in the file.";
            ReportDiagnostic (type, span, message, originated, lineNumber);
        }
        public void FileReader_ReportEndOfProgramInString (TextSpan span, int lineNumber) {
            string type = Error;
            ErrorCount++;
            string originated = FileReader;
            string message = "An end of program marker was found inside a string, perhaps you forgot a closing quote? ";
            ReportDiagnostic (type, span, message, originated, lineNumber);
        }
        internal void EntryPoint_MalformedCommand () {
            string type = Error;
            ErrorCount++;
            string originated = EntryPoint;
            string message = "Malformed command entered.Run 'help' or '?' to see the syntax for Illumi 's commands.";
            ReportDiagnostic (type, originated, message);
        }
        internal void Lexer_ReportMalformedComment (int startPosition, int lineNumber) {
            string type = Error;
            ErrorCount++;
            string originated = Lexer;
            string message = $"The lexer encountered a malformed comment. Try closing the comment on line {lineNumber + 1}.";
            ReportDiagnostic (type, originated, message);
        }
        internal void FileReader_ReportNoFileFound (string fileName) {
            string type = Error;
            ErrorCount++;
            string originated = FileReader;
            string message = $"No file found with the name {fileName}. Try typing a different name or correcting any mistakes.";
            ReportDiagnostic (type, originated, message);
        }
        internal void Lexer_ReportToken (Token token) {
            if (token.Kind != TokenKind.WhitespaceToken && token.Kind != TokenKind.CommentToken) {
                string type = Debug;
                string originated = Lexer;
                string message = $"Token {token.Kind.ToString ()} [ {token.Text} ] found.";
                Console.WriteLine ($"[{type}] - [{originated}] ({token.LinePosition}:{token.LineNumber}) -> {message}");
            }
        }
        internal void Lexer_ReportUnterminatedString (TextSpan span, int lineNumber) {
            string type = Error;
            ErrorCount++;
            string originated = Lexer;
            string message = "An unterminated string was encountered. Try terminating the string.";
            ReportDiagnostic (type, span, message, originated, lineNumber);
        }
        internal void Lexer_ReportInvalidCharacterInString (TextSpan span, int lineNumber, char character) {
            string type = Error;
            ErrorCount++;
            string originated = Lexer;
            string message = $"An invalid character ( {character} ) was encountered in a string. Strings may only contain lower case letters.";
            ReportDiagnostic (type, span, message, originated, lineNumber);
        }
        internal void Lexer_ReportInvalidCharacterInComment (TextSpan span, int lineNumber, char character) {
            string type = Error;
            ErrorCount++;
            string originated = Lexer;
            string message = $"An invalid character ( {character} ) was encountered in a comment. Comments may only contain lower case letters, and certain punctuation marks.";
            ReportDiagnostic (type, span, message, originated, lineNumber);
        }
        internal void Parser_ReportNoTokens () {
            string type = Error;
            ErrorCount++;
            string originated = Parser;
            string message = $"Lexer passed no tokens to the parser. Ending parse.";
            ReportDiagnostic (type, originated, message);
        }
        internal void Parser_ReportIncorrectStatement (Token token) {
            string type = Error;
            ErrorCount++;
            string originated = Parser;
            string message = $"Incorrect statement encountered at column [ {token.LinePosition} ] on line [ {token.LineNumber} ]. Entering panic recovery mode.";
            ReportDiagnostic (type, originated, message);
        }
        internal void Parser_ReportNoRemainingTokens () {
            string type = Error;
            ErrorCount++;
            string originated = Parser;
            string message = $"No tokens remaining in parse stream. Parse cannot continue.";
            ReportDiagnostic (type, originated, message);
        }
        internal void Parser_ReportUnexpectedToken (Token foundToken, TokenKind expectedKind) {
            string type = Error;
            ErrorCount++;
            string originated = Parser;
            TextSpan span = new TextSpan (foundToken.LinePosition, foundToken.Text.Length);
            string message = $"Unexpected token found in parse stream. Expected a token of type [ {expectedKind} ], but found [ {foundToken.Kind} ].";
            ReportDiagnostic (type, span, message, originated, foundToken.LineNumber);
        }
        internal void Parser_ReportPanickedToken (Token foundToken) {
            string type = Error;
            ErrorCount++;
            string originated = Parser;
            TextSpan span = new TextSpan (foundToken.LinePosition, foundToken.Text.Length);
            string message = $"Panic mode, discarding token [ {foundToken.Kind} ].";
            ReportDiagnostic (type, span, message, originated, foundToken.LineNumber);
        }
        internal void Parser_ReportStartOfParse (int programCounter) {
            string type = Information;
            string originated = Parser;
            string message = $"Parsing program [ {programCounter} ].";
            ReportDiagnostic (type, originated, message);
        }
        internal void Parser_ReportEndOfParse (int programCounter) {
            string type = Information;
            string originated = Parser;
            string message = $"Finished parsing program {programCounter}. Parse ended with [ {ErrorCount} ] errors and [ {WarningCount} ] warnings.";
            ReportDiagnostic (type, originated, message);
        }
        internal void Parser_EnteredParseStage (string stage) {
            string type = Debug;
            string originated = Parser;
            string message = $"Parser attempting to parse grammar object [ {stage} ].";
            ReportDiagnostic (type, originated, message);
        }
        internal void Parser_EncounteredLexError () {
            string type = Error;
            string originated = Parser;
            string message = "Lex error, cannot parse. Exiting.";
            ReportDiagnostic (type, originated, message);
        }
        internal void Parser_ParseEndedWithErrors () {
            string type = Information;
            string originated = Parser;
            string message = $"Parse encountered errors, discarding concrete syntax tree.";
            ReportDiagnostic (type, originated, message);
        }
        internal void Parser_ReportMatchingToken (TokenKind kind) {
            string type = Debug;
            string originated = Parser;
            string message = $"Attempting to match token of kind [ {kind} ].";
            ReportDiagnostic (type, originated, message);
        }
        internal void Parser_ReportConsumingToken (Token token) {
            string type = Debug;
            string originated = Parser;
            string message = $"Found match. Consuming token {token.Kind} [ {token.Text} ].";
            ReportDiagnostic (type, originated, message);
        }
        internal void Parser_ReportExitedPanicMode () {
            string type = Error;
            string originated = Parser;
            string message = "Leaving panic recovery mode.";
            ReportDiagnostic (type, originated, message);
        }
        internal void Parser_ReportMissingExpression (Token currentToken) {
            string type = Error;
            ErrorCount++;
            TextSpan span = new TextSpan (currentToken.LinePosition, currentToken.Text.Length);
            string originated = Parser;
            string message = $"Parser expected an expression and found [ {currentToken.Kind} ].";
            ReportDiagnostic (type, span, message, originated, currentToken.LineNumber);
        }
    }
}
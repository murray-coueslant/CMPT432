using System;
using System.Collections;
using System.Collections.Generic;
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
                Console.WriteLine (diag.ToString ());
            }

            _diagnostics = new List<Diagnostic> ();
        }

        public void AddMultiple (DiagnosticCollection diagnostics) {
            _diagnostics.AddRange (diagnostics._diagnostics);
        }

        public void ReportDiagnostic (string type, TextSpan span, string message, string originated, int lineNumber) {
            Diagnostic diagnostic = new Diagnostic (type, span, message, originated, lineNumber);
            _diagnostics.Add (diagnostic);
        }

        private void ReportDiagnostic (string type, string originated, string message) {
            Diagnostic diagnostic = new Diagnostic (type, originated, message);
            _diagnostics.Add (diagnostic);
        }

        public void Lexer_ReportInvalidIdentifier (TextSpan span, int lineNumber) {
            string type = Error;
            ErrorCount++;
            string originated = Lexer;
            string message = "Invalid identifier. Identifiers may only be single characters, a - z.";
            ReportDiagnostic (type, span, message, originated, lineNumber);
        }

        internal void EntryPoint_ReportInvalidCommand () {
            string type = Error;
            string originated = EntryPoint;
            string message = "Invalid command entered. Enter 'help' or '?' to see all the available commands.";
            ReportDiagnostic (type, originated, message);
        }

        public void Lexer_ReportUnrecognisedToken (TextSpan span, int lineNumber) {
            string type = Error;
            ErrorCount++;
            string originated = Lexer;
            string message = "Unrecognised token found. You have used a character not included in the grammar.";
            ReportDiagnostic (type, span, message, originated, lineNumber);
        }

        internal void Lexer_LexerFindsNoTokens () {
            string type = Error;
            ErrorCount++;
            string originated = Lexer;
            string message = "No tokens found in stack. Ending lex.";
            ReportDiagnostic (type, originated, message);
        }

        public void Lexer_ReportInvalidCharacter (TextSpan span, int lineNumber, char character) {
            string type = Error;
            ErrorCount++;
            string originated = Lexer;
            string message = $"Invalid character [ {character} ] found in input. See the grammar sheet for alanC to view the permitted characters.";
            ReportDiagnostic (type, span, message, originated, lineNumber);
        }

        public void FileReader_ReportNoFinalEndOfProgramToken (TextSpan span, int lineNumber) {
            string type = Warning;
            string originated = FileReader;
            string message = "Did not find a final '$' character in file. Inserting one at the last position in the file.";
            ReportDiagnostic (type, span, message, originated, lineNumber);
        }

        internal void EntryPoint_MalformedCommand () {
            string type = Error;
            ErrorCount++;
            string originated = EntryPoint;
            string message = "Malformed command entered. Run 'help' or '?' to see the syntax for Illumi's commands.";
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
            string originated = FileReader;
            string message = $"No file found with the name {fileName}. Try typing a different name or correcting any mistakes.";
            ErrorCount++;
            ReportDiagnostic (type, originated, message);
        }

        internal void Lexer_ReportToken (Token token) {
            if (token.Kind != TokenKind.WhitespaceToken && token.Kind != TokenKind.CommentToken) {
                string type = Debug;
                string originated = Lexer;
                string message = $"Token {token.Kind.ToString()} [ {token.Text} ] found.";
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
            string originated = Parser;
            string message = $"Lexer returned no tokens to the parser. Ending parse.";
            ErrorCount++;
            ReportDiagnostic (type, originated, message);
        }

        internal void Parser_ReportIncorrectStatement (Token token) {
            string type = Error;
            string originated = Parser;
            string message = $"Incorrect statement encountered at column [ {token.LinePosition} ] on line [ {token.LineNumber} ]. Entering panic recovery mode.";
            ErrorCount++;
            ReportDiagnostic (type, originated, message);
        }

        internal void Parser_ReportNoRemainingTokens () {
            string type = Error;
            string originated = Parser;
            string message = $"No tokens remaining in parse stream. Parse cannot continue.";
            ErrorCount++;
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
            string type = Warning;
            WarningCount++;
            string originated = Parser;
            TextSpan span = new TextSpan (foundToken.LinePosition, foundToken.Text.Length);
            string message = $"Panic mode, discarding token [ {foundToken.Kind} ].";
            ReportDiagnostic (type, span, message, originated, foundToken.LineNumber);
        }

        internal void Parser_ReportParseBeginning () {
            string type = Information;
            string originated = Parser;
            string message = $"Beginning parse.";
            ReportDiagnostic (type, originated, message);
        }

        internal void Parser_ReportEndOfParse (int programCount) {
            string type = Information;
            string originated = Parser;
            string message = $"Finished parsing program {programCount}. Parse ended with [ {ErrorCount} ] errors and [ {WarningCount} ] warnings.";
            ReportDiagnostic (type, originated, message);
        }

        internal void Parser_EnteredParseStage (string stage) {
            string type = Debug;
            string originated = Parser;
            string message = $"Parser attempting to parse grammar object [ {stage} ].";
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
    }
}
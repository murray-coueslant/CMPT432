using System;
using Microsoft.CodeAnalysis.Text;
using System.Collections;
using System.Collections.Generic;

namespace Illumi_CLI
{
    internal class Diagnostic
    {
        public Diagnostic(string type, string originated, string message)
        {
            Type = type;
            Originated = originated;
            Message = message;
            shortDiagnostic = true;
        }

        public Diagnostic(string type, TextSpan span, string message, string originated, int lineNumber)
        {
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

        public override string ToString()
        {
            if (shortDiagnostic)
            {
                return $"[{Type}] - [{Originated}] -> {Message}";
            }
            else
            {
                return $"[{Type}] - [{Originated}] ({Span.Start}:{LineNumber}) -> {Message}";
            }
        }
    }

    internal class DiagnosticCollection : IEnumerable
    {
        private const string Error = "ERROR";
        private const string Warning = "WARNING";
        private const string Debug = "Debug";
        private const string EntryPoint = "Entry Point";
        private const string Lexer = "Lexer";
        private const string FileReader = "File Reader";
        private List<Diagnostic> _diagnostics = new List<Diagnostic>();
        public int ErrorCount { get; internal set; }
        public int WarningCount { get; internal set; }

        public IEnumerator GetEnumerator() => _diagnostics.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void DisplayDiagnostics()
        {
            foreach (Diagnostic diag in _diagnostics)
            {
                Console.WriteLine(diag.ToString());
            }

            _diagnostics = new List<Diagnostic>();
        }

        public void AddMultiple(DiagnosticCollection diagnostics)
        {
            _diagnostics.AddRange(diagnostics._diagnostics);
        }

        public void ReportDiagnostic(string type, TextSpan span, string message, string originated, int lineNumber)
        {
            Diagnostic diagnostic = new Diagnostic(type, span, message, originated, lineNumber);
            _diagnostics.Add(diagnostic);
        }

        private void ReportDiagnostic(string type, string originated, string message)
        {
            Diagnostic diagnostic = new Diagnostic(type, originated, message);
            _diagnostics.Add(diagnostic);
        }

        public void Lexer_ReportInvalidIdentifier(TextSpan span, int lineNumber)
        {
            string type = Error;
            ErrorCount++;
            string originated = Lexer;
            string message = "Invalid identifier. Identifiers may only be single characters, a - z.";
            ReportDiagnostic(type, span, message, originated, lineNumber);
        }

        internal void EntryPoint_ReportInvalidCommand()
        {
            string type = Error;
            string originated = EntryPoint;
            string message = "Invalid command entered. Enter 'help' or '?' to see all the available commands.";
            ReportDiagnostic(type, originated, message);
        }

        public void Lexer_ReportUnrecognisedToken(TextSpan span, int lineNumber)
        {
            string type = Error;
            ErrorCount++;
            string originated = Lexer;
            string message = "Unrecognised token found. You have used a character not included in the grammar.";
            ReportDiagnostic(type, span, message, originated, lineNumber);
        }

        public void Lexer_ReportInvalidCharacter(TextSpan span, int lineNumber, char character)
        {
            string type = Error;
            ErrorCount++;
            string originated = Lexer;
            string message = $"Invalid character [ {character} ] found in input. See the grammar sheet for alanC to view the permitted characters.";
            ReportDiagnostic(type, span, message, originated, lineNumber);
        }

        public void FileReader_ReportNoFinalEndOfProgramToken(TextSpan span, int lineNumber)
        {
            string type = Warning;
            string originated = FileReader;
            string message = "Did not find a final '$' character in file. Inserting one at the last position in the file.";
            ReportDiagnostic(type, span, message, originated, lineNumber);
        }

        internal void EntryPoint_MalformedCommand()
        {
            string type = Error;
            ErrorCount++;
            string originated = EntryPoint;
            string message = "Malformed command entered. Run 'help' or '?' to see the syntax for Illumi's commands.";
            ReportDiagnostic(type, originated, message);
        }

        internal void Lexer_ReportMalformedComment(int startPosition, int lineNumber)
        {
            string type = Error;
            ErrorCount++;
            string originated = Lexer;
            string message = $"The lexer encountered a malformed comment. Try closing the comment on line {lineNumber + 1}.";
            ReportDiagnostic(type, originated, message);
        }

        internal void FileReader_ReportNoFileFound(string fileName)
        {
            string type = Error;
            string originated = FileReader;
            string message = $"No file found with the name {fileName}. Try typing a different name or correcting any mistakes.";
            ReportDiagnostic(type, originated, message);
        }

        internal void Lexer_ReportToken(TokenKind kind, string text, int tokenStart, int lineNumber)
        {
            if (kind != TokenKind.WhitespaceToken)
            {
                string type = Debug;
                string originated = Lexer;
                string message = $"Token {kind.ToString()} [ {text} ] found.";
                Console.WriteLine($"[{type}] - [{originated}] ({tokenStart}:{lineNumber}) -> {message}");
            }
        }

        internal void Lexer_ReportUnterminatedString(TextSpan span, int lineNumber)
        {
            string type = Error;
            ErrorCount++;
            string originated = Lexer;
            string message = "An unterminated string was encountered. Try terminating the string.";
            ReportDiagnostic(type, span, message, originated, lineNumber);
        }

        internal void Lexer_ReportInvalidCharacterInString(TextSpan span, int lineNumber, char character)
        {
            string type = Error;
            ErrorCount++;
            string originated = Lexer;
            string message = $"An invalid character ({character}) was encountered in a string. Strings may only contain letters.";
            ReportDiagnostic(type, span, message, originated, lineNumber);
        }

        internal void Lexer_ReportInvalidCharacterInComment(TextSpan span, int lineNumber, char character)
        {
            string type = Error;
            ErrorCount++;
            string originated = Lexer;
            string message = $"An invalid character ({character}) was encountered in a comment. Comments may only contain letters, and certain punctuation marks.";
            ReportDiagnostic(type, span, message, originated, lineNumber);
        }
    }
}
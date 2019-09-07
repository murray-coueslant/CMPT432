using System;
using Microsoft.CodeAnalysis.Text;
using System.Collections;
using System.Collections.Generic;

namespace Illumi_CLI
{
    internal class Diagnostic
    {
        public Diagnostic(string type, TextSpan span, string message, string originated, int lineNumber)
        {
            Type = type;
            Span = span;
            Message = message;
            Originated = originated;
            LineNumber = lineNumber;
        }

        public string Type { get; }
        public TextSpan Span { get; }
        public string Message { get; }
        public string Originated { get; }
        public int LineNumber { get; }

        public override string ToString() => $"[{Type}] - [{Originated}] ({Span.Start}:{LineNumber}) : {Message}";
    }

    internal class DiagnosticCollection : IEnumerable
    {
        private List<Diagnostic> _diagnostics = new List<Diagnostic>();

        public IEnumerator GetEnumerator() => _diagnostics.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void DisplayDiagnostics()
        {
            foreach (Diagnostic diag in _diagnostics)
            {
                Console.WriteLine(diag.ToString());
            }
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

        public void Lexer_ReportInvalidIdentifier(TextSpan span, int lineNumber)
        {
            string type = "ERROR";
            string originated = "Lexer";
            string message = "Invalid identifier. Identifiers may only be single characters, a - z.";
            ReportDiagnostic(type, span, message, originated, lineNumber);
        }

        public void FileReader_ReportNoFinalEndOfProgramToken(TextSpan span, int lineNumber)
        {
            string type = "WARNING";
            string originated = "File Reader";
            string message = "Did not find a final '$' character in file. Inserting one at the last position.";
            ReportDiagnostic(type, span, message, originated, lineNumber);
        }
    }
}
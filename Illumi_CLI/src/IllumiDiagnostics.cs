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
                return $"[ {Type} ] - [ {Originated} ] -> {Message}";
            } else {
                return $"[ {Type} ] - [ {Originated}]  ( {Span.Start} : {LineNumber} ) -> {Message}";
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
        private const string Semantic = "Semantic Analyser";
        private const string FileReader = "File Reader";
        private const string Tree = "Tree";
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
            string message = "No tokens found in stack. Ending lex.";
            ReportDiagnostic (type, originated, message);
        }
        public void Lexer_ReportInvalidCharacter (TextSpan span, int lineNumber, char character) {
            string type = Error;
            ErrorCount++;
            string originated = Lexer;
            string message = $"Invalid character [ { character } ] found in input. See the grammar sheet for alanC to view the permitted characters.";
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
                Console.WriteLine ($"[ {type} ] - [ {originated} ] ( {token.LinePosition} : {token.LineNumber} ) -> {message}");
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
            string message = $"Lexer passed no tokens to the parser. Check if you had any lexical errors.";
            ReportDiagnostic (type, originated, message);
        }
        internal void Parser_ReportIncorrectStatement (Token token) {
            string type = Error;
            ErrorCount++;
            string originated = Parser;
            string message = $"Incorrect statement encountered at column [ {token.LinePosition} ] on line [ {token.LineNumber} ] due to token [ {token.Text} ]. Entering panic recovery mode.";
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
            string message = $"Finished parsing program [ {programCounter} ]. Parse ended with [ {ErrorCount} ] errors and [ {WarningCount} ] warnings.";
            ErrorCount = 0;
            WarningCount = 0;
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
            string message = $"Found match. Consuming token [ {token.Kind} ] ( {token.Text} ).";
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
        internal void Semantic_EncounteredParseError () {
            string type = Error;
            string originated = Semantic;
            string message = "Parse error, cannot analyse. Exiting.";
            ReportDiagnostic (type, originated, message);
        }
        internal void Semantic_ReportStartOfSemantic (int programCounter) {
            string type = Information;
            string originated = Semantic;
            string message = $"Analysing program [ {programCounter} ].";
            ReportDiagnostic (type, originated, message);
        }
        internal void Semantic_ReportEndOfSemantic (int programCounter) {
            string type = Information;
            string originated = Semantic;
            string message = $"Finished analysing program [ {programCounter} ]. Semantic analysis ended with [ {ErrorCount} ] errors and [ {WarningCount} ] warnings.";
            ReportDiagnostic (type, originated, message);
            ErrorCount = 0;
            WarningCount = 0;
        }
        internal void Semantic_ParserGaveNoTree () {
            string type = Error;
            ErrorCount++;
            string originated = Semantic;
            string message = "The parser handed a null tree to the semantic analyser, cannot analyse. Perhaps you encountered a lex or parse error?";
            ReportDiagnostic (type, originated, message);
        }
        internal void Semantic_ReportAddingSymbol (string symbol, string symbolType, int scope) {
            string type = Information;
            string originated = Semantic;
            string message = $"Adding symbol [ {symbol} ] of type [ {symbolType} ] to symbol table for scope [ {scope} ].";
            ReportDiagnostic (type, originated, message);
        }
        internal void Semantic_ReportSymbolAlreadyDeclared (string symbol, int currentScope, TextSpan span, int lineNumber, string symbolType) {
            string type = Error;
            ErrorCount++;
            string originated = Semantic;
            string message = $"Symbol [ {symbol} ] of type [ {symbolType} ] has already been declared in scope [ {currentScope} ].";
            ReportDiagnostic (type, span, message, originated, lineNumber);
        }
        internal void Semantic_ReportAscendingScope (Scope currentScope) {
            string type = Information;
            string originated = Semantic;
            string message = $"Ascending from scope [ {currentScope.Level} ] to [ {currentScope.ParentScope.Level} ].";
            ReportDiagnostic (type, originated, message);
        }
        internal void Semantic_ReportReachedRootScope () {
            string type = Information;
            string originated = Semantic;
            string message = "Attempting to ascend from a scope with no parent, analyser has reached the root scope.";
            ReportDiagnostic (type, originated, message);
        }
        internal void Semantic_ReportDisplayingSymbolTables () {
            string type = Information;
            string originated = Semantic;
            string message = $"Displaying symbol tables for the current program.";
            ReportDiagnostic (type, originated, message);
        }
        internal void Semantic_ReportUndeclaredIdentifier (Token identifier, int scopeLevel) {
            string type = Error;
            ErrorCount++;
            string originated = Semantic;
            TextSpan span = new TextSpan (identifier.LinePosition, 1);
            string message = $"The identifier [ {identifier.Text} ] was used before being declared in scope [ {scopeLevel} ].";
            ReportDiagnostic (type, span, message, originated, identifier.LineNumber);
        }
        internal void Semantic_ReportSymbolLookup (string symbol) {
            string type = Information;
            string originated = Semantic;
            string message = $"Attempting to find [ {symbol} ] in symbol table.";
            ReportDiagnostic (type, originated, message);
        }
        internal void Semantic_ReportFoundSymbol (string symbol, Scope foundScope) {
            string type = Information;
            string originated = Semantic;
            string message = $"Found symbol [ {symbol} ] in scope [ {foundScope.Level} ]";
            ReportDiagnostic (type, originated, message);
        }
        internal void Semantic_ReportSymbolNotFound (string symbol, Scope searchScope) {
            string type = Information;
            string originated = Semantic;
            string message = $"Could not find symbol [ {symbol} ] in scope [ {searchScope.Level} ]. Searching [ {searchScope.ParentScope.Level} ]";
            ReportDiagnostic (type, originated, message);
        }
        internal void Semantic_ReportCheckingScope () {
            string type = Information;
            string originated = Semantic;
            string message = $"Checking scope.";
            ReportDiagnostic (type, originated, message);
        }
        internal void Semantic_ReportCheckingType () {
            string type = Information;
            string originated = Semantic;
            string message = $"Checking type.";
            ReportDiagnostic (type, originated, message);
        }
        internal void Tree_ReportAscendingLevel (ASTNode node) {
            string type = Information;
            string originated = Tree;
            string message = $"Ascending from node [ {node.Token.Text} ] to [ {node.Parent.Token.Text} ].";
            ReportDiagnostic (type, originated, message);
        }
        internal void Semantic_ReportInvalidType (Token token) {
            string type = Error;
            ErrorCount++;
            string originated = Semantic;
            TextSpan span = new TextSpan (token.LinePosition, token.Text.Length);
            string message = $"An invalid type declaration was encountered due to token [ {token.Text} ].";
            ReportDiagnostic (type, span, message, originated, token.LineNumber);
        }
        internal void Semantic_ReportAddingASTNode (string nodeType) {
            string type = Information;
            string originated = Semantic;
            string message = $"Adding node of type [ {nodeType} ] to AST.";
            ReportDiagnostic (type, originated, message);
        }
        internal void Semantic_ReportDisplayingAST () {
            string type = Information;
            string originated = Semantic;
            string message = "No semantic errors, Displaying AST.";
            ReportDiagnostic (type, originated, message);
        }
        internal void Semantic_ReportScopeError () {
            string type = Warning;
            string originated = Semantic;
            string message = "Scope errors encountered, will not check type.";
            ReportDiagnostic (type, originated, message);
        }
        internal void Semantic_ReportMatchedTypes (ASTNode node) {
            string type = Information;
            string originated = Semantic;
            TextSpan span = new TextSpan (node.Token.LinePosition, node.Token.Text.Length);
            string message = $"The types for operation [ {node.Token.Text} ] match.";
            ReportDiagnostic (type, span, message, originated, node.Token.LineNumber);
        }
        internal void Semantic_ReportTypeMismatch (ASTNode node) {
            string type = Error;
            ErrorCount++;
            string originated = Semantic;
            TextSpan span = new TextSpan (node.Token.LinePosition, node.Token.Text.Length);
            string message = $"Type mismatch around operation [ {node.Token.Text} ].";
            ReportDiagnostic (type, span, message, originated, node.Token.LineNumber);
        }
    }
}
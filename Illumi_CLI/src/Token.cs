namespace Illumi_CLI {
    class Token {
        public Token (TokenKind kind, string text, int lineNumber, int linePosition) {
            Kind = kind;
            Text = text;
            LineNumber = lineNumber;
            LinePosition = linePosition;
        }

        public TokenKind Kind { get; }
        public string Text { get; }
        public int LineNumber { get; }
        public int LinePosition { get; }
    }
}
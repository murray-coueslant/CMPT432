using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Illumi_CLI
{
    class RegularExpressions
    {
        public Regex identifierRegex = new Regex("[a-z]");
        public Regex digitRegex = new Regex("[1-9]");
        public Regex symbolsRegex = new Regex("[{}()\" ==!=\\+]");
        public Regex whitespaceRegex = new Regex(@"\s");

        public IList<RegexMap> Specials;
        public IList<RegexMap> Symbols;
        public IList<RegexMap> Keywords;

        public RegularExpressions()
        {
            /*
                Build out the set of special regexes
            */
            Specials.Add(new RegexMap(TokenKind.StartCommentToken, new Regex("/*")));
            Specials.Add(new RegexMap(TokenKind.EndCommentToken, new Regex("*/")));
            Specials.Add(new RegexMap(TokenKind.IdentifierToken, identifierRegex));
            Specials.Add(new RegexMap(TokenKind.DigitToken, digitRegex));
            Specials.Add(new RegexMap(TokenKind.SymbolToken, symbolsRegex));
            Specials.Add(new RegexMap(TokenKind.WhitespaceToken, whitespaceRegex));
            /*
                Build out the set of symbol regexes
            */
            Symbols.Add(new RegexMap(TokenKind.LeftBraceToken, new Regex("{")));
            Symbols.Add(new RegexMap(TokenKind.RightBraceToken, new Regex("}")));
            Symbols.Add(new RegexMap(TokenKind.LeftParenthesisToken, new Regex("(")));
            Symbols.Add(new RegexMap(TokenKind.RightParenthesisToken, new Regex(")")));
            Symbols.Add(new RegexMap(TokenKind.SpaceToken, new Regex(" ")));
            Symbols.Add(new RegexMap(TokenKind.AssignmentToken, new Regex("=")));
            Symbols.Add(new RegexMap(TokenKind.EquivalenceToken, new Regex("==")));
            Symbols.Add(new RegexMap(TokenKind.QuoteToken, new Regex('"'.ToString())));
            Symbols.Add(new RegexMap(TokenKind.AdditionToken, new Regex(@"\+")));
            /*
                Build out the set of keyword regexes
            */
            Keywords.Add(new RegexMap(TokenKind.WhileToken, new Regex(@"\bwhile\b")));
            Keywords.Add(new RegexMap(TokenKind.PrintToken, new Regex(@"\bprint\b")));
            Keywords.Add(new RegexMap(TokenKind.Type_IntegerToken, new Regex(@"\bint\b")));
            Keywords.Add(new RegexMap(TokenKind.Type_StringToken, new Regex(@"\bstring\b")));
            Keywords.Add(new RegexMap(TokenKind.Type_BooleanToken, new Regex(@"\bboolean\b")));
            Keywords.Add(new RegexMap(TokenKind.TrueToken, new Regex(@"\btrue\b")));
            Keywords.Add(new RegexMap(TokenKind.FalseToken, new Regex(@"\bfalse\b")));
            Keywords.Add(new RegexMap(TokenKind.IfToken, new Regex(@"\bif\b")));
        }
    }

    class RegexMap
    {
        public TokenKind Kind { get; }
        public Regex Regex { get; }

        public RegexMap(TokenKind kind, Regex regex)
        {
            Kind = kind;
            Regex = regex;
        }

    }
}
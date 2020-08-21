using System;
namespace Illumi_CLI {
    class Symbol {
        public Token Token { get; set; }
        public string Type { get; set; }
        public Boolean Used { get; set; }
        public Boolean Initialized { get; set; }
        public Symbol (Token token, string type, Boolean used = false, Boolean initialized = false) {
            Token = token;
            Type = type;
            Used = used;
            Initialized = initialized;
        }
    }
}
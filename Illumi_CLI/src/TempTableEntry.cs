namespace Illumi_CLI {
    class TempTableEntry {
        public string TempAddress { get; set; }
        public string Var { get; set; }
        public string Type { get; set; }
        public int Scope { get; set; }
        public int Offset { get; set; }

        public TempTableEntry (string tempAddress, string var, string type, int scope, int offset) {
            TempAddress = tempAddress;
            Var = var;
            Type = type;
            Scope = scope;
            Offset = offset;
        }

    }
}
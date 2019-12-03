namespace Illumi_CLI {
    class TempTableEntry {
        public string Address { get; set; }
        public string Var { get; set; }
        public string Type { get; set; }
        public int Scope { get; set; }
        public string Value { get; set; }
        public int Offset { get; set; }

        public TempTableEntry (string tempAddress, string var, string type, int scope, string value, int offset = 0) {
            Address = tempAddress;
            Var = var;
            Type = type;
            Scope = scope;
            Value = value;
            Offset = offset;
        }

        public void BackPatch (string finalAddress) {
            Address = finalAddress;
        }
    }
}
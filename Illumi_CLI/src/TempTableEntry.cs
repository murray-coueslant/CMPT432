namespace Illumi_CLI {
    class TempTableEntry {
        public string Address { get; set; }
        public string Var { get; set; }
        public string Type { get; set; }
        public int Scope { get; set; }
        public int Offset { get; set; }

        public TempTableEntry (string tempAddress, string var, string type, int scope, int offset = 0) {
            Address = tempAddress;
            Var = var;
            Type = type;
            Scope = scope;
            Offset = offset;
        }

        public void BackPatch (string finalAddress) {
            Address = finalAddress;
        }
    }
}
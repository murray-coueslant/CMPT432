namespace Illumi_CLI {
    class RuntimeImage {
        public string[, ] Bytes { get; set; }
        int CurrentRow { get; set; }
        int CurrentCol { get; set; }
        public RuntimeImage () {
            CurrentRow = 0;
            CurrentCol = 0;
            Bytes = new string[32, 8];
        }

        public void WriteByte (string newByte) {
            Bytes[CurrentRow, CurrentCol] = newByte;
            if (CurrentCol + 1 > 7) {
                if (CurrentRow + 1 > 31) {
                    // todo Diagnostics.CodeGen_ReportRuntimeImageOverflow();
                    return;
                } else {
                    CurrentRow++;
                    CurrentCol = 0;
                }
            } else {
                CurrentCol++;
            }
        }
    }
}
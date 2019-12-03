using System.Reflection;
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

        public string GetCurrentAddress () {
            int currentByte = (CurrentRow * 8) + CurrentCol;
            string hexVal = currentByte.ToString ("X");
            if (hexVal.Length == 2) {
                return $"{hexVal} 00";
            } else {
                return $"0{hexVal} 00";
            }
        }

        public void BackPatch (TempTableEntry variable, string address) {
            string[] splitAddress = address.Split (" ");
            for (int i = 0; i < Bytes.GetLength (0); i++) {
                for (int j = 0; j < Bytes.GetLength (1); j++) {
                    if (j + 1 < Bytes.GetLength (1)) {
                        if ($"{Bytes[i,j]} {Bytes[i, j+1]}" == variable.Address) {
                            Bytes[i, j] = splitAddress[0];
                            Bytes[i, j + 1] = splitAddress[1];
                        }
                    } else {
                        if (i + 1 < Bytes.GetLength (0)) {
                            if ($"{Bytes[i,j]} {Bytes[i + 1, 0]}" == variable.Address) {
                                Bytes[i, j] = splitAddress[0];
                                Bytes[i + 1, 0] = splitAddress[1];
                            }
                        }
                    }
                }
            }
            variable.BackPatch (address);
        }
    }
}
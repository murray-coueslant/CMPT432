using System;
using System.Collections.Generic;
using System.Linq;
namespace Illumi_CLI {
    class TempTable {
        public List<TempTableEntry> Rows { get; set; }
        string NextTempAddress {
            get {
                return $"T{Rows.Count} xx";
            }
        }
        int Offset {
            get {
                if (Rows.Count != 0) {
                    TempTableEntry prevEntry = GetTempTableEntry ($"T{Rows.Count - 1} xx");
                    if (prevEntry.Type == "int" || prevEntry.Type == "boolean") {
                        return 1;
                    } else {
                        return 2;
                    }
                } else {
                    return 0;
                }
            }
        }
        public TempTable (List<TempTableEntry> rows = null) {
            if (rows is null) {
                Rows = new List<TempTableEntry> ();
            } else {
                Rows = rows;
            }
        }
        public TempTableEntry GetTempTableEntry (string tempAddress) {
            return Rows.Where (r => r.TempAddress == tempAddress).ToList ().FirstOrDefault ();
        }
        public void NewEntry (string var, string type, int scope) {
            Rows.Add (new TempTableEntry (NextTempAddress, var, type, scope, Offset));
        }
    }
}
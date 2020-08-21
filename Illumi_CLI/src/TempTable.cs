using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
namespace Illumi_CLI {
    class TempTable {
        public List<TempTableEntry> Rows { get; set; }
        public TempTableEntry MostRecentEntry { get; set; }
        public bool Heap { get; set; }
        string NextTempAddress {
            get {
                if (!Heap) {
                    return $"T{Rows.Count} xx";
                } else {
                    return $"H{Rows.Count} xx";
                }
            }
        }
        int Offset {
            get {
                TempTableEntry prevEntry;
                if (!Heap) {
                    prevEntry = GetTempTableEntry ($"T{Rows.Count - 1} xx");
                } else {
                    prevEntry = GetTempTableEntry ($"H{Rows.Count - 1} xx");
                }
                if (prevEntry.Type == "int" || prevEntry.Type == "boolean") {
                    return 1;
                } else {
                    return 2;
                }
            }
        }
        public TempTable (List<TempTableEntry> rows = null, bool heap = false) {
            if (rows is null) {
                Rows = new List<TempTableEntry> ();
            } else {
                Rows = rows;
            }
            Heap = heap;
        }
        public TempTableEntry GetTempTableEntry (string tempAddress) {
            return Rows.Where (r => r.Address == tempAddress).ToList ().FirstOrDefault ();
        }
        public TempTableEntry GetTempTableEntry (string variableName, int variableScope) {
            return Rows.Where (r => r.Var == variableName && r.Scope == variableScope).ToList ().FirstOrDefault ();
        }
        public void NewStaticEntry (string var, string value, string type, int scope) {
            Rows.Add (new TempTableEntry (NextTempAddress, var, type, scope, value));
            Rows.Last ().Offset = Offset;
            MostRecentEntry = Rows.Last ();
        }
        public void NewHeapEntry (string var, int scope) { }
    }
}
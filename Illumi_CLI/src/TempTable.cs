using System;
using System.Collections.Generic;
using System.Linq;
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
            return Rows.Where (r => r.TempAddress == tempAddress).ToList ().FirstOrDefault ();
        }
        public void NewStaticEntry (string var, string type, int scope) {
            Rows.Add (new TempTableEntry (NextTempAddress, var, type, scope));
            Rows.Last ().Offset = Offset;
            MostRecentEntry = Rows.Last ();
        }
        public void NewHeapEntry (string data) {
            // todo work out the pointer logic for this and make it nice
        }
    }
}
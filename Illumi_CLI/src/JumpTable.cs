using System.Collections.Generic;
using System.Linq;
namespace Illumi_CLI {
    class JumpTable {
        public List<JumpTableEntry> Jumps { get; set; }
        public JumpTable () {
            Jumps = new List<JumpTableEntry> ();
        }

        public JumpTableEntry NewJumpEntry () {
            string name = $"J{Jumps.Count}";
            Jumps.Add (new JumpTableEntry (name));
            return GetJumpTableEntry (name);
        }

        public JumpTableEntry GetJumpTableEntry (string name) {
            return Jumps.Where (j => j.Name == name).FirstOrDefault ();
        }
    }
}
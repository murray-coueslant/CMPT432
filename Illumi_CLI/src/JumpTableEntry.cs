using System;
namespace Illumi_CLI {

    class JumpTableEntry {
        public int JumpLength { get; set; }
        public string Name { get; set; }
        public JumpTableEntry (string name = "", int jumpLength = 0) {
            Name = name;
            JumpLength = jumpLength;
        }
    }
}
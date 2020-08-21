using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Text;

namespace Illumi_CLI {
    class ConcreteSyntaxTree : Tree {
        public ConcreteSyntaxTree (CSTNode root = null) : base (root) { }

        public void DisplayCST () {
            PrintTree (Root, "");
        }
    }
}
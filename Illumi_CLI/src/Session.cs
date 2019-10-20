using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis.Text;

namespace Illumi_CLI {
    class Session {
        public Session () {
            Diagnostics = new DiagnosticCollection ();
            debugMode = true;
            setupMode = false;
        }

        public bool setupMode { get; set; }
        public bool debugMode { get; private set; }
        public DiagnosticCollection Diagnostics { get; private set; }

        internal void setDebugMode () {
            debugMode = !debugMode;

            Console.WriteLine ($"Debug mode changed from {(!debugMode).ToString().ToUpper()} to {debugMode.ToString().ToUpper()}.");
        }
    }

}
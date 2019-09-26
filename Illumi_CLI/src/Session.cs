using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis.Text;

namespace Illumi_CLI
{
    class Session
    {
        public bool setupMode;
        public bool verboseMode;

        public Session()
        {
            Diagnostics = new DiagnosticCollection();
            debugMode = true;
            verboseMode = true;
        }

        public bool debugMode { get; private set; }
        public DiagnosticCollection Diagnostics { get; private set; }

        internal void setDebugMode()
        {
            debugMode = !debugMode;

            Console.WriteLine($"Debug mode changed from {(!debugMode).ToString().ToUpper()} to {debugMode.ToString().ToUpper()}.");
        }

        internal void setVerboseMode()
        {
            verboseMode = !verboseMode;

            Console.WriteLine($"Verbose output mode changed from {(!verboseMode).ToString().ToUpper()} to {verboseMode.ToString().ToUpper()}.");
        }
    }

}

using System.Linq;
using System;
using System.IO;
using Microsoft.CodeAnalysis.Text;

namespace Illumi_CLI
{
    internal class IllumiFileReader
    {
        internal static string ReadFile(FileInfo filePath, DiagnosticCollection diagnostics)
        {
            if (filePath.Exists)
            {
                string fullFileText = File.ReadAllText(filePath.FullName);

                if (fullFileText[fullFileText.Length - 1] != '$')
                {
                    TextSpan span = new TextSpan(fullFileText.Length - 1, 1);
                    int lineNumber = fullFileText.Count(c => c == '\n') + 1;
                    diagnostics.FileReader_ReportNoFinalEndOfProgramToken(span, lineNumber);
                    return fullFileText + "$";
                }
                return fullFileText;
            }
            else
            {
                // IllumiErrorReporter.SendError($"Could not find the specified file ({filePath.Name}), correct the path and try again.");
                return null;
            }
        }
    }
}
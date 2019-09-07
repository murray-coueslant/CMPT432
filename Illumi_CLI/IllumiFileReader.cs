using System.Linq;
using System;
using System.IO;

namespace Illumi_CLI
{
    internal class IllumiFileReader
    {
        internal static string ReadFile(FileInfo filePath)
        {
            if (filePath.Exists)
            {
                string fullFileText = File.ReadAllText(filePath.FullName);

                if (fullFileText[fullFileText.Length - 1] != '$')
                {
                    TextSpan warningSpan = new TextSpan(fullFileText.Length - 1, 1);
                    int lineNumber = fullFileText.Count(c => c == '\n');
                    diagnostics.FileReader_ReportNoFinalEndOfProgramToken(span, lineNumber);
                    return fullFileText + "$";
                }
                return fullFileText;
            }
            else
            {
                // IllumiErrorReporter.SendError($"Could not find the specified file ({filePath.Name}), correct the path and try again.");
                // return null;
            }
        }
    }
}
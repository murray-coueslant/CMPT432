using System.Linq;
using System;
using System.IO;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;

namespace Illumi_CLI
{
    internal class IllumiFileReader
    {
        internal static IList<string> ReadFile(FileInfo filePath, Session currentSession)
        {
            if (filePath.Exists)
            {
                string fullFileText = File.ReadAllText(filePath.FullName);

                if (fullFileText[fullFileText.Length - 1] != '$')
                {
                    TextSpan span = new TextSpan(fullFileText.Length - 1, 1);
                    int lineNumber = fullFileText.Count(c => c == '\n') + 1;
                    currentSession.Diagnostics.FileReader_ReportNoFinalEndOfProgramToken(span, lineNumber);
                    currentSession.Diagnostics.WarningCount++;
                    fullFileText = fullFileText + "$";
                }

                return ExtractPrograms(fullFileText.Replace("\r", string.Empty));
            }
            else
            {

                currentSession.Diagnostics.FileReader_ReportNoFileFound(filePath.Name);
                return new List<string>();
            }
        }

        private static IList<string> ExtractPrograms(string sourceText)
        {
            IList<string> programs = new List<string>();

            int currentPosition = 0;
            int programStartPosition = currentPosition;

            bool inQuotes = false;

            int length = 0;

            while (currentPosition < sourceText.Length)
            {
                char currentChar = sourceText[currentPosition];

                if (currentChar == '"')
                {
                    inQuotes = !inQuotes;
                }

                if (currentChar != '$' || (currentChar == '$' && inQuotes))
                {
                    length++;
                }
                else
                {
                    length++;
                    string programSubstring = sourceText.Substring(programStartPosition, length).Trim();
                    programs.Add(programSubstring);
                    length = 0;
                    programStartPosition = currentPosition + 1;
                }

                currentPosition++;
            }

            string programSubstring2 = sourceText.Substring(programStartPosition, length).Trim();
            programs.Add(programSubstring2);

            return programs;
        }
    }
}
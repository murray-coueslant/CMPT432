using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis.Text;

namespace Illumi_CLI {
    internal class IllumiFileReader {
        internal static IList<string> ReadFile (FileInfo filePath, Session currentSession) {
            if (filePath.Exists) {
                string fullFileText = File.ReadAllText (filePath.FullName);
                if (fullFileText != string.Empty) {
                    if (fullFileText[fullFileText.Length - 1] != '$') {
                        TextSpan span = new TextSpan (fullFileText.Length - 1, 1);
                        int lineNumber = fullFileText.Count (c => c == '\n') + 1;
                        currentSession.Diagnostics.FileReader_ReportNoFinalEndOfProgramToken (span, lineNumber);
                        currentSession.Diagnostics.WarningCount++;
                        currentSession.Diagnostics.DisplayDiagnostics ();
                        fullFileText = fullFileText + "$";
                    }

                    return ExtractPrograms (fullFileText.Replace ("\r", string.Empty), currentSession);
                } else {
                    // todo Diagnostics.FileReader_ReportEmptySource();
                    return ExtractPrograms ("", currentSession);
                }

            } else {

                currentSession.Diagnostics.FileReader_ReportNoFileFound (filePath.Name);
                return new List<string> ();
            }
        }

        private static IList<string> ExtractPrograms (string sourceText, Session currentSession) {
            IList<string> programs = new List<string> ();

            int currentPosition = 0;
            int programStartPosition = currentPosition;

            Boolean inString = false;

            int length = 0;

            while (currentPosition < sourceText.Length) {
                char currentChar = sourceText[currentPosition];

                if (currentChar == '"') {
                    inString = !inString;
                }

                if (currentChar != '$') {
                    length++;
                } else if (currentChar == '$' && inString) {
                    TextSpan span = new TextSpan (currentPosition, 1);
                    int lineNumber = sourceText.Count (c => c == '\n') + 1;
                    currentSession.Diagnostics.FileReader_ReportEndOfProgramInString (span, lineNumber);
                } else {
                    length++;
                    string programSubstring = sourceText.Substring (programStartPosition, length).Trim ();
                    if (programSubstring.Length > 0 && programSubstring != "") {
                        programs.Add (programSubstring);
                        programStartPosition = currentPosition + 1;
                    }
                    length = 0;
                }

                currentPosition++;
            }

            return programs;
        }
    }
}
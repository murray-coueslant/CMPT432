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
                /*string[] programTexts = fullFileText.Split('$', StringSplitOptions.RemoveEmptyEntries);*/
                if(fullFileText[fullFileText.Length - 1] != '$')
                {
                    IllumiErrorReporter.SendWarning("Source file does not have a final '$' character. Inserting '$' at the end of the file.");
                    return fullFileText + "$";
                }
                return fullFileText;
            } else
            {
                IllumiErrorReporter.SendError($"Could not find the specified file ({filePath.Name}), correct the path and try again.");
                return null;
            }
        }
    }
}
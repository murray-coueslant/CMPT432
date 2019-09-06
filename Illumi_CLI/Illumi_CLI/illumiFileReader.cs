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
                return fullFileText;
            } else
            {
                IllumiErrorReporter.Send("IL002", $"Could not find the specified file ({filePath.Name}), correct the path and try again.");
                return null;
            }
        }
    }
}
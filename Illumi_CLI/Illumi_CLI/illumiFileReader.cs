using System;
using System.IO;

namespace Illumi_CLI
{
    internal class IllumiFileReader
    {
        internal static string[] ReadFile(FileInfo filePath)
        {
            if (filePath.Exists)
            {
                string fullFileText = File.ReadAllText(filePath.FullName);
                string[] programTexts = fullFileText.Split('$');
                return programTexts;
            } else
            {
                IllumiErrorReporter.Send("IL001", $"Could not find the specified file ({filePath.Name}), correct the path and try again.");
                return null;
            }
        }
    }
}
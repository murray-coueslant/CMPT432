using System;
using System.IO;

namespace Illumi_CLI
{
    internal class illumiFileReader
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
                return null;
            }
        }
    }
}
using System;

namespace Illumi_CLI
{
    internal class IllumiErrorReporter
    {
        internal static void Send(string code, string message)
        {
            Console.WriteLine($"[Error] {code}: {message}");
        }
    }
}
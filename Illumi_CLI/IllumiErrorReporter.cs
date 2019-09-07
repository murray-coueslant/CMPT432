using System;

namespace Illumi_CLI
{
    internal class IllumiErrorReporter
    {
        internal static void SendError(string message)
        {
            Console.WriteLine($"[Error] {message}");
        }

        internal static void SendWarning(string message)
        {
            Console.WriteLine($"[Warning] {message}");
        }
    }
}
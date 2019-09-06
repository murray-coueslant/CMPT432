using System;
using System.IO;

namespace Illumi_CLI
{
    internal class IllumiLexer
    {
        internal static void Lex(string[] programs)
        {
            int programCount = 0;

            int[] warningsErrors;

            foreach(string program in programs)
            {
                Console.WriteLine($"Lexing program {programCount}");
                warningsErrors = LexProgram(program);
                Console.WriteLine($"Program {programCount} lex finished with {warningsErrors[0]} warnings and {warningsErrors[1]} errors.");
                programCount++;
            }
        }

        private static int[] LexProgram(string program)
        {
            int[] warningsErrors = new int[2];
            int warnings = 0;
            int errors = 0;

            foreach(char c in program)
            {
                Console.WriteLine(c);
            }

            warningsErrors[0] = warnings;
            warningsErrors[1] = errors;

            return warningsErrors;
        }
    }
}
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;

namespace Illumi_CLI
{
    class Program
    {
        static int Main(string[] args)
        {
            var lexCommand = new Command("lex") { };

            // var parseCommand = new Command { };

            // var semanticAnalysisCommand = new Command { };

            // var codeGenerationCommand = new Command { };

            var filePathOption = new Option(
                "--file-path",
                "Use this option to specify the path of the file you wish to invoke the compiler on."
            )
            {
                Argument = new Argument<FileInfo>()
            };

            filePathOption.AddAlias("-fp");

            lexCommand.AddOption(filePathOption);

            lexCommand.Handler = CommandHandler.Create<FileInfo>((filePath) =>
            {
                if (filePath != null)
                {
                    Console.WriteLine("Lexing " + filePath.Name);
                    string[] programs = illumiFileReader.ReadFile(filePath);

                    if(programs != null)
                    {
                        illumiLexer.Lex(programs);
                    }
                }
            });

            return lexCommand.InvokeAsync(args).Result;
        }
    }
}

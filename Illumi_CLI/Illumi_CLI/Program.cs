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
            Console.WriteLine("Welcome to Illumi!");

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
                    string text = IllumiFileReader.ReadFile(filePath);

                    if(text != String.Empty)
                    {
                        IllumiLexer.Lex(text);
                    }
                } else
                {
                    IllumiErrorReporter.Send("IL001", "No file specified. Try again with the --file-path option.");
                }
            });

            return lexCommand.InvokeAsync(args).Result;
        }
    }
}

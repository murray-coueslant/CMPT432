using System;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace Illumi_CLI
{
    class Program
    {
        /// <summary>
        /// The entry point to the command line interface for the Illumi compiler.
        /// </summary>
        static void Main(string[] args)
        {
            var lexCommand = new Command { };

            // var parseCommand = new Command { };

            // var semanticAnalysisCommand = new Command { };

            // var codeGenerationCommand = new Command { };

            var fileNameOption = new Option(
                "--file-name",
                "Use this option to specify the file you wish to invoke the compiler on."
            )
            {
                Argument = new Argument<FileInfo>()
            };

            lexCommand.AddOption(fileNameOption);

            lexCommand.Handler = CommandHandler.Create<FileInfo>((fileNameOption) =>
            {
                System.Console.WriteLine("Lexing " + fileNameOption.FullName);
            });

        }
        //             if (command != String.Empty && fileName != String.Empty)
        //             {
        //                 switch (command)
        //                 {
        //                     case "lex":
        //                         System.Console.WriteLine("Lexing " + fileName);
        //                         illumiLexer.Lex(fileName);
        //                         break;
        //                     case "parse":
        //                         // token[] tokens = illumiLexer.Lex(args[1]);
        //                         // illumiParser.Parse(tokens);
        //                         break;
        //                     case "semanticAnalysis":
        //                         // token[] tokens = illumiLexer.Lex(args[1]);
        //                         // syntaxTree[] CST = illumiParser.Parse(tokens);
        //                         // illumiAnalyser.Analyse(CST);
        //                         break;
        //                     case "codeGenerate":
        //                         // tokens[] tokens = illumiLexer.Lex(args[1]);
        //                         // syntaxTree[] CST = illumiParser.Parse(tokens);
        //                         // syntaxTree[] AST = illumiAnalyser.Analyse(CST);
        //                         // illumiCodeGenerator.GenerateCode(AST);
        //                         break;
        //                     default:
        //                         printHelp();
        //                         break;
        //                 }
        // }
        //             else
        //             {
        //                 printHelp();
        //             }

        public static void printHelp()
        {
            Console.WriteLine("Invalid command entered, to see a help message use `Illumi_CLI -h` or `dotnet run -- -h`.");
        }
    }
}

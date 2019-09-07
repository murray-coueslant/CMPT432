using System.Linq;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;

namespace Illumi_CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.BackgroundColor = ConsoleColor.Black;

            Console.WriteLine("Welcome to Illumi!");
            Console.Write("> ");

            bool commandLineEnded = false;

            while (commandLineEnded != true)
            {
                string commandInput = Console.ReadLine();

                string[] splitCommandInput = commandInput.Split(' ');

                switch (splitCommandInput.FirstOrDefault().ToLower())
                {
                    case "lex":
                        Console.WriteLine("Entering the Illumi lexer.");
                        if (splitCommandInput.Length != 2)
                        {
                            Console.WriteLine("Please enter the command in the right form. Enter 'help' to see the help message.");
                            break;
                        }

                        FileInfo file = new FileInfo(splitCommandInput[1]);

                        IllumiLexer.Lex(IllumiFileReader.ReadFile(file));

                        break;

                    case "help":
                    case "h":
                    case "?":
                        Console.WriteLine("Currently, the commands for Illumi are:");
                        Console.WriteLine("\t- lex [file]");
                        Console.WriteLine("\t\t- This command will invoke the Illumi lexer on the specified file. You can specify additional settings for the lexer in setup mode.");
                        Console.WriteLine("\t- help, h, or ?");
                        Console.WriteLine("\t- quit, end, exit, close");
                        break;

                    case "quit":
                    case "end":
                    case "exit":
                    case "close":
                        Console.WriteLine("Thanks for using Illumi!");
                        return;

                    default:
                        Console.WriteLine("Enter a valid command. Enter 'help' to see all the available commands.");
                        break;
                }

                Console.Write("> ");
            }

            // var lexCommand = new Command("lex") { };

            // // var parseCommand = new Command { };

            // // var semanticAnalysisCommand = new Command { };

            // // var codeGenerationCommand = new Command { };

            // var filePathOption = new Option(
            //     "--file-path",
            //     "Use this option to specify the path of the file you wish to invoke the compiler on."
            // )
            // {
            //     Argument = new Argument<FileInfo>()
            // };

            // filePathOption.AddAlias("-fp");

            // lexCommand.AddOption(filePathOption);

            // lexCommand.Handler = CommandHandler.Create<FileInfo>((filePath) =>
            // {
            //     if (filePath != null)
            //     {
            //         Console.WriteLine("Lexing " + filePath.Name);
            //         string text = IllumiFileReader.ReadFile(filePath);

            //         if(text != String.Empty)
            //         {
            //             IllumiLexer.Lex(text);
            //         }
            //     } else
            //     {
            //         IllumiErrorReporter.SendError("No file specified. Try again with the --file-path option.");
            //     }
            // });

            // return lexCommand.InvokeAsync(args).Result;
        }
    }
}

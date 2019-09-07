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

            Session currentSession = new Session();

            bool commandLineEnded = false;

            while (commandLineEnded != true)
            {
                string[] command = getCommand();

                switch (command.FirstOrDefault().ToLower())
                {
                    case "lex":
                        Console.WriteLine("Entering the Illumi lexer.");
                        if (command.Length != 2)
                        {
                            Console.WriteLine("Please enter the command in the right form. Enter 'help' to see the help message.");
                            break;
                        }

                        FileInfo file = new FileInfo(command[1]);

                        IllumiLexer.Lex(IllumiFileReader.ReadFile(file));
                        break;

                    case "settings":
                    case "options":
                    case "setup":
                        setupMode(currentSession);
                        Console.WriteLine(currentSession.debugMode);
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
                        commandLineEnded = true;
                        break;

                    default:
                        Console.WriteLine("Enter a valid command. Enter 'help' to see all the available commands.");
                        break;
                }
            }
        }

        public static string[] getCommand()
        {
            Console.Write("> ");

            string commandInput = Console.ReadLine();

            string[] splitCommandInput = commandInput.Split(' ');

            return splitCommandInput;
        }

        public static void setupMode(Session session)
        {
            Console.WriteLine("Entering setup mode.");


            bool setupModeEnded = false;


            while (setupModeEnded != true)
            {
                string[] setupCommand = getCommand();

                switch (setupCommand.FirstOrDefault().ToLower())
                {
                    case "debug":
                        session.setDebugMode();
                        break;

                    case "quit":
                    case "exit":
                    case "return":
                    case "close":
                        Console.WriteLine("Leaving setup mode.");
                        return;

                    default:
                        Console.WriteLine("Enter a valid setup command. Enter 'help' to see available setup commands.");
                        break;
                }
            }
        }
    }

    class Session
    {
        public Session() { }

        public bool debugMode { get; private set; }
        internal void setDebugMode()
        {
            debugMode = !debugMode;

            Console.WriteLine($"Debug mode changed from {(!debugMode).ToString().ToUpper()} to {debugMode.ToString().ToUpper()}.");
        }
    }

}

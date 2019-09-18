﻿using System.Linq;
using System;
using System.IO;

namespace Illumi_CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            DiagnosticCollection diagnostics = new DiagnosticCollection();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.BackgroundColor = ConsoleColor.Black;

            Console.WriteLine("Welcome to Illumi!");

            Session currentSession = new Session();

            bool commandLineEnded = false;

            while (commandLineEnded != true)
            {
                string[] command = getCommand(currentSession);

                switch (command.FirstOrDefault().ToLower())
                {
                    case "lex":
                        if (command.Length != 2)
                        {
                            currentSession.Diagnostics.EntryPoint_MalformedCommand();
                            break;
                        }

                        FileInfo file = new FileInfo(command[1]);

                        string fileText = IllumiFileReader.ReadFile(file, currentSession);

                        currentSession.Diagnostics.DisplayDiagnostics();

                        if (fileText != string.Empty)
                        {
                            Lexer lexer = new Lexer(fileText);
                            while(lexer.Lex())
                        }
                        break;

                    case "settings":
                    case "options":
                    case "setup":
                        setupMode(currentSession);
                        break;

                    case "help":
                    case "h":
                    case "?":
                        showHelp();
                        break;

                    case "quit":
                    case "end":
                    case "exit":
                    case "close":
                    case "q":
                        Console.WriteLine("Thanks for using Illumi!");
                        commandLineEnded = true;
                        break;

                    default:
                        currentSession.Diagnostics.EntryPoint_ReportInvalidCommand();
                        break;
                }

                currentSession.Diagnostics.DisplayDiagnostics();
            }

            Console.ForegroundColor = ConsoleColor.White;
        }

        public static string[] getCommand(Session session)
        {

            if (session.setupMode)
            {
                Console.Write("(Setup) > ");
            }
            else
            {
                Console.Write("> ");
            }


            string commandInput = Console.ReadLine();

            string[] splitCommandInput = commandInput.Split(' ');

            return splitCommandInput;
        }

        public static void showHelp()
        {
            Console.WriteLine("Currently, the commands for Illumi are:");
            Console.WriteLine("\t- lex [file]");
            Console.WriteLine("\t  - This command will invoke the Illumi lexer on the specified file. You can specify additional settings for the lexer in setup mode.");
            Console.WriteLine("\t- settings, options, setup");
            Console.WriteLine("\t  - Enter into setup mode, to alter some settings for the compiler.");
            Console.WriteLine("\t- help, h, or ?");
            Console.WriteLine("\t- quit, end, exit, close");
        }

        public static void setupMode(Session session)
        {
            Console.WriteLine("Entering setup mode.");

            session.setupMode = true;

            while (true)
            {
                string[] setupCommand = getCommand(session);

                switch (setupCommand.FirstOrDefault().ToLower())
                {
                    case "debug":
                        session.setDebugMode();
                        break;

                    case "v":
                    case "verbose":
                        session.setVerboseMode();
                        break;

                    case "quit":
                    case "exit":
                    case "return":
                    case "close":
                    case "q":
                        Console.WriteLine("Leaving setup mode.");
                        session.setupMode = false;
                        return;

                    case "help":
                    case "h":
                    case "?":
                        showSetupHelp();
                        break;

                    default:
                        Console.WriteLine("Enter a valid setup command. Enter 'help' to see available setup commands.");
                        break;
                }
            }
        }

        private static void showSetupHelp()
        {
            Console.WriteLine("Setup help message.");
        }
    }

    class Session
    {
        public bool setupMode;
        public bool verboseMode;

        public Session()
        {
            Diagnostics = new DiagnosticCollection();
        }

        public bool debugMode { get; private set; }
        public DiagnosticCollection Diagnostics { get; private set; }

        internal void setDebugMode()
        {
            debugMode = !debugMode;

            Console.WriteLine($"Debug mode changed from {(!debugMode).ToString().ToUpper()} to {debugMode.ToString().ToUpper()}.");
        }

        internal void setVerboseMode()
        {
            verboseMode = !verboseMode;

            Console.WriteLine($"Verbose output mode changed from {(!verboseMode).ToString().ToUpper()} to {verboseMode.ToString().ToUpper()}.");
        }
    }

}

using System.Linq;
using System;
using System.IO;
using System.Collections.Generic;

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

                        IList<string> filePrograms = IllumiFileReader.ReadFile(file, currentSession);

                        currentSession.Diagnostics.DisplayDiagnostics();

                        if (filePrograms.Count >= 1)
                        {
                            IList<Lexer> lexers = new List<Lexer>();
                            int programCounter = 0;

                            foreach (string program in filePrograms)
                            {
                                lexers.Add(new Lexer(program, currentSession));
                            }

                            foreach (Lexer lexer in lexers)
                            {
                                Console.WriteLine($"Lexing program {programCounter}.");
                                LexProgram(lexer);
                                Console.WriteLine(value: $"Finished lexing program {programCounter}. Lex ended with {lexer.Diagnostics.ErrorCount} error(s) and {diagnostics.WarningCount} warnings.");
                                Console.WriteLine();
                                programCounter++;
                            }
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

        private static void LexProgram(Lexer lexer)
        {
            lexer.Lex();

            while (lexer.GetTokens().LastOrDefault().Kind != TokenKind.EndOfProgramToken && lexer.Diagnostics.ErrorCount == 0)
            {
                lexer.Lex();
            }

            lexer.Diagnostics.DisplayDiagnostics();

            if (lexer.Diagnostics.ErrorCount >= 1)
            {
                lexer.ClearTokens();
            }
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

                    case "config":
                        showConfig(session);
                        break;

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

        private static void showConfig(Session session)
        {
            Console.WriteLine("The current compiler configuration is:");
            Console.WriteLine($"\t- debug = {session.debugMode.ToString().ToUpper()}");
            Console.WriteLine($"\t- verbose = {session.verboseMode.ToString().ToUpper()}");
        }

        private static void showSetupHelp()
        {
            Console.WriteLine("The current settings for Illumi are:");
            Console.WriteLine("\t- debug -> output more specific information about what the compiler is doing");
            Console.WriteLine("\t- verbose -> Sets the compiler to verbose mode, making it output more informative messages (including comments and whitespace in lex etc...). Not currently implemented.");
            Console.WriteLine("\t- config -> display the current compiler configuration");
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

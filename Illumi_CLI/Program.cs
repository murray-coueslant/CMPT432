using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Illumi_CLI {
    class Program {
        static void Main (string[] args) {
            DiagnosticCollection mainDiagnostics = new DiagnosticCollection ();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.BackgroundColor = ConsoleColor.Black;

            Console.WriteLine ("Welcome to Illumi!");

            Session currentSession = new Session ();

            bool commandLineEnded = false;

            while (commandLineEnded != true) {
                string[] command = getCommand (currentSession);

                switch (command.FirstOrDefault ().ToLower ()) {
                    case "lex":
                        if (command.Length != 2) {
                            currentSession.Diagnostics.EntryPoint_MalformedCommand ();
                            break;
                        }

                        IList<string> lexerPrograms = openFile (command[1], currentSession);

                        if (lexerPrograms.Count >= 1) {
                            IList<Lexer> lexers = new List<Lexer> ();
                            int programCounter = 0;

                            foreach (string program in lexerPrograms) {
                                lexers.Add (new Lexer (program, currentSession));
                            }

                            foreach (Lexer lexer in lexers) {
                                Console.WriteLine ($"[Info] - [Lexer] -> Lexing program {programCounter}.");
                                LexProgram (lexer, currentSession);
                                Console.WriteLine (value: $"[Info] - [Lexer] -> Finished lexing program {programCounter}. Lex ended with {lexer.Diagnostics.ErrorCount} error(s) and {mainDiagnostics.WarningCount} warnings.");
                                Console.WriteLine ();
                                programCounter++;
                            }
                        }
                        break;

                    case "parse":
                        if (command.Length != 2) {
                            currentSession.Diagnostics.EntryPoint_MalformedCommand ();
                            break;
                        }

                        IList<string> parserPrograms = openFile (command[1], currentSession);

                        if (parserPrograms.Count >= 1) {
                            IList<Lexer> lexers = new List<Lexer> ();
                            IList<Parser> parsers = new List<Parser> ();
                            int programCounter = 0;

                            foreach (string program in parserPrograms) {
                                lexers.Add (new Lexer (program, currentSession));
                            }

                            foreach (Lexer lexer in lexers) {
                                parsers.Add (new Parser (lexer, currentSession));
                            }

                            foreach (Parser parser in parsers) {
                                Console.WriteLine ($"[Info] - [Lexer] -> Lexing program {programCounter}.");
                                LexProgram (parser.Lexer, currentSession);
                                Console.WriteLine ($"[Info] - [Lexer] -> Finished lexing program {programCounter}. Lex ended with [{parser.Lexer.Diagnostics.ErrorCount}] error(s) and [{mainDiagnostics.WarningCount}] warnings.");
                                Console.WriteLine ();
                                if (parser.Lexer.Diagnostics.ErrorCount > 0) {
                                    System.Console.WriteLine ("[Error] - [Lexer] Lex error, cannot parse. Exiting.");
                                } else {
                                    Console.WriteLine ($"[Info] - [Parser] -> Parsing program {programCounter}.");
                                    ParseProgram (parser, currentSession);
                                    parser.diagnostics.Parser_ReportEndOfParse (programCounter);
                                }
                                Console.WriteLine ();
                                programCounter++;
                            }

                        }
                        break;

                    case "settings":
                    case "options":
                    case "setup":
                        setupMode (currentSession);
                        break;

                    case "help":
                    case "h":
                    case "?":
                        showHelp ();
                        break;

                    case "quit":
                    case "end":
                    case "exit":
                    case "close":
                    case "q":
                        Console.WriteLine ("Thanks for using Illumi!");
                        commandLineEnded = true;
                        break;

                    default:
                        currentSession.Diagnostics.EntryPoint_ReportInvalidCommand ();
                        break;
                }
            }
            Console.ForegroundColor = ConsoleColor.White;
        }

        private static void LexProgram (Lexer lexer, Session currentSession) {
            lexer.Lex ();

            try {
                while (lexer.GetTokens ().LastOrDefault ().Kind != TokenKind.EndOfProgramToken && lexer.Diagnostics.ErrorCount == 0) {
                    lexer.Lex ();
                }
            } catch {
                currentSession.Diagnostics.Lexer_LexerFindsNoTokens ();
            }

            if (lexer.Diagnostics.ErrorCount >= 1) {
                lexer.ClearTokens ();
            }

            currentSession.Diagnostics.ErrorCount = 0;
            currentSession.Diagnostics.WarningCount = 0;

        }

        private static void ParseProgram (Parser parser, Session currentSession) {
            parser.Parse ();
        }

        public static string[] getCommand (Session session) {

            if (session.setupMode) {
                Console.Write ("(Setup) > ");
            } else {
                Console.Write ("> ");
            }

            string commandInput = Console.ReadLine ();

            string[] splitCommandInput = commandInput.Split (' ');

            return splitCommandInput;
        }

        public static void showHelp () {
            Console.WriteLine ("Currently, the commands for Illumi are:");
            Console.WriteLine ("\t- lex [file]");
            Console.WriteLine ("\t  - This command will invoke the Illumi lexer on the specified file. You can specify additional settings for the lexer in setup mode.");
            Console.WriteLine ("\t- parse [file]");
            Console.WriteLine ("\t  - This command will invoke syntactical analysis on the given source file using the Illumi parser. You may specify additional settings \n\t    in the setup mode.");
            Console.WriteLine ("\t- settings, options, setup");
            Console.WriteLine ("\t  - Enter into setup mode, to alter some settings for the compiler.");
            Console.WriteLine ("\t- help, h, or ?");
            Console.WriteLine ("\t- quit, end, exit, close");
        }

        public static IList<string> openFile (string filePath, Session currentSession) {
            FileInfo file = new FileInfo (filePath);

            return IllumiFileReader.ReadFile (file, currentSession);
        }

        public static void setupMode (Session session) {
            Console.WriteLine ("Entering setup mode.");

            session.setupMode = true;

            while (true) {
                string[] setupCommand = getCommand (session);

                switch (setupCommand.FirstOrDefault ().ToLower ()) {
                    case "debug":
                        session.setDebugMode ();
                        break;

                    case "quit":
                    case "exit":
                    case "return":
                    case "close":
                    case "q":
                        Console.WriteLine ("Leaving setup mode.");
                        session.setupMode = false;
                        return;

                    case "config":
                        showConfig (session);
                        break;

                    case "help":
                    case "h":
                    case "?":
                        showSetupHelp ();
                        break;

                    default:
                        Console.WriteLine ("Enter a valid setup command. Enter 'help' to see available setup commands.");
                        break;
                }
            }
        }

        private static void showConfig (Session session) {
            Console.WriteLine ("The current compiler configuration is:");
            Console.WriteLine ($"\t- debug = {session.debugMode.ToString().ToUpper()}");
        }

        private static void showSetupHelp () {
            Console.WriteLine ("The current settings for Illumi are:");
            Console.WriteLine ("\t- debug -> Output more specific information about what the compiler is doing");
            Console.WriteLine ("\t- config -> Display the current compiler configuration");
        }
    }
}
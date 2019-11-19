using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Xml.Serialization;

namespace Illumi_CLI {
    class Program {
        public static Session currentSession = new Session ();
        static DiagnosticCollection mainDiagnostics = new DiagnosticCollection ();

        static void Main (string[] args) {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.BackgroundColor = ConsoleColor.Black;

            Console.WriteLine ("Welcome to Illumi!");

            bool commandLineEnded = false;

            while (commandLineEnded != true) {
                string[] command = getCommand (currentSession);

                switch (command.FirstOrDefault ().ToLower ()) {
                    case "lex":
                        if (command.Length != 2) {
                            mainDiagnostics.EntryPoint_MalformedCommand ();
                            break;
                        }

                        IList<string> lexerPrograms = openFile (command[1], currentSession);

                        if (lexerPrograms.Count >= 1) {
                            IList<Lexer> lexers = new List<Lexer> ();
                            int programCounter = 0;

                            foreach (string program in lexerPrograms) {
                                lexers.Add (new Lexer (program, currentSession, mainDiagnostics));
                            }

                            foreach (Lexer lexer in lexers) {
                                mainDiagnostics.Lexer_ReportLexStart (programCounter);
                                LexProgram (lexer, currentSession);
                                mainDiagnostics.Lexer_ReportLexEnd (programCounter);
                                Console.WriteLine ();
                                programCounter++;
                            }
                        }
                        break;

                    case "parse":
                        if (command.Length != 2) {
                            mainDiagnostics.EntryPoint_MalformedCommand ();
                            break;
                        }

                        IList<string> parserPrograms = openFile (command[1], currentSession);

                        if (parserPrograms.Count >= 1) {
                            IList<Lexer> lexers = new List<Lexer> ();
                            IList<Parser> parsers = new List<Parser> ();
                            int programCounter = 0;

                            foreach (string program in parserPrograms) {
                                lexers.Add (new Lexer (program, currentSession, mainDiagnostics));
                            }

                            foreach (Lexer lexer in lexers) {
                                parsers.Add (new Parser (lexer, currentSession, mainDiagnostics));
                            }

                            foreach (Parser parser in parsers) {
                                mainDiagnostics.Lexer_ReportLexStart (programCounter);
                                LexProgram (parser.Lexer, currentSession);
                                mainDiagnostics.Lexer_ReportLexEnd (programCounter);

                                Console.WriteLine ();

                                if (mainDiagnostics.ErrorCount > 0) {
                                    mainDiagnostics.Parser_EncounteredLexError ();
                                } else {
                                    mainDiagnostics.Parser_ReportStartOfParse (programCounter);
                                    ParseProgram (parser, currentSession);
                                    if (mainDiagnostics.ErrorCount > 0) {
                                        mainDiagnostics.Parser_ParseEndedWithErrors ();
                                    }
                                    mainDiagnostics.Parser_ReportEndOfParse (programCounter);
                                }

                                Console.WriteLine ();
                                programCounter++;
                            }

                        }
                        break;

                    case "semantic":
                        if (command.Length != 2) {
                            mainDiagnostics.EntryPoint_MalformedCommand ();
                            break;
                        }

                        IList<string> programs = openFile (command[1], currentSession);

                        if (programs.Count >= 1) {
                            IList<Lexer> lexers = new List<Lexer> ();
                            IList<Parser> parsers = new List<Parser> ();
                            IList<SemanticAnalyser> semanticAnalysers = new List<SemanticAnalyser> ();

                            int programCounter = 0;

                            foreach (string program in programs) {
                                lexers.Add (new Lexer (program, currentSession, mainDiagnostics));
                            }

                            foreach (Lexer lexer in lexers) {
                                parsers.Add (new Parser (lexer, currentSession, mainDiagnostics));
                            }

                            foreach (Parser parser in parsers) {
                                semanticAnalysers.Add (new SemanticAnalyser (parser, currentSession, mainDiagnostics));
                            }

                            foreach (SemanticAnalyser sA in semanticAnalysers) {
                                mainDiagnostics.Lexer_ReportLexStart (programCounter);
                                LexProgram (sA.Parser.Lexer, currentSession);
                                if (mainDiagnostics.ErrorCount > 0) {
                                    sA.Parser.Lexer.Failed = true;
                                }
                                mainDiagnostics.Lexer_ReportLexEnd (programCounter);

                                Console.WriteLine ();

                                if (mainDiagnostics.ErrorCount > 0 || sA.Parser.Lexer.Failed) {
                                    mainDiagnostics.Parser_EncounteredLexError ();
                                    sA.Parser.Failed = true;
                                } else {
                                    mainDiagnostics.Parser_ReportStartOfParse (programCounter);
                                    ParseProgram (sA.Parser, currentSession);
                                    mainDiagnostics.Parser_ReportEndOfParse (programCounter);
                                }

                                Console.WriteLine ();

                                if (mainDiagnostics.ErrorCount > 0 || sA.Parser.Failed) {
                                    mainDiagnostics.Semantic_EncounteredParseError ();
                                } else {
                                    mainDiagnostics.Semantic_ReportStartOfSemantic (programCounter);
                                    SemanticProgram (sA);
                                    mainDiagnostics.Semantic_ReportEndOfSemantic (programCounter);
                                }

                                Console.WriteLine ();
                                programCounter++;
                            }
                        }
                        break;
                    case "codegen":
                        if (command.Length != 2) {
                            mainDiagnostics.EntryPoint_MalformedCommand ();
                            break;
                        }

                        IList<string> programs = openFile (command[1], currentSession);

                        if (programs.Count >= 1) {
                            IList<Lexer> lexers = new List<Lexer> ();
                            IList<Parser> parsers = new List<Parser> ();
                            IList<SemanticAnalyser> semanticAnalysers = new List<SemanticAnalyser> ();

                            int programCounter = 0;

                            foreach (string program in programs) {
                                lexers.Add (new Lexer (program, currentSession, mainDiagnostics));
                            }

                            foreach (Lexer lexer in lexers) {
                                parsers.Add (new Parser (lexer, currentSession, mainDiagnostics));
                            }

                            foreach (Parser parser in parsers) {
                                semanticAnalysers.Add (new SemanticAnalyser (parser, currentSession, mainDiagnostics));
                            }

                            foreach (SemanticAnalyser sA in semanticAnalysers) {
                                codeGenerators.Add (new CodeGenerator (sA, currentSession, mainDiagnostics));
                            }

                            foreach (CodeGenerator cG in CodeGenerators) {
                                mainDiagnostics.Lexer_ReportLexStart (programCounter);
                                LexProgram (cG.SemanticAnalyser.Parser.Lexer, currentSession);
                                if (mainDiagnostics.ErrorCount > 0) {
                                    cG.SemanticAnalyser.Parser.Lexer.Failed = true;
                                }
                                mainDiagnostics.Lexer_ReportLexEnd (programCounter);

                                Console.WriteLine ();

                                if (mainDiagnostics.ErrorCount > 0 || cG.SemanticAnalyser.Parser.Lexer.Failed) {
                                    mainDiagnostics.Parser_EncounteredLexError ();
                                    cG.SemanticAnalyser.Parser.Failed = true;
                                } else {
                                    mainDiagnostics.Parser_ReportStartOfParse (programCounter);
                                    ParseProgram (cG.SemanticAnalyser.Parser, currentSession);
                                    mainDiagnostics.Parser_ReportEndOfParse (programCounter);
                                }

                                Console.WriteLine ();

                                if (mainDiagnostics.ErrorCount > 0 || cG.SemanticAnalyser.Parser.Failed) {
                                    mainDiagnostics.Semantic_EncounteredParseError ();
                                } else {
                                    mainDiagnostics.Semantic_ReportStartOfSemantic (programCounter);
                                    SemanticProgram (cG.SemanticAnalyser);
                                    mainDiagnostics.Semantic_ReportEndOfSemantic (programCounter);
                                }

                                if (mainDiagnostics.ErrorCount > 0 || cG.SemanticAnalyser.Failed) {
                                    // todo mainDiagnostics.CodeGen_EncounteredSemanticError();
                                } else {
                                    // todo mainDiagnostics.CodeGen_ReportStartOfCodeGen();
                                    CodeGen (cG);
                                    // todo mainDiagnostics.CodeGen_ReportEndOfCodeGen();
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
                        mainDiagnostics.EntryPoint_ReportInvalidCommand ();
                        break;
                }
            }
            Console.ForegroundColor = ConsoleColor.White;
        }

        private static void LexProgram (Lexer lexer, Session currentSession) {
            lexer.Lex ();

            try {
                while (lexer.GetTokens ().LastOrDefault ().Kind != TokenKind.EndOfProgramToken && mainDiagnostics.ErrorCount == 0) {
                    lexer.Lex ();
                }
            } catch {
                mainDiagnostics.Lexer_LexerFindsNoTokens ();
            }

            if (mainDiagnostics.ErrorCount >= 1) {
                lexer.ClearTokens ();
            }

        }

        private static void ParseProgram (Parser parser, Session currentSession) {
            parser.Parse ();
        }

        private static void SemanticProgram (SemanticAnalyser sA) {
            sA.Analyse ();
        }

        private static void CodeGen (CodeGenerator cG) {
            cG.Generate ();
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
            Console.WriteLine ("\t- semantic [file]");
            Console.WriteLine ("\t  - This command will invoke the semantic analyser on the given source file. This includes semantic analysis, and variable scope and type checking.");
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
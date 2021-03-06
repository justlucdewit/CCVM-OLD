﻿using System;
using System.Collections.Generic;
using System.Text;
using CCVM.CCAssembler;

namespace CCVM
{
    class Program
    {
        static void Main(string[] args)    
        {
            ArgParser.Parse(args);

            if (ArgParser.Option("-v") || ArgParser.Option("--version"))
            {
                PrintVersion();
                return;
            }

            // check if an executable was given
            if (args.Length <= 0)
            {
                Console.WriteLine("[ERROR]: No file given");
                return;
            }

            // run the cc binary
            if (args[0].EndsWith(".ccb") || args[0].EndsWith(".CCB"))
                RunBinary(args);

            // assemble the cc assembly
            else if (args[0].EndsWith(".cca") || args[0].EndsWith(".CCA"))
                Assemble(args);

            // error
            else
                Console.WriteLine($"[ERROR] CCVM does not know what to do with file extension \".{args[0].Split(".")[1]}\"");
        }

        static void RunBinary(string[] args)
        {
            DateTime start = DateTime.Now;
            byte[] content = FileParser.ParseBytes(args[0]);
            VM CCVM = new VM();

            CCVM.LoadProgram(content);

            CCVM.Run();
            DateTime end = DateTime.Now;

            if (ArgParser.Option("-d") || ArgParser.Option("--debug"))
            {
                Console.WriteLine("");
                CCVM.PrintStack();

                Console.WriteLine("");
                CCVM.PrintRegs();

                Console.WriteLine("");
                CCVM.PrintMem();

                Console.WriteLine("");
                CCVM.PrintFlags();
            }

            if (ArgParser.Option("-t") || ArgParser.Option("--time"))
            {
                Console.WriteLine("");
                Console.WriteLine($"Execution took {(end - start).Milliseconds} ms");
            }
        }

        static void Assemble(string[] args)
        {
            DateTime start = DateTime.Now;
            string[] parts = args[0].Replace("/", ".").Split(".");
            List<string> pathName = new List<string>(args[0].Split("/"));
            pathName.RemoveAt(pathName.Count - 1);
            string finalPath = string.Join("/", pathName) + "/";
            if (finalPath[0] == '/') {
                finalPath = finalPath.Remove(0);
            }
            
            Console.WriteLine("Assembling...");

            string content = FileParser.ParseString(args[0]);
            Assembler assembler = new Assembler();
            assembler.SetMainPath(finalPath);
            
            content = assembler.HandleInports(content);

            assembler.LoadAssembly(content);
            assembler.Lex();

            if (ArgParser.Option("-d") || ArgParser.Option("--debug"))
                assembler.PrintTokens();

            assembler.GenerateCode(finalPath + parts[parts.Length - 2] + ".ccb");
            Console.WriteLine($"Done, took {(DateTime.Now - start).Milliseconds} ms");
        }

        static void PrintVersion()
        {
            if (Console.BufferWidth < 31)
            {
                Console.WriteLine("\nCCVM V1.1.0");
                return;
            }
            string indent = new StringBuilder(Console.BufferWidth/2 - 13).AppendJoin(" ", new string[Console.BufferWidth / 2 - 12]).ToString();
            string version = @$"___        ___ __       ___
\  \      /  /|   \    /   |
 \  \     \ / |    \  / ^  |
  \  \  ^  v  |  ^  \ \/|  |
   \  \/ \    |  | \ \  |  |
    \____/    |__|  \_| |__|

+--------------------------+
| CCVM V1.1.0              |
| developed by luke_       |
| github.com/justlucdewit  |
+--------------------------+";
            foreach(string line in version.Split('\n'))
            {
                Console.WriteLine(indent+line);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cent.Core;

namespace Cent
{
    class Program
    {
        private const string OPTIMIZE = "--optimize";
        private const string OUT_FILE = "-o";

        static void Main(string[] args)
        {
            try
            {
                if (args.Length > 0)
                {
                    var isOptimized = Array.IndexOf(args, OPTIMIZE) != -1;
                    var outFileOptionIndex = Array.IndexOf(args, OUT_FILE);
                    CentTranscompiler cent;

                    if (outFileOptionIndex == -1)
                    {
                        cent = GetTranscompiler(args.Where(x => x != OPTIMIZE).ToList());
                        cent.IsOptimized = isOptimized;

                        if (Array.IndexOf(args, "--ubpl") != -1)
                        {
                            cent.Output("a.out");
                        }
                        else if (Array.IndexOf(args, "--lua64") != -1)
                        {
                            cent.Output("a.lua");
                        }
                        else
                        {
                            cent.Output("a.lk");
                        }
                    }
                    else if (outFileOptionIndex == args.Length - 1)
                    {
                        Console.WriteLine("No set output file name");
                        DisplayUsage();
                        Environment.Exit(1);
                    }
                    else
                    {
                        var outFileIndex = outFileOptionIndex + 1;
                        var inFiles = args.Where((x, i) => i != outFileOptionIndex && i != outFileIndex && x != OPTIMIZE).ToList();

                        cent = GetTranscompiler(inFiles.ToList());
                        cent.IsOptimized = isOptimized;

                        cent.Output(args.ElementAt(outFileIndex));
                    }
                }
                else
                {
                    DisplayUsage();
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine(ex.StackTrace);

                Environment.Exit(1);
            }
        }

        static CentTranscompiler GetTranscompiler(List<string> inFileNames)
        {
            if (inFileNames.Any(x => x == "--ubpl"))
            {
                return new CentToUbplBinary(inFileNames.Where(x => x != "--ubpl").ToList());
            }
            else if (inFileNames.Any(x => x == "--lua64"))
            {
                return new CentToLua64(inFileNames.Where(x => x != "--lua64").ToList());
            }
            else
            {
                return new CentTo2003lk(inFileNames.Where(x => x != "-l").ToList());
            }

            throw new ApplicationException();
        }

        static void DisplayUsage()
        {
            Console.WriteLine("cent.exe (-l|--ubpl|--lua64) [inFileNames] (-o [outFileName]) (--optimize)");
        }
    }
}

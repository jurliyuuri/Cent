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
        static void Main(string[] args)
        {
            try
            {

                if (args.Length > 0)
                {
                    var outFileOptionIndex = Array.IndexOf(args, "-o");
                    CentTranscompiler cent;

                    if (outFileOptionIndex == -1)
                    {
                        cent = GetTranscompiler(args.ToList());
                        if (Array.IndexOf(args, "-f") != -1)
                        {
                            cent.Run("a.out");
                        }
                        else if (Array.IndexOf(args, "--wat") != -1)
                        {
                            cent.Run("a.wat");
                        }
                        else
                        {
                            cent.Run("a.lk");
                        }
                    }
                    else if (outFileOptionIndex == args.Length - 1)
                    {
                        Console.WriteLine("No set output file name");
                        Console.WriteLine("cent.exe (-l|-f|--wat) [inFileNames] (-o [outFileName])");
                        Environment.Exit(1);
                    }
                    else
                    {
                        var outFileIndex = outFileOptionIndex + 1;
                        var inFiles = args.Where((x, i) => i != outFileOptionIndex && i != outFileIndex).ToList();

                        cent = GetTranscompiler(args.ToList());
                        cent.Run(args[outFileIndex]);
                    }
                }
                else
                {
                    Console.WriteLine("cent.exe (-l|-f|--wat) [inFileNames] (-o [outFileName])");
                }
            }
            catch(Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine(ex.StackTrace);

                Environment.Exit(1);
            }
        }

        static CentTranscompiler GetTranscompiler(List<string> inFileNames)
        {
            if (inFileNames.Any(x => x == "-f" || x == "--2003f"))
            {
                return new CentCompiler(inFileNames.Where(x => x != "-f" && x != "--2003f").ToList(), "2003f");
            }
            else if (inFileNames.Any(x => x == "--ubpl"))
            {
                return new CentCompiler(inFileNames.Where(x => x != "-ubpl").ToList(), "ubpl");
            }
            else if (inFileNames.Any(x => x == "--wat"))
            {
                return new CentToWat(inFileNames.Where(x => x != "--wat").ToList());
            }
            else
            {
                return new CentTo2003lk(inFileNames.Where(x => x != "-l").ToList());
            }
        }
    }
}

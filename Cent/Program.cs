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
                        if (Array.IndexOf(args, "-f") != -1 || Array.IndexOf(args, "--2003f") != -1
                            || Array.IndexOf(args, "--ubpl") != -1)
                        {
                            cent.Output("a.out");
                        }
                        else if (Array.IndexOf(args, "--wat") != -1)
                        {
                            cent.Output("a.wat");
                        }
                        else if (Array.IndexOf(args, "--lua64") != -1)
                        {
                            cent.Output("a.lua");
                        }
                        else if (Array.IndexOf(args, "--win64nasm") != -1)
                        {
                            cent.Output("a.asm");
                        }
                        else
                        {
                            cent.Output("a.lk");
                        }
                    }
                    else if (outFileOptionIndex == args.Length - 1)
                    {
                        Console.WriteLine("No set output file name");
                        Console.WriteLine("cent.exe (-l|-f|--2003f|--ubpl|--wat|--lua64|--win64nasm) [inFileNames] (-o [outFileName])");
                        Environment.Exit(1);
                    }
                    else
                    {
                        var outFileIndex = outFileOptionIndex + 1;
                        var inFiles = args.Where((x, i) => i != outFileOptionIndex && i != outFileIndex).ToList();

                        cent = GetTranscompiler(inFiles.ToList());
                        cent.Output(args[outFileIndex]);
                    }
                }
                else
                {
                    Console.WriteLine("cent.exe (-l|-f|--2003f|--ubpl|--wat|--lua64|--win64nasm) [inFileNames] (-o [outFileName])");
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
            if (inFileNames.Any(x => x == "-f" || x == "--2003f"))
            {
                return new CentTo2003fBinary(inFileNames.Where(x => x != "-f" && x != "--2003f").ToList());
            }
            else if (inFileNames.Any(x => x == "--ubpl"))
            {
                return new CentToUbplBinary(inFileNames.Where(x => x != "-ubpl").ToList());
            }
            else if (inFileNames.Any(x => x == "--wat"))
            {
                return new CentToWat(inFileNames.Where(x => x != "--wat").ToList());
            }
            else if (inFileNames.Any(x => x == "--lua64"))
            {
                return new CentToLua64(inFileNames.Where(x => x != "--lua64").ToList());
            }
            else if (inFileNames.Any(x => x == "--win64nasm"))
            {
                return new CentToWin64Nasm(inFileNames.Where(x => x != "--win64nasm").ToList());
            }
            else
            {
                return new CentTo2003lk(inFileNames.Where(x => x != "-l").ToList());
            }

            throw new ApplicationException();
        }
    }
}

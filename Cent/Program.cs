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
                        if (Array.IndexOf(args, "-f") != -1)
                        {
                            cent = new CentCompiler(args.Where(x => x != "-f").ToList());
                            cent.Run("a.out");
                        }
                        else
                        {
                            cent = new CentTo2003lk(args);
                            cent.Run("a.lk");
                        }
                    }
                    else if (outFileOptionIndex == args.Length - 1)
                    {
                        Console.WriteLine("No set output file name");
                        Console.WriteLine("cent.exe (-l|-f) [inFileNames] (-o [outFileName])");
                        Environment.Exit(1);
                    }
                    else
                    {
                        var outFileIndex = outFileOptionIndex + 1;
                        var inFiles = args.Where((x, i) => i != outFileOptionIndex && i != outFileIndex).ToList();

                        if (Array.IndexOf(args, "-f") != -1)
                        {
                            cent = new CentCompiler(args.Where(x => x != "-f").ToList());
                        }
                        else
                        {
                            cent = new CentTo2003lk(inFiles);
                        }

                        cent.Run(args[outFileIndex]);
                    }
                }
                else
                {
                    Console.WriteLine("cent.exe (-l|-f) [inFileNames] (-o [outFileName])");
                }
            }
            catch(Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine(ex.StackTrace);

                Environment.Exit(1);
            }
        }
    }
}

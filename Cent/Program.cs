using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

                    if (outFileOptionIndex == -1)
                    {
                        var transpiler = new CentTo2003lk(args);
                        transpiler.Run("a.lk");
                    }
                    else if (outFileOptionIndex == args.Length - 1)
                    {
                        Console.WriteLine("No set output file name");
                        Console.WriteLine("cent.exe [inFileNames] (-o [outFileName])");
                        Environment.Exit(1);
                    }
                    else
                    {
                        var outFileIndex = outFileOptionIndex + 1;
                        var inFiles = args.Where((x, i) => i != outFileOptionIndex && i != outFileIndex).ToList();
                        var transpiler = new CentTo2003lk(inFiles);

                        transpiler.Run(args[outFileIndex]);
                    }
                }
                else
                {
                    Console.WriteLine("cent.exe [inFileNames] (-o [outFileName])");
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

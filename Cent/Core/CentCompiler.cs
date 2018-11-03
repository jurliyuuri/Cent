using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cent.Core
{
    class CentCompiler : CentTranscompiler
    {
        readonly CentTo2003fBinary bin2003f;
        readonly CentToUbplBinary binUbpl;

        public CentCompiler(List<string> inFileNames, string format = "2003f") : base(inFileNames)
        {
            this.bin2003f = null;
            this.binUbpl = null;

            switch(format)
            {
                case "2003f":
                    this.bin2003f = new CentTo2003fBinary()
                    {
                        OperatorList = operatorMap.Select(x => x.Key).ToList(),
                        CentOperatorList = centOperatorMap.Select(x => x.Key).ToList(),
                        CompareList = compareMap.Select(x => x.Key).ToList(),

                        Operations = this.operations,
                        Subroutines = this.subroutines,
                    };
                    break;
                case "ubpl":
                    this.binUbpl = new CentToUbplBinary()
                    {
                        OperatorList = operatorMap.Select(x => x.Key).ToList(),
                        CentOperatorList = centOperatorMap.Select(x => x.Key).ToList(),
                        CompareList = compareMap.Select(x => x.Key).ToList(),

                        Operations = this.operations,
                        Subroutines = this.subroutines,
                    };
                    break;
                default:
                    throw new Exception($"Unsupported format: {format}");
            }
        }

        public CentCompiler(string[] inFileNames) : this(inFileNames.ToList())
        {
        }

        protected override void Write(string outFileName)
        {
            if(!(this.bin2003f is null))
            {
                this.bin2003f.Execute();
                this.bin2003f.Write(outFileName);
            }
            else if (!(this.binUbpl is null))
            {
                this.binUbpl.Execute();
                this.binUbpl.Write(outFileName);
            }
        }
    }
}

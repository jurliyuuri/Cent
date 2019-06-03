using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cent.Core
{
    class CentTo2003lk : CentTranscompiler
    {
        readonly Stack<string> jumpLabelStack;
        readonly Dictionary<string, int> labelCount;
        readonly StringBuilder writer;

        public CentTo2003lk(List<string> inFileNames) : base(inFileNames)
        {
            jumpLabelStack = new Stack<string>();
            labelCount = new Dictionary<string, int>()
            {
                ["cecio"] = 0,
                ["fal"] = 0,
                ["fi"] = 0,
                ["leles"] = 0,
            };

            writer = new StringBuilder();
        }

        public CentTo2003lk(string[] inFileNames) : this(inFileNames.ToList())
        {
        }

        protected override void PreProcess(string outFileName)
        {
            // インポートする関数の設定
            foreach (var func in this.funcNames)
            {
                this.writer.Append("xok ").AppendLine(func.Key);
            }
            
            this.writer.AppendLine("'i'c");
        }

        protected override void PostProcess(string outFileName)
        {
            this.writer.AppendLine("nll stack-top lifem 0");

            using (var file = new StreamWriter(outFileName, false, new UTF8Encoding(false)))
            {
                file.Write(this.writer.ToString());
            }
        }

        protected override void MainroutinePreProcess()
        {
            this.writer.AppendLine("nta 8 f5 krz f2 f5+4@ krz stack-top f2")
                .AppendLine("krz f3 f5@ krz f5 f3");
        }

        protected override void MainroutinePostProcess()
        {
            this.writer.AppendLine("krz f3 f5 krz f5@ f3")
                .AppendLine("krz f5+4@ f2 ata 8 f5")
                .AppendLine("krz f5@ xx");
        }

        protected override void SubroutinePreProcess(string name)
        {
            this.writer.Append("nll ").AppendLine(name)
                .AppendLine("nta 4 f5 krz f3 f5@ krz f5 f3");
        }

        protected override void SubroutinePostProcess()
        {
            this.writer.AppendLine("krz f3 f5 krz f5@ f3 ata 4 f5")
                .AppendLine("krz f5@ xx");
        }

        protected override void FenxeSubroutine(string subroutineName)
        {
            this.writer.AppendFormat("nta 4 f5 inj {0} xx f5@ ata 4 f5", subroutineName).AppendLine();
        }

        protected override void Fenxe(string funcName, uint argc)
        {
            if (argc != 0)
            {
                this.writer.AppendFormat("nta {0} f5 nta {0} f2", argc * 4).AppendLine();
                for (uint i = argc; i > 0; i--)
                {
                    this.writer.AppendFormat("krz f2+{0}@ f5+{1}@", i * 4, (argc - i) * 4).AppendLine();
                }
            }
            this.writer.AppendFormat("nta 4 f5 inj {0} xx f5@ ata {1} f5 ata 4 f2 krz f0 f2@", funcName, (argc + 1) * 4).AppendLine();
        }

        protected override void Value(uint result)
        {
            this.writer.AppendFormat("ata 4 f2 krz {0} f2@", result).AppendLine();
        }

        protected override void Nac()
        {
            this.writer.AppendLine("dal 0 f2@");
        }

        protected override void Sna()
        {
            this.writer.AppendLine("dal 0 f2@ ata 1 f2@");
        }

        protected override void Ata()
        {
            BiOperator("ata");
        }

        protected override void Nta()
        {
            BiOperator("nta");
        }

        protected override void Ada()
        {
            BiOperator("ada");
        }

        protected override void Ekc()
        {
            BiOperator("ekc");
        }

        protected override void Dto()
        {
            BiOperator("dto");
        }

        protected override void Dro()
        {
            BiOperator("dro");
        }

        protected override void Dtosna()
        {
            BiOperator("dtosna");
        }

        protected override void Dal()
        {
            BiOperator("dal");
        }

        private void BiOperator(string op)
        {
            this.writer.Append("nta 4 f2 ").Append(op).AppendLine(" f2+4@ f2@");
        }

        protected override void Lat()
        {
            this.writer.AppendLine("lat f2@ f2+4294967292@ f0 inj f0 f2+4294967292@ f2@");
        }

        protected override void Latsna()
        {
            this.writer.AppendLine("latsna f2@ f2+4294967292@ f0 inj f0 f2+4294967292@ f2@");
        }

        protected override void Xtlo()
        {
            CompareOperator("xtlo");
        }

        protected override void Xylo()
        {
            CompareOperator("xylo");
        }

        protected override void Clo()
        {
            CompareOperator("clo");
        }

        protected override void Niv()
        {
            CompareOperator("niv");
        }

        protected override void Llo()
        {
            CompareOperator("llo");
        }

        protected override void Xolo()
        {
            CompareOperator("xolo");
        }

        protected override void Xtlonys()
        {
            CompareOperator("xtlonys");
        }

        protected override void Xylonys()
        {
            CompareOperator("xylonys");
        }

        protected override void Llonys()
        {
            CompareOperator("llonys");
        }

        protected override void Xolonys()
        {
            CompareOperator("xolonys");
        }

        private void CompareOperator(string op)
        {
            int count = this.labelCount["leles"];
            this.labelCount["leles"] = ++count;

            this.writer.AppendFormat("fi f2@ f2+4294967292@ {0}", op)
                .AppendFormat(" malkrz --leles-niv--{0} xx", count).AppendLine()
                .AppendFormat("krz 0 f2+4294967292@ krz --leles-situv--{0} xx", count).AppendLine()
                .AppendFormat("nll --leles-niv--{0} krz 1 f2+4294967292@", count).AppendLine()
                .AppendFormat("nll --leles-situv--{0} kta 4 f2", count).AppendLine();
        }

        protected override void Tikl()
        {
            this.writer.AppendLine("nta 4 f5 krz f2@ f5@ nta 4 f2")
                .AppendLine("nta 4 f5 inj 3126834864 xx f5@ ata 8 f5");
        }

        protected override void Krz()
        {
            this.writer.AppendLine("krz f2@ f2+4@ ata 4 f2");
        }

        protected override void Ach()
        {
            this.writer.AppendLine("inj f2@ f2+4294967292@ f2@");
        }

        protected override void Roft()
        {
            this.writer.AppendLine("krz f2+4294967288@ f0 inj f2@ f2+4294967292@ f2+4294967288@ krz f0 f2@");
        }

        protected override void Ycax()
        {
            this.writer.AppendLine("nta 4 f2");
        }

        protected override void Pielyn()
        {
            this.writer.AppendLine("krz stack-top f2");
        }

        protected override void Fal()
        {
            int count = this.labelCount["fal"]++;
            string falLabel = "--fal--" + count;
            string lafLabel = "--laf--" + count;
            this.jumpLabelStack.Push(falLabel);

            this.jumpLabelStack.Push(lafLabel);
            this.jumpLabelStack.Push(falLabel);

            this.writer.AppendFormat("nll {0} fi f2@ 0 clo malkrz {1} xx", falLabel, lafLabel).AppendLine();
        }

        protected override void Laf()
        {
            if (!this.jumpLabelStack.Peek().StartsWith("--fal--"))
            {
                throw new ApplicationException("'laf' cannot be here");
            }

            this.writer.AppendFormat("krz {0} xx nll {1}", this.jumpLabelStack.Pop(), this.jumpLabelStack.Pop())
                .AppendLine();
        }

        protected override void Fi()
        {
            int count = this.labelCount["fi"]++;
            string olLabel = "--ol--" + count;
            string ifLabel = "--if--" + count;

            this.jumpLabelStack.Push(ifLabel);
            this.jumpLabelStack.Push(olLabel);

            this.writer.AppendFormat("fi f2@ 0 clo malkrz {0} xx", olLabel).AppendLine();
        }

        protected override void Ol()
        {
            if (!this.jumpLabelStack.Peek().StartsWith("--ol--"))
            {
                throw new ApplicationException("'ol' cannot be here");
            }

            string label = this.jumpLabelStack.Pop();

            this.writer.AppendFormat("krz {0} xx nll {1}", this.jumpLabelStack.Peek(), label).AppendLine();
        }

        protected override void If()
        {
            string label = this.jumpLabelStack.Pop();

            if (!(label.StartsWith("--ol--") || label.StartsWith("--if--")))
            {
                throw new ApplicationException("'if' cannot be here");
            }

            if (label.StartsWith("--ol--"))
            {
                this.jumpLabelStack.Pop();
            }

            this.writer.AppendFormat("nll {0}", label).AppendLine();
        }

        protected override void Cecio()
        {
            int count = this.labelCount["cecio"]++;
            string oicecLabel = "--oicec--" + count;
            string cecioLabel = "--cecio--" + count;

            this.jumpLabelStack.Push(oicecLabel);
            this.jumpLabelStack.Push(cecioLabel);

            this.writer.AppendFormat("nll {0} fi f2@ f2+4294967292@ llo malkrz {1} xx", cecioLabel, oicecLabel)
                .AppendLine();
        }

        protected override void Oicec()
        {
            if (!this.jumpLabelStack.Peek().StartsWith("--cecio--"))
            {
                throw new ApplicationException("'oicec' cannot be here");
            }

            this.writer.AppendFormat("ata 1 f2@ krz {0} xx nll {1} nta 8 f2",
                this.jumpLabelStack.Pop(), this.jumpLabelStack.Pop()).AppendLine();
        }

        protected override void Kinfit()
        {
            this.writer.AppendLine("krz f2 f0 nta stack-top f0 dtosna 2 f0 ata 4 f2 krz f0 f2@");
        }

        protected override void Ata1()
        {
            this.writer.AppendLine("ata 1 f2@");
        }

        protected override void Nta1()
        {
            this.writer.AppendLine("nta 1 f2@");
        }

        protected override void RoftNia()
        {
            this.writer.AppendLine("krz f2@ f0 inj f2+4294967288@ f2+4294967292@ f2@ krz f0 f2+4294967288@");
        }
    }
}

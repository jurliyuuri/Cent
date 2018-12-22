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
            
            writer.AppendLine("'i'c");
            writer.AppendLine("nta 4 f5 krz f1 f5@ krz f5 f1");

        }

        protected override void PostProcess(string outFileName)
        {
            writer.AppendLine("krz f1 f5");
            writer.AppendLine("krz f5@ f1 ata 4 f5 krz f5@ xx");

            using (var file = new StreamWriter(outFileName, false, new UTF8Encoding(false)))
            {
                file.Write(this.writer.ToString());
            }
        }

        protected override void Fenxe(string funcName, uint argc)
        {
            if (argc == 0)
            {
                this.writer.AppendFormat("nta 4 f5 inj {0} xx f5@ krz f0 f5@", funcName);
            }
            else
            {
                this.writer.AppendFormat("nta 4 f5 inj {0} xx f5@ ata {1} f5 krz f0 f5@", funcName, argc * 4);
            }
            this.writer.AppendLine();
        }

        protected override void Value(uint result)
        {
            this.writer.AppendFormat("nta 4 f5 krz {0} f5@", result).AppendLine();
        }

        protected override void Nac()
        {
            this.writer.AppendLine("dal 0 f5@");
        }

        protected override void Sna()
        {
            this.writer.AppendLine("dal 0 f5@ ata 1 f5@");
        }

        protected override void Ata()
        {
            this.writer.AppendLine("ata f5+4@ f5@ ata 4 f5");
        }

        protected override void Nta()
        {
            this.writer.AppendLine("nta f5+4@ f5@ ata 4 f5");
        }

        protected override void Ada()
        {
            this.writer.AppendLine("ada f5+4@ f5@ ata 4 f5");
        }

        protected override void Ekc()
        {
            this.writer.AppendLine("akc f5+4@ f5@ ata 4 f5");
        }

        protected override void Dto()
        {
            this.writer.AppendLine("dto f5+4@ f5@ ata 4 f5");
        }

        protected override void Dro()
        {
            this.writer.AppendLine("dro f5+4@ f5@ ata 4 f5");
        }

        protected override void Dtosna()
        {
            this.writer.AppendLine("dtosna f5+4@ f5@ ata 4 f5");
        }

        protected override void Dal()
        {
            this.writer.AppendLine("dal f5+4@ f5@ ata 4 f5");
        }

        protected override void Lat()
        {
            this.writer.AppendLine("lat f5@ f5+4@ f0 inj f0 f5+4@ f5@");
        }

        protected override void Latsna()
        {
            this.writer.AppendLine("latsna f5@ f5+4@ f0 inj f0 f5+4@ f5@");
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

            this.writer.Append("fi f5@ f5+4@ ").Append(op)
                .AppendFormat(" malkrz --leles-niv--{0}", count).AppendLine(" xx")
                .AppendFormat("krz 0 f5+4@ krz --leles-situv--{0}", count).AppendLine(" xx")
                .AppendFormat("nll --leles-niv--{0}", count).AppendLine(" krz 1 f5+4@")
                .AppendFormat("nll --leles-situv--{0}", count).AppendLine(" ata 4 f5");
        }

        protected override void Tikl()
        {
            this.writer.AppendLine("inj 3126834864 xx f5@ ata 4 f5");
        }

        protected override void Krz()
        {
            this.writer.AppendLine("nta 4 f5 krz f5+4@ f5@");
        }

        protected override void Ach()
        {
            this.writer.AppendLine("inj f5@ f5+4@ f5@");
        }

        protected override void Roft()
        {
            this.writer.AppendLine("krz f5+8@ f0 inj f5@ f5+4@ f5+8@ krz f0 f5@");
        }

        protected override void Ycax()
        {
            this.writer.AppendLine("ata 4 f5");
        }

        protected override void Pielyn()
        {
            this.writer.AppendLine("krz f1 f5");
        }

        protected override void Fal()
        {
            int count = this.labelCount["fal"]++;
            string falLabel = "--fal--" + count;
            string lafLabel = "--laf--" + count;
            this.jumpLabelStack.Push(falLabel);

            this.jumpLabelStack.Push(lafLabel);
            this.jumpLabelStack.Push(falLabel);

            this.writer.AppendFormat("nll {0} fi f5@ 0 clo malkrz {1}", falLabel, lafLabel).AppendLine(" xx");
        }

        protected override void Laf()
        {
            if (!this.jumpLabelStack.Peek().StartsWith("--fal--"))
            {
                throw new ApplicationException("'laf' cannot be here");
            }

            this.writer.AppendFormat("krz {0} xx nll ", this.jumpLabelStack.Pop())
                .AppendLine(this.jumpLabelStack.Pop());
        }

        protected override void Fi()
        {
            int count = this.labelCount["fi"]++;
            string olLabel = "--ol--" + count;
            string ifLabel = "--if--" + count;

            this.jumpLabelStack.Push(ifLabel);
            this.jumpLabelStack.Push(olLabel);

            this.writer.AppendFormat("fi f5@ 0 clo malkrz {0}", olLabel).AppendLine(" xx");
        }

        protected override void Ol()
        {
            if (!this.jumpLabelStack.Peek().StartsWith("--ol--"))
            {
                throw new ApplicationException("'ol' cannot be here");
            }

            string label = this.jumpLabelStack.Pop();

            this.writer.AppendFormat("krz {0} xx nll {1}", this.jumpLabelStack.Pop(), label).AppendLine();
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

            this.writer.Append("nll ").AppendLine(label);
        }

        protected override void Cecio()
        {
            int count = this.labelCount["cecio"]++;
            string oicecLabel = "--oicec--" + count;
            string cecioLabel = "--cecio--" + count;

            this.jumpLabelStack.Push(oicecLabel);
            this.jumpLabelStack.Push(cecioLabel);

            this.writer.AppendFormat("nll {0} fi f5@ f5+4@ llo malkrz {1}", cecioLabel, oicecLabel)
                .AppendLine(" xx");
        }

        protected override void Oicec()
        {
            if (!this.jumpLabelStack.Peek().StartsWith("--cecio--"))
            {
                throw new ApplicationException("'oicec' cannot be here");
            }

            this.writer.AppendFormat("ata 1 f5@ krz {0} xx nll {1}", this.jumpLabelStack.Pop(), this.jumpLabelStack.Pop())
                .AppendLine(" ata 8 f5");
        }

        protected override void Kinfit()
        {
            this.writer.AppendLine("krz f1 f0 nta f5 f0 dtosna 2 f0 nta 4 f5 krz f0 f5@");
        }
    }
}

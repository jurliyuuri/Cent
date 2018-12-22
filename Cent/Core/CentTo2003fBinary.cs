using System;
using System.Collections.Generic;
using System.Linq;
using LkCommon.Translator;

namespace Cent.Core
{
    class CentTo2003fBinary : CentTranscompiler
    {
        readonly Binary2003fCreator creator;

        public CentTo2003fBinary(List<string> inFileNames) : base(inFileNames)
        {
            this.creator = new Binary2003fCreator();
        }

        public CentTo2003fBinary(string[] inFileNames) : this(inFileNames.ToList())
        {
        }

        protected override void PreProcess(string outFileName)
        {
            this.creator.PreProcess();
        }

        protected override void PostProcess(string outFileName)
        {
            this.creator.PostProcess();
            this.creator.Write(outFileName);
        }

        protected override void Fenxe(string funcName, uint argc)
        {
            this.creator.Fenxe(funcName, argc);
        }

        protected override void Value(uint result)
        {
            this.creator.Value(result);
        }

        protected override void Nac()
        {
            this.creator.Nac();
        }

        protected override void Sna()
        {
            this.creator.Sna();
        }

        protected override void Ata()
        {
            this.creator.Ata();
        }

        protected override void Nta()
        {
            this.creator.Nta();
        }

        protected override void Ada()
        {
            this.creator.Ada();
        }

        protected override void Ekc()
        {
            this.creator.Ekc();
        }

        protected override void Dto()
        {
            this.creator.Dto();
        }

        protected override void Dro()
        {
            this.creator.Dro();
        }

        protected override void Dtosna()
        {
            this.creator.Dtosna();
        }

        protected override void Dal()
        {
            this.creator.Dal();
        }

        protected override void Lat()
        {
            this.creator.Lat();
        }

        protected override void Latsna()
        {
            this.creator.Latsna();
        }

        protected override void Xtlo()
        {
            this.creator.Xtlo();
        }

        protected override void Xylo()
        {
            this.creator.Xylo();
        }

        protected override void Clo()
        {
            this.creator.Clo();
        }

        protected override void Niv()
        {
            this.creator.Niv();
        }

        protected override void Llo()
        {

            this.creator.Llo();
        }

        protected override void Xolo()
        {
            this.creator.Xolo();
        }

        protected override void Xtlonys()
        {
            this.creator.Xtlonys();
        }

        protected override void Xylonys()
        {
            this.creator.Xylonys();
        }

        protected override void Llonys()
        {
            this.creator.Llonys();
        }

        protected override void Xolonys()
        {
            this.creator.Xolonys();
        }

        protected override void Tikl()
        {
            this.creator.Tikl();
        }

        protected override void Krz()
        {
            this.creator.Krz();
        }

        protected override void Ach()
        {
            this.creator.Ach();
        }

        protected override void Roft()
        {
            this.creator.Roft();
        }

        protected override void Ycax()
        {
            this.creator.Ycax();
        }

        protected override void Pielyn()
        {
            this.creator.Pielyn();
        }

        protected override void Fal()
        {
            this.creator.Fal();
        }

        protected override void Laf()
        {
            this.creator.Laf();
        }

        protected override void Fi()
        {
            this.creator.Fi();
        }

        protected override void Ol()
        {
            this.creator.Ol();
        }

        protected override void If()
        {
            this.creator.If();
        }

        protected override void Cecio()
        {
            this.creator.Cecio();
        }

        protected override void Oicec()
        {
            this.creator.Oicec();
        }

        protected override void Kinfit()
        {
            this.creator.Kinfit();
        }
    }

    class Binary2003fCreator : CodeGenerator
    {
        readonly Dictionary<string, int> labelCount;
        readonly Dictionary<string, bool> kuexok;
        readonly Stack<string> jumpLabelStack;

        public Binary2003fCreator() : base()
        {
            labelCount = new Dictionary<string, int>()
            {
                ["cecio"] = 0,
                ["fal"] = 0,
                ["fi"] = 0,
                ["leles"] = 0,
            };
            kuexok = new Dictionary<string, bool>();
            jumpLabelStack = new Stack<string>();
        }

        public void PreProcess()
        {
            Nta(4, F5);
            Krz(F1, Seti(F5));
            Krz(F5, F1);
        }

        public void PostProcess()
        {
            Krz(F1, F5);
            Krz(Seti(F5), F1);
            Ata(4, F5);
            Krz(Seti(F5), XX);
        }

        public void Fenxe(string funcName, uint argc)
        {
            if (argc == 0)
            {
                Nta(4, F5);
                Inj(funcName, XX, Seti(F5));
                Krz(F0, Seti(F5));
            }
            else
            {
                Nta(4, F5);
                Inj(funcName, XX, Seti(F5));
                Ata(argc * 4, F5);
                Krz(F0, Seti(F5));
            }
        }

        public void Value(uint result)
        {
            Nta(4, F5);
            Krz(result, Seti(F5));
        }

        public void Nac()
        {
            Dal(0, Seti(F5));
        }

        public void Sna()
        {
            Dal(0, Seti(F5));
            Ata(1, Seti(F5));
        }

        public void Ata()
        {
            Ata(Seti(F5), Seti(F5 + 4));
        }

        public void Nta()
        {
            Nta(Seti(F5), Seti(F5 + 4));
        }

        public void Ada()
        {
            Ada(Seti(F5), Seti(F5 + 4));
        }

        public void Ekc()
        {
            Ekc(Seti(F5), Seti(F5 + 4));
        }

        public void Dto()
        {
            Dto(Seti(F5), Seti(F5 + 4));
        }

        public void Dro()
        {
            Dro(Seti(F5), Seti(F5 + 4));
        }

        public void Dtosna()
        {
            Dtosna(Seti(F5), Seti(F5 + 4));
        }

        public void Dal()
        {
            Dal(Seti(F5), Seti(F5 + 4));
        }

        public void Lat()
        {
            Lat(Seti(F5), Seti(F5 + 4), F0);
            Inj(F0, Seti(F5 + 4), Seti(F5));
        }

        public void Latsna()
        {
            Latsna(Seti(F5), Seti(F5 + 4), F0);
            Inj(F0, Seti(F5 + 4), Seti(F5));
        }

        private void Compare(FiType type)
        {
            int count = this.labelCount["leles"];
            this.labelCount["leles"] = ++count;

            Fi(Seti(F5), Seti(F5 + 4), type);
            Malkrz($"leles@niv{count}", XX);
            Krz(0, Seti(F5 + 4));
            Krz($"leles@situv{count}", XX);
            Nll($"leles@niv{count}");
            Krz(1, Seti(F5 + 4));
            Nll($"leles@situv{count}");
            Ata(4, F5);
        }

        public void Xtlo()
        {
            Compare(XTLO);
        }

        public void Xylo()
        {
            Compare(XYLO);
        }

        public void Clo()
        {
            Compare(CLO);
        }

        public void Niv()
        {
            Compare(NIV);
        }

        public void Llo()
        {
            Compare(LLO);
        }

        public void Xolo()
        {
            Compare(XOLO);
        }

        public void Xtlonys()
        {
            Compare(XTLONYS);
        }

        public void Xylonys()
        {
            Compare(XYLONYS);
        }

        public void Llonys()
        {
            Compare(LLONYS);
        }

        public void Xolonys()
        {
            Compare(XOLONYS);
        }

        public void Tikl()
        {
            Inj(LkCommon.LkConstant.TVARLON_KNLOAN_ADDRESS, XX, Seti(F5));
            Ata(4, F5);
        }

        public void Krz()
        {
            Nta(4, F5);
            Krz(Seti(F5 + 4), Seti(F5));
        }

        public void Ach()
        {
            Inj(Seti(F5), Seti(F5 + 4), Seti(F5));
        }

        public void Roft()
        {
            Krz(Seti(F5 + 8), F0);
            Inj(Seti(F5), Seti(F5 + 4), Seti(F5 + 8));
            Krz(F0, Seti(F5));
        }

        public void Ycax()
        {
            Ata(4, F5);
        }

        public void Pielyn()
        {
            Krz(F1, F5);
        }

        public void Fal()
        {
            this.labelCount["fal"] += 1;

            this.jumpLabelStack.Push($"laf@{this.labelCount["fal"]}");
            this.jumpLabelStack.Push($"fal@{this.labelCount["fal"]}");

            Nll($"fal@{this.labelCount["fal"]}");
            Fi(Seti(F5), 0, CLO);
            Malkrz($"laf@{this.labelCount["fal"]}", XX);
        }

        public void Laf()
        {
            if (!this.jumpLabelStack.Peek().StartsWith("fal"))
            {
                throw new ApplicationException("'laf' cannot be here");
            }
            Krz(this.jumpLabelStack.Pop(), XX);
            Nll(this.jumpLabelStack.Pop());
        }

        public void Fi()
        {
            this.labelCount["fi"] += 1;

            this.jumpLabelStack.Push($"if@{this.labelCount["fi"]}");
            this.jumpLabelStack.Push($"ol@{this.labelCount["fi"]}");

            Fi(Seti(F5), 0, CLO);
            Malkrz($"ol@{this.labelCount["fi"]}", XX);
        }

        public void Ol()
        {
            if (!this.jumpLabelStack.Peek().StartsWith("ol"))
            {
                throw new ApplicationException("'ol' cannot be here");
            }

            string label = this.jumpLabelStack.Pop();

            Krz(this.jumpLabelStack.Peek(), XX);
            Nll(label);
            Krz(F0, F0);
        }

        public void If()
        {
            string label = this.jumpLabelStack.Pop();

            if (!(label.StartsWith("ol") || label.StartsWith("if")))
            {
                throw new ApplicationException("'if' cannot be here");
            }

            if (label.StartsWith("ol"))
            {
                this.jumpLabelStack.Pop();
            }

            Nll(label);
        }

        public void Cecio()
        {
            this.labelCount["cecio"] += 1;

            this.jumpLabelStack.Push($"oicec@{this.labelCount["cecio"]}");
            this.jumpLabelStack.Push($"cecio@{this.labelCount["cecio"]}");

            Nll($"cecio@{this.labelCount["cecio"]}");
            Fi(Seti(F5), Seti(F5 + 4), LLO);
            Malkrz($"oicec@{this.labelCount["cecio"]}", XX);
        }

        public void Oicec()
        {
            Ata(1, Seti(F5));
            Krz(this.jumpLabelStack.Pop(), XX);
            Nll(this.jumpLabelStack.Pop());
            Ata(8, F5);
        }

        public void Kinfit()
        {
            Krz(F1, F0);
            Nta(F5, F0);
            Dtosna(2, F0);
            Nta(4, F5);
            Krz(F0, Seti(F5));
        }
    }
}

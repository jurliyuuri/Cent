using System;
using System.Collections.Generic;
using System.Linq;
using UbplCommon.Translator;

namespace Cent.Core
{
    class CentToUbplBinary : CentTranscompiler
    {
        readonly BinaryUbplCreator creator;

        public CentToUbplBinary(List<string> inFileNames) : base(inFileNames)
        {
            this.creator = new BinaryUbplCreator();
        }

        public CentToUbplBinary(string[] inFileNames) : this(inFileNames.ToList())
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

        protected override void Ata1()
        {
            this.creator.Ata1();
        }

        protected override void Nta1()
        {
            this.creator.Nta1();
        }

        protected override void RoftNia()
        {
            this.creator.RoftNia();
        }
    }

    class BinaryUbplCreator : CodeGenerator
    {
        readonly Dictionary<string, int> labelCount;
        readonly Dictionary<string, bool> kuexok;
        readonly Stack<string> jumpLabelStack;
        readonly Dictionary<string, JumpLabel> labels;

        public BinaryUbplCreator() : base()
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
            labels = new Dictionary<string, JumpLabel>();
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
            JumpLabel label;
            if(this.labels.ContainsKey(funcName))
            {
                label = this.labels[funcName];
            }
            else
            {
                label = new JumpLabel();
                this.labels.Add(funcName, label);
            }

            if (argc == 0)
            {
                Nta(4, F5);
                Fnx(label, Seti(F5));
                Krz(F0, Seti(F5));
            }
            else
            {
                Nta(4, F5);
                Fnx(label, Seti(F5));
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
            Lat(Seti(F5), Seti(F5 + 4));
            Anf(Seti(F5 + 4), Seti(F5));
        }

        public void Latsna()
        {
            Latsna(Seti(F5), Seti(F5 + 4));
            Anf(Seti(F5 + 4), Seti(F5));
        }

        private void Compare(FiType type)
        {
            JumpLabel niv = new JumpLabel();
            JumpLabel situv = new JumpLabel();
            
            Fi(Seti(F5), Seti(F5 + 4), type);
            Malkrz(niv, XX);
            Krz(0, Seti(F5 + 4));
            Krz(situv, XX);
            Nll(niv);
            Krz(1, Seti(F5 + 4));
            Nll(situv);
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
            Klon(0xFF, Seti(F5));
            Ata(4, F5);
        }

        public void Krz()
        {
            Nta(4, F5);
            Krz(Seti(F5 + 4), Seti(F5));
        }

        public void Ach()
        {
            Mte(Seti(F5), Seti(F5 + 4));
            Anf(Seti(F5 + 4), Seti(F5));
        }

        public void Roft()
        {
            Krz(Seti(F5 + 8), F0);
            Mte(Seti(F5), Seti(F5 + 4));
            Anf(Seti(F5 + 4), Seti(F5 + 8));
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
            string laf = $"laf@{this.labelCount["fal"]}";
            string fal = $"fal@{this.labelCount["fal"]}";

            JumpLabel lafLabel = new JumpLabel();
            JumpLabel falLabel = new JumpLabel();

            this.jumpLabelStack.Push(laf);
            this.jumpLabelStack.Push(fal);

            this.labels.Add(laf, lafLabel);
            this.labels.Add(fal, falLabel);

            Nll(falLabel);
            Fi(Seti(F5), 0, CLO);
            Malkrz(lafLabel, XX);
        }

        public void Laf()
        {
            string fal = this.jumpLabelStack.Pop();
            if (!fal.StartsWith("fal"))
            {
                throw new ApplicationException("'laf' cannot be here");
            }

            string laf = this.jumpLabelStack.Pop();

            Krz(this.labels[fal], XX);
            Nll(this.labels[laf]);

            this.labels.Remove(fal);
            this.labels.Remove(laf);
        }

        public void Fi()
        {
            this.labelCount["fi"] += 1;

            string _if = $"if@{this.labelCount["fi"]}";
            string ol = $"ol@{this.labelCount["fi"]}";

            JumpLabel ifLabel = new JumpLabel();
            JumpLabel olLabel = new JumpLabel();

            this.jumpLabelStack.Push(_if);
            this.jumpLabelStack.Push(ol);

            this.labels.Add(_if, ifLabel);
            this.labels.Add(ol, olLabel);

            Fi(Seti(F5), 0, CLO);
            Malkrz(olLabel, XX);
        }

        public void Ol()
        {
            string ol = this.jumpLabelStack.Pop();
            if (!ol.StartsWith("ol"))
            {
                throw new ApplicationException("'ol' cannot be here");
            }

            string _if = this.jumpLabelStack.Peek();

            Krz(this.labels[_if], XX);
            Nll(this.labels[ol]);
            Krz(F0, F0);
            
            this.labels.Remove(ol);
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

            Nll(this.labels[label]);

            this.labels.Remove(label);
        }

        public void Cecio()
        {
            this.labelCount["cecio"] += 1;

            string oicec = $"oicec@{this.labelCount["cecio"]}";
            string cecio = $"cecio@{this.labelCount["cecio"]}";

            JumpLabel oicecLabel = new JumpLabel();
            JumpLabel cecioLabel = new JumpLabel();

            this.jumpLabelStack.Push(oicec);
            this.jumpLabelStack.Push(cecio);

            this.labels.Add(oicec, oicecLabel);
            this.labels.Add(cecio, cecioLabel);

            Nll(cecioLabel);
            Fi(Seti(F5), Seti(F5 + 4), LLO);
            Malkrz(oicecLabel, XX);
        }

        public void Oicec()
        {
            string cecio = this.jumpLabelStack.Pop();

            if (!cecio.StartsWith("cecio"))
            {
                throw new ApplicationException("'oicec' cannot be here");
            }

            string oicec = this.jumpLabelStack.Pop();

            Ata(1, Seti(F5));
            Krz(this.labels[cecio], XX);
            Nll(this.labels[oicec]);
            Ata(8, F5);

            this.labels.Remove(cecio);
            this.labels.Remove(oicec);
        }

        public void Kinfit()
        {
            Krz(F1, F0);
            Nta(F5, F0);
            Dtosna(2, F0);
            Nta(4, F5);
            Krz(F0, Seti(F5));
        }

        public void Ata1()
        {
            Ata(1, Seti(F5));
        }

        public void Nta1()
        {
            Nta(1, Seti(F5));
        }

        public void RoftNia()
        {
            Krz(Seti(F5), F0);
            Mte(Seti(F5 + 4), Seti(F5 + 8));
            Anf(Seti(F5), Seti(F5 + 4));
            Krz(F0, Seti(F5 + 8));
        }
    }
}

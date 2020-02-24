using System;
using System.Collections.Generic;
using System.Linq;
using UbplCommon.Translator;

namespace Cent.Core
{
    class CentToUbplBinary : CentTranscompiler
    {
        private readonly BinaryUbplCreator _creator;

        public CentToUbplBinary(IEnumerable<string> inFileNames) : base(inFileNames)
        {
            _creator = new BinaryUbplCreator();
        }

        public CentToUbplBinary(string[] inFileNames) : this(inFileNames.ToList()) { }

        protected override void PreProcess(string outFileName)
        {
            _creator.PreProcess();
        }

        protected override void PostProcess(string outFileName)
        {
            _creator.PostProcess();
            _creator.Write(outFileName);
        }
        protected override void MainroutinePreProcess()
        {
            _creator.MainroutinePreProcess();
        }

        protected override void MainroutinePostProcess()
        {
            _creator.MainroutinePostProcess();
        }

        protected override void SubroutinePreProcess(string name)
        {
            _creator.SubroutinePreProcess(name);
        }

        protected override void SubroutinePostProcess()
        {
            _creator.SubroutinePostProcess();
        }

        protected override void FenxeSubroutine(string subroutineName)
        {
            _creator.FenxeSubroutine(subroutineName);
        }

        protected override void Fenxe(string funcName, uint argc)
        {
            _creator.Fenxe(funcName, argc);
        }

        protected override void Value(uint result)
        {
            _creator.Value(result);
        }

        protected override void Nac()
        {
            _creator.Nac();
        }

        protected override void Sna()
        {
            _creator.Sna();
        }

        protected override void Ata()
        {
            _creator.Ata();
        }

        protected override void Nta()
        {
            _creator.Nta();
        }

        protected override void Ada()
        {
            _creator.Ada();
        }

        protected override void Ekc()
        {
            _creator.Ekc();
        }

        protected override void Dto()
        {
            _creator.Dto();
        }

        protected override void Dro()
        {
            _creator.Dro();
        }

        protected override void Dtosna()
        {
            _creator.Dtosna();
        }

        protected override void Dal()
        {
            _creator.Dal();
        }

        protected override void Lat()
        {
            _creator.Lat();
        }

        protected override void Latsna()
        {
            _creator.Latsna();
        }

        protected override void Xtlo()
        {
            _creator.Xtlo();
        }

        protected override void Xylo()
        {
            _creator.Xylo();
        }

        protected override void Clo()
        {
            _creator.Clo();
        }

        protected override void Niv()
        {
            _creator.Niv();
        }

        protected override void Llo()
        {

            _creator.Llo();
        }

        protected override void Xolo()
        {
            _creator.Xolo();
        }

        protected override void Xtlonys()
        {
            _creator.Xtlonys();
        }

        protected override void Xylonys()
        {
            _creator.Xylonys();
        }

        protected override void Llonys()
        {
            _creator.Llonys();
        }

        protected override void Xolonys()
        {
            _creator.Xolonys();
        }

        protected override void Tikl()
        {
            _creator.Tikl();
        }

        protected override void Krz()
        {
            _creator.Krz();
        }

        protected override void Ach()
        {
            _creator.Ach();
        }

        protected override void Roft()
        {
            _creator.Roft();
        }

        protected override void Ycax()
        {
            _creator.Ycax();
        }

        protected override void Pielyn()
        {
            _creator.Pielyn();
        }

        protected override void Fal()
        {
            _creator.Fal();
        }

        protected override void Laf()
        {
            _creator.Laf();
        }

        protected override void Fi()
        {
            _creator.Fi();
        }

        protected override void Ol()
        {
            _creator.Ol();
        }

        protected override void If()
        {
            _creator.If();
        }

        protected override void Cecio()
        {
            _creator.Cecio();
        }

        protected override void Oicec()
        {
            _creator.Oicec();
        }

        protected override void Kinfit()
        {
            _creator.Kinfit();
        }

        protected override void Ata1()
        {
            _creator.Ata1();
        }

        protected override void Nta1()
        {
            _creator.Nta1();
        }

        protected override void RoftNia()
        {
            _creator.RoftNia();
        }

        protected override void Lat32()
        {
            _creator.Lat32();
        }

        protected override void Latsna32()
        {
            _creator.Latsna32();
        }
    }

    class BinaryUbplCreator : CodeGenerator
    {
        private readonly Dictionary<string, int> _labelCount;
        private readonly Stack<string> _jumpLabelStack;
        private readonly Dictionary<string, JumpLabel> _labels;

        public BinaryUbplCreator() : base()
        {
            _labelCount = new Dictionary<string, int>()
            {
                ["cecio"] = 0,
                ["fal"] = 0,
                ["fi"] = 0,
                ["leles"] = 0,
            };
            _jumpLabelStack = new Stack<string>();
            _labels = new Dictionary<string, JumpLabel>();
        }

        public void PreProcess()
        {
            _labels["stack-top"] = new JumpLabel();
        }

        public void PostProcess()
        {
            Nll(_labels["stack-top"]);
            Lifem(0);
        }

        public void MainroutinePreProcess()
        {
            Nta(8, F5);
            Krz(F2, Seti(F5+4));
            Krz(_labels["stack-top"], F2);
            Krz(F3, Seti(F5));
            Krz(F5, F3);
        }

        public void MainroutinePostProcess()
        {
            Krz(F3, F5);
            Krz(Seti(F5), F3);
            Krz(Seti(F5+4), F2);
            Ata(8, F5);
            Krz(Seti(F5), XX);
        }

        public void SubroutinePreProcess(string subroutineName)
        {
            JumpLabel label;
            if (_labels.ContainsKey(subroutineName))
            {
                label = _labels[subroutineName];
            }
            else
            {
                label = new JumpLabel();
                _labels.Add(subroutineName, label);
            }

            Nll(label);
            Nta(4, F5);
            Krz(F3, Seti(F5));
            Krz(F5, F3);
        }

        public void SubroutinePostProcess()
        {
            Krz(F3, F5);
            Krz(Seti(F5), F3);
            Ata(4, F5);
            Krz(Seti(F5), XX);
        }

        public void FenxeSubroutine(string subroutineName)
        {
            JumpLabel label;
            if (_labels.ContainsKey(subroutineName))
            {
                label = _labels[subroutineName];
            }
            else
            {
                label = new JumpLabel();
                _labels.Add(subroutineName, label);
            }

            Nta(4, F5);
            Fnx(label, Seti(F5));
            Ata(4, F5);
        }

        public void Fenxe(string funcName, uint argc)
        {
            JumpLabel label;
            if(_labels.ContainsKey(funcName))
            {
                label = _labels[funcName];
            }
            else
            {
                label = new JumpLabel();
                _labels.Add(funcName, label);
            }

            if (argc != 0)
            {
                uint pushValue = argc * 4;
                Nta(pushValue, F5);
                Nta(pushValue, F2);

                for (uint i = argc; i > 0; i--)
                {
                    uint value = i * 4;
                    Krz(Seti(F2 + value), Seti(F5 + (pushValue + value)));
                }
            }

            Nta(4, F5);
            Fnx(label, Seti(F5));
            Ata((argc + 1) * 4, F5);
            Ata(4, F2);
            Krz(F0, Seti(F2));
        }

        public void Value(uint result)
        {
            Ata(4, F2);
            Krz(result, Seti(F2));
        }

        public void Nac()
        {
            Dal(0, Seti(F2));
        }

        public void Sna()
        {
            Dal(0, Seti(F2));
            Ata(1, Seti(F2));
        }

        public void Ata()
        {
            Nta(4, F2);
            Ata(Seti(F2 + 4), Seti(F2));
        }

        public void Nta()
        {
            Nta(4, F2);
            Nta(Seti(F2 + 4), Seti(F2));
        }

        public void Ada()
        {
            Nta(4, F2);
            Ada(Seti(F2 + 4), Seti(F2));
        }

        public void Ekc()
        {
            Nta(4, F2);
            Ekc(Seti(F2 + 4), Seti(F2));
        }

        public void Dto()
        {
            Nta(4, F2);
            Dto(Seti(F2 + 4), Seti(F2));
        }

        public void Dro()
        {
            Nta(4, F2);
            Dro(Seti(F2 + 4), Seti(F2));
        }

        public void Dtosna()
        {
            Nta(4, F2);
            Dtosna(Seti(F2 + 4), Seti(F2));
        }

        public void Dal()
        {
            Nta(4, F2);
            Dal(Seti(F2 + 4), Seti(F2));
        }

        public void Lat()
        {
            Lat(Seti(F2), Seti(F2 - 4));
            Anf(Seti(F2), Seti(F2 - 4));
        }

        public void Latsna()
        {
            Latsna(Seti(F2), Seti(F2 - 4));
            Anf(Seti(F2), Seti(F2 - 4));
        }

        private void Compare(FiType type)
        {
            JumpLabel niv = new JumpLabel();
            JumpLabel situv = new JumpLabel();
            
            Fi(Seti(F2), Seti(F2 - 4), type);
            Malkrz(niv, XX);
            Krz(0, Seti(F2 - 4));
            Krz(situv, XX);
            Nll(niv);
            Krz(1, Seti(F2 - 4));
            Nll(situv);
            Nta(4, F2);
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
            Klon(0xFF, Seti(F2));
            Nta(4, F2);
        }

        public void Krz()
        {
            Krz(Seti(F2), Seti(F2 + 4));
            Ata(4, F2);
        }

        public void Ach()
        {
            Mte(Seti(F2), Seti(F2 - 4));
            Anf(Seti(F2 - 4), Seti(F2));
        }

        public void Roft()
        {
            Krz(Seti(F2 - 8), F0);
            Mte(Seti(F2), Seti(F2 - 4));
            Anf(Seti(F2 - 4), Seti(F2 - 8));
            Krz(F0, Seti(F2));
        }

        public void Ycax()
        {
            Nta(4, F2);
        }

        public void Pielyn()
        {
            Krz(_labels["stack-top"], F2);
        }

        public void Fal()
        {
            _labelCount["fal"] += 1;
            string laf = $"laf@{_labelCount["fal"]}";
            string fal = $"fal@{_labelCount["fal"]}";

            JumpLabel lafLabel = new JumpLabel();
            JumpLabel falLabel = new JumpLabel();

            _jumpLabelStack.Push(laf);
            _jumpLabelStack.Push(fal);

            _labels.Add(laf, lafLabel);
            _labels.Add(fal, falLabel);

            Nll(falLabel);
            Fi(Seti(F2), 0, CLO);
            Malkrz(lafLabel, XX);
        }

        public void Laf()
        {
            string fal = _jumpLabelStack.Pop();
            if (!fal.StartsWith("fal"))
            {
                throw new ApplicationException("'laf' cannot be here");
            }

            string laf = _jumpLabelStack.Pop();

            Krz(_labels[fal], XX);
            Nll(_labels[laf]);

            _labels.Remove(fal);
            _labels.Remove(laf);
        }

        public void Fi()
        {
            _labelCount["fi"] += 1;

            string _if = $"if@{_labelCount["fi"]}";
            string ol = $"ol@{_labelCount["fi"]}";

            JumpLabel ifLabel = new JumpLabel();
            JumpLabel olLabel = new JumpLabel();

            _jumpLabelStack.Push(_if);
            _jumpLabelStack.Push(ol);

            _labels.Add(_if, ifLabel);
            _labels.Add(ol, olLabel);

            Fi(Seti(F2), 0, CLO);
            Malkrz(olLabel, XX);
        }

        public void Ol()
        {
            string ol = _jumpLabelStack.Pop();
            if (!ol.StartsWith("ol"))
            {
                throw new ApplicationException("'ol' cannot be here");
            }

            string _if = _jumpLabelStack.Peek();

            Krz(_labels[_if], XX);
            Nll(_labels[ol]);
            Krz(F0, F0);
            
            _labels.Remove(ol);
        }

        public void If()
        {
            string label = _jumpLabelStack.Pop();

            if (!(label.StartsWith("ol") || label.StartsWith("if")))
            {
                throw new ApplicationException("'if' cannot be here");
            }

            if (label.StartsWith("ol"))
            {
                _jumpLabelStack.Pop();
            }

            Nll(_labels[label]);

            _labels.Remove(label);
        }

        public void Cecio()
        {
            _labelCount["cecio"] += 1;

            string oicec = $"oicec@{_labelCount["cecio"]}";
            string cecio = $"cecio@{_labelCount["cecio"]}";

            JumpLabel oicecLabel = new JumpLabel();
            JumpLabel cecioLabel = new JumpLabel();

            _jumpLabelStack.Push(oicec);
            _jumpLabelStack.Push(cecio);

            _labels.Add(oicec, oicecLabel);
            _labels.Add(cecio, cecioLabel);

            Nll(cecioLabel);
            Fi(Seti(F2), Seti(F2 - 4), LLO);
            Malkrz(oicecLabel, XX);
        }

        public void Oicec()
        {
            string cecio = _jumpLabelStack.Pop();

            if (!cecio.StartsWith("cecio"))
            {
                throw new ApplicationException("'oicec' cannot be here");
            }

            string oicec = _jumpLabelStack.Pop();

            Ata(1, Seti(F2));
            Krz(_labels[cecio], XX);
            Nll(_labels[oicec]);
            Nta(8, F2);

            _labels.Remove(cecio);
            _labels.Remove(oicec);
        }

        public void Kinfit()
        {
            Krz(F2, F0);
            Nta(_labels["stack-top"], F0);
            Dtosna(2, F0);
            Ata(4, F2);
            Krz(F0, Seti(F2));
        }

        public void Ata1()
        {
            Ata(1, Seti(F2));
        }

        public void Nta1()
        {
            Nta(1, Seti(F2));
        }

        public void RoftNia()
        {
            Krz(Seti(F2), F0);
            Mte(Seti(F2 - 4), Seti(F2 - 8));
            Anf(Seti(F2), Seti(F2 - 4));
            Krz(F0, Seti(F2 - 8));
        }

        public void Lat32()
        {
            Nta(4, F2);
            Lat(Seti(F2 + 4), Seti(F2));
            Anf(Seti(F2), Seti(F2));
        }

        public void Latsna32()
        {
            Nta(4, F2);
            Latsna(Seti(F2 + 4), Seti(F2));
            Anf(Seti(F2), Seti(F2));
        }
    }
}

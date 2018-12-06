using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UbplCommon.Translator;

namespace Cent.Core
{
    class CentToUbplBinary : CodeGenerator
    {
        readonly List<string> subroutineNames;
        readonly Dictionary<string, int> funcNames;
        readonly Stack<string> jumpLabelStack;
        readonly Stack<string> callSubroutines;
        readonly Dictionary<string, int> labelCount;
        readonly Dictionary<string, bool> kuexok;

        public IList<string> OperatorList { get; set; }
        public IList<string> CentOperatorList { get; set; }
        public IList<string> CompareList { get; set; }
        public IList<string> Operations { get; set; }
        public IList<List<string>> Subroutines { get; set; }


        public CentToUbplBinary() : base()
        {
            subroutineNames = new List<string>();
            funcNames = new Dictionary<string, int>();
            jumpLabelStack = new Stack<string>();
            callSubroutines = new Stack<string>();
            labelCount = new Dictionary<string, int>()
            {
                ["cecio"] = 0,
                ["fal"] = 0,
                ["fi"] = 0,
                ["leles"] = 0,
            };
            kuexok = new Dictionary<string, bool>();
        }

        public void Execute()
        {
            foreach (var subrt in this.Subroutines)
            {
                if (subrt[0] == "xok")
                {
                    this.kuexok[subrt[1]] = true;
                    this.funcNames.Add(subrt[1], int.Parse(subrt[2]));
                }
                else
                {
                    this.subroutineNames.Add(subrt[0]);
                }
            }

            Nta(4, F5);
            Krz(F1, Seti(F5));
            Krz(F5, F1);

            bool isMalef = false;
            foreach (var item in this.Operations)
            {

                if (item == "malef")
                {
                    isMalef = true;
                    throw new ApplicationException($"Sorry, not support this keyword: {item}");
                }
                else if (isMalef)
                {
                }
                else
                {
                    WriteOperation(item);
                }
            }

            Krz(F1, F5);
            Krz(Seti(F5), F1);
            Ata(4, F5);
            Krz(Seti(F5), XX);
        }

        private void WriteOperation(string item)
        {
            if (item.All(char.IsDigit))
            {
                Nta(4, F5);
                Krz(uint.Parse(item), Seti(F5));
            }
            else if (this.OperatorList.Contains(item))
            {
                FromOperator(item);
            }
            else if (this.CentOperatorList.Contains(item))
            {
                FromCentOperator(item);
            }
            else if (this.CompareList.Contains(item))
            {
                FromCompareOperator(item);
            }
            else if (this.subroutineNames.Contains(item))
            {
                if (this.callSubroutines.Contains(item))
                {
                    throw new ApplicationException("Not support recursive subroutine");
                }

                this.callSubroutines.Push(item);
                foreach (var funcItem in this.Subroutines.Where(x => x[0] == item).Single().Skip(1))
                {
                    WriteOperation(funcItem);
                }
                this.callSubroutines.Pop();
            }
            else if (this.funcNames.ContainsKey(item))
            {
                var argc = this.funcNames[item];
                if (argc == 0)
                {
                    Nta(4, F5);
                    Fnx(item, Seti(F5));
                    Krz(F0, Seti(F5));
                }
                else
                {
                    Nta(4, F5);
                    Fnx(item, Seti(F5));
                    Ata((uint)(argc * 4), F5);
                    Krz(F0, Seti(F5));
                }
            }
            else
            {
                throw new ApplicationException($"Unknown word: '{item}'");
            }
        }

        private void FromOperator(string operation)
        {
            if (operation == "kak")
            {
                throw new ApplicationException($"Sorry, not support this keyword: {operation}");
            }

            uint pop = 0U;
            switch (operation)
            {
                case "nac":
                    Dal(0, Seti(F5));
                    break;
                case "ata":
                    Ata(Seti(F5), Seti(F5 + 4));
                    pop = 1;
                    break;
                case "nta":
                    Nta(Seti(F5), Seti(F5 + 4));
                    pop = 1;
                    break;
                case "kak":
                    throw new ApplicationException($"Sorry, not support this keyword");
                case "ada":
                    Ada(Seti(F5), Seti(F5 + 4));
                    pop = 1;
                    break;
                case "ekc":
                    Ekc(Seti(F5), Seti(F5 + 4));
                    pop = 1;
                    break;
                case "dal":
                    Dal(Seti(F5), Seti(F5 + 4));
                    pop = 1;
                    break;
                case "dto":
                    Dto(Seti(F5), Seti(F5 + 4));
                    pop = 1;
                    break;
                case "dtosna":
                    Dtosna(Seti(F5), Seti(F5 + 4));
                    pop = 1;
                    break;
                case "dro":
                case "dRo":
                    Dro(Seti(F5), Seti(F5 + 4));
                    pop = 1;
                    break;
                case "lat":
                    Lat(Seti(F5), Seti(F5 + 4));
                    Anf(Seti(F5 + 4), Seti(F5));
                    break;
                case "latsna":
                    Latsna(Seti(F5), Seti(F5 + 4));
                    Anf(Seti(F5 + 4), Seti(F5));
                    break;
                default:
                    throw new ApplicationException($"Invalid operation: {operation}");
            }

            if (pop > 0)
            {
                Ata(pop * 4, F5);
            }
        }

        private void FromCompareOperator(string operation)
        {
            int count = this.labelCount["leles"];
            this.labelCount["leles"] = ++count;

            FiType type;
            switch (operation)
            {
                case "xtlo":
                    type = XTLO;
                    break;
                case "xylo":
                    type = XYLO;
                    break;
                case "clo":
                    type = CLO;
                    break;
                case "niv":
                    type = NIV;
                    break;
                case "llo":
                    type = LLO;
                    break;
                case "xolo":
                    type = XOLO;
                    break;
                case "xtlonys":
                    type = XTLONYS;
                    break;
                case "xylonys":
                    type = XYLONYS;
                    break;
                case "llonys":
                    type = LLONYS;
                    break;
                case "xolonys":
                    type = XOLONYS;
                    break;
                default:
                    throw new ApplicationException($"Invalid operation: {operation}");
            }

            Fi(Seti(F5), Seti(F5 + 4), type);
            Malkrz($"leles@niv{count}", XX);
            Krz(0, Seti(F5 + 4));
            Krz($"leles@situv{count}", XX);
            Nll($"leles@niv{count}");
            Krz(1, Seti(F5 + 4));
            Nll($"leles@situv{count}");
            Ata(4, F5);
        }

        private void FromCentOperator(string operation)
        {
            string label;
            switch (operation)
            {
                case ".":
                    Fnx(UbplCommon.UbplConstant.TVARLON_KNLOAN_ADDRESS, Seti(F5));
                    Ata(4, F5);
                    break;
                case "krz":
                case "kRz":
                    Nta(4, F5);
                    Krz(Seti(F5 + 4), Seti(F5));
                    break;
                case "ach":
                    Mte(Seti(F5), Seti(F5 + 4));
                    Anf(Seti(F5), Seti(F5 + 4));
                    break;
                case "roft":
                    Krz(Seti(F5 + 8), F0);
                    Mte(Seti(F5), Seti(F5 + 4));
                    Anf(Seti(F5 + 8), Seti(F5 + 4));
                    Krz(F0, Seti(F5));
                    break;
                case "ycax":
                    Ata(4, F5);
                    break;
                case "pielyn":
                    Krz(F1, F5);
                    break;
                case "fal":
                    this.labelCount["fal"] += 1;

                    this.jumpLabelStack.Push($"laf@{this.labelCount["fal"]}");
                    this.jumpLabelStack.Push($"fal@{this.labelCount["fal"]}");

                    Nll($"fal@{this.labelCount["fal"]}");
                    Fi(Seti(F5), 0, CLO);
                    Malkrz($"laf@{this.labelCount["fal"]}", XX);
                    break;
                case "laf":
                    if (!this.jumpLabelStack.Peek().StartsWith("fal"))
                    {
                        throw new ApplicationException("'laf' cannot be here");
                    }
                    Krz(this.jumpLabelStack.Pop(), XX);
                    Nll(this.jumpLabelStack.Pop());
                    break;
                case "fi":
                    this.labelCount["fi"] += 1;

                    this.jumpLabelStack.Push($"if@{this.labelCount["fi"]}");
                    this.jumpLabelStack.Push($"ol@{this.labelCount["fi"]}");

                    Fi(Seti(F5), 0, CLO);
                    Malkrz($"ol@{this.labelCount["fi"]}", XX);
                    break;
                case "ol":
                    if (!this.jumpLabelStack.Peek().StartsWith("ol"))
                    {
                        throw new ApplicationException("'ol' cannot be here");
                    }

                    label = this.jumpLabelStack.Pop();

                    Krz(this.jumpLabelStack.Peek(), XX);
                    Nll(label);
                    Krz(F0, F0);
                    break;
                case "if":
                    label = this.jumpLabelStack.Pop();

                    if (!(label.StartsWith("ol") || label.StartsWith("if")))
                    {
                        throw new ApplicationException("'if' cannot be here");
                    }

                    if (label.StartsWith("ol"))
                    {
                        this.jumpLabelStack.Pop();
                    }

                    Nll(label);
                    break;
                case "cecio":
                    this.labelCount["cecio"] += 1;

                    this.jumpLabelStack.Push($"oicec@{this.labelCount["cecio"]}");
                    this.jumpLabelStack.Push($"cecio@{this.labelCount["cecio"]}");

                    Nll($"cecio@{this.labelCount["cecio"]}");
                    Fi(Seti(F5), Seti(F5 + 4), LLO);
                    Malkrz($"oicec@{this.labelCount["cecio"]}", XX);
                    break;
                case "oicec":
                    Ata(1, Seti(F5));
                    Krz(this.jumpLabelStack.Pop(), XX);
                    Nll(this.jumpLabelStack.Pop());
                    Ata(8, F5);
                    break;
                case "kinfit":
                    Krz(F1, F0);
                    Nta(F5, F0);
                    Dtosna(2, F0);
                    Nta(4, F5);
                    Krz(F0, Seti(F5));
                    break;
                default:
                    throw new ApplicationException($"Invalid operation: {operation}");
            }
        }
    }
}

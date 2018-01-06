using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Cent
{
    class CentTo2003lk : CentTranscompiler
    {
        List<string> functionNames;
        Stack<string> jumpLabelStack;
        Dictionary<string, int> labelCount;

        public CentTo2003lk(List<string> inFileNames) : base(inFileNames)
        {
            functionNames = new List<string>();
            jumpLabelStack = new Stack<string>();
            labelCount = new Dictionary<string, int>()
            {
                ["cecio"] = 0,
                ["oicec"] = 0,
                ["fal"] = 0,
                ["laf"] = 0,
                ["ol"] = 0,
                ["if"] = 0,
            };
        }

        public CentTo2003lk(string[] inFileNames) : this(inFileNames.ToList())
        {
        }

        protected override void Write(string outFileName)
        {
            using (var writer = new StreamWriter(outFileName, false, new UTF8Encoding(false)))
            {
                foreach (var func in this.functions)
                {
                    this.functionNames.Add(func[0]);
                }

                writer.WriteLine("nta 4 f5 krz f1 f5@");

                foreach (var item in this.operations)
                {
                    WriteOperation(writer, item);
                }

                writer.WriteLine("ata {0} f5", (this.useVarStack * 4));
                writer.WriteLine("krz f5@ f1 ata 4 f5 krz f5@ xx");
            }
        }

        private void WriteOperation(StreamWriter writer, string item)
        {
            if (item.All(x => char.IsDigit(x)))
            {
                this.useVarStack++;
                writer.WriteLine("nta 4 f5 krz {0} f5@", item);
            }
            else if (operatorMap.ContainsKey(item))
            {
                writer.WriteLine(FromOperator(item, operatorMap[item]));
            }
            else if (centOperatorMap.ContainsKey(item))
            {
                writer.WriteLine(FromCentOperator(item, centOperatorMap[item]));
            }
            else if (compareMap.ContainsKey(item))
            {
                throw new ApplicationException($"Sorry, not support this keyword");
            }
            else if (this.functionNames.Contains(item))
            {
                foreach (var funcItem in this.functions.Where(x => x[0] == item).Single().Skip(1))
                {
                    WriteOperation(writer, funcItem);
                }
            }
            else
            {
                throw new ApplicationException($"Unknown word: '{item}'");
            }
        }

        private string FromOperator(string operation, int operandCount)
        {
            if(this.useVarStack < operandCount)
            {
                throw new ApplicationException("Stack is empty");
            }
            else if(operation == "kak")
            {
                throw new ApplicationException($"Sorry, not support this keyword");
            }

            switch(operandCount)
            {
                case 1:
                    return $"{operation} f5@";
                case 2:
                    this.useVarStack--;
                    return $"{operation} f5@ f5+4@ ata 4 f5";
                case 3:
                    // lat及びlatsnaのみ
                    return $"{operation} f5@ f5+4@ f0 inj f0 f5+4@ f5@";
                default:
                    throw new ApplicationException($"Invalid operation: {operation}");
            }
        }

        private string FromCentOperator(string operation, int operandCount)
        {
            if (this.useVarStack < operandCount)
            {
                throw new ApplicationException("Stack is empty");
            }

            string label;
            switch(operation)
            {
                case "krz":
                case "kRz":
                    this.useVarStack++;
                    return "nta 4 f5 krz f5+4@ f5@";
                case "ach":
                    return "inj f5@ f5+4@ f5@";
                case "roft":
                    return "krz f5+8@ f0 inj f5@ f5+4@ f5+8@ krz f0 f5@";
                case "ycax":
                    this.useVarStack--;
                    return "ata 4 f5";
                case "pielyn":
                    var count = this.useVarStack;
                    this.useVarStack = 0;
                    return $"ata {(count * 4)} f5";
                case "fal":
                    this.labelCount["fal"] += 1;
                    this.labelCount["laf"] += 1;

                    this.jumpLabelStack.Push($"laf{this.labelCount["laf"]}");
                    this.jumpLabelStack.Push($"fal{this.labelCount["fal"]}");

                    return $"nll fal{this.labelCount["fal"]} fi f5@ 0 clo malkrz laf{this.labelCount["laf"]} xx";
                case "laf":
                    if(!this.jumpLabelStack.Peek().StartsWith("fal"))
                    {
                        throw new ApplicationException("'laf' cannot be here");
                    }
                    return $"krz {this.jumpLabelStack.Pop()} xx nll {this.jumpLabelStack.Pop()}";
                case "fi":
                    this.labelCount["if"] += 1;
                    this.labelCount["ol"] += 1;
                    
                    this.jumpLabelStack.Push($"if{this.labelCount["if"]}");
                    this.jumpLabelStack.Push($"ol{this.labelCount["ol"]}");

                    return $"fi f5@ 0 clo malkrz ol{this.labelCount["ol"]} xx";
                case "ol":
                    if(!this.jumpLabelStack.Peek().StartsWith("ol"))
                    {
                        throw new ApplicationException("'ol' cannot be here");
                    }

                    label = this.jumpLabelStack.Pop();
                    var translate = $"krz {this.jumpLabelStack.Peek()} xx nll {label} fen";
                    
                    return translate;
                case "if":
                    label = this.jumpLabelStack.Pop();

                    if (!(label.StartsWith("ol") || label.StartsWith("if")))
                    {
                        throw new ApplicationException("'if' cannot be here");
                    }

                    if(label == "ol")
                    {
                        label = this.jumpLabelStack.Pop();
                    }

                    return $"nll {label}";
                case "cecio":
                    this.labelCount["cecio"] += 1;
                    this.labelCount["oicec"] += 1;

                    this.jumpLabelStack.Push($"oicec{this.labelCount["oicec"]}");
                    this.jumpLabelStack.Push($"cecio{this.labelCount["cecio"]}");

                    return $"nll cecio{this.labelCount["cecio"]} fi f5@ f5+4@ llo malkrz oicec{this.labelCount["oicec"]} xx";
                case "oicec":
                    useVarStack -= 2;
                    return $"ata 1 f5@ krz {this.jumpLabelStack.Pop()} xx nll {this.jumpLabelStack.Pop()} ata 8 f5";
                default:
                    throw new ApplicationException($"Invalid operation: {operation}");
            }
        }
    }
}

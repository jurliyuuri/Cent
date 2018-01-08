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
                ["fal"] = 0,
                ["fi"] = 0,
                ["leles"] = 0,
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

                writer.WriteLine("nta 4 f5 krz f1 f5@ krz f5 f1");

                foreach (var item in this.operations)
                {
                    WriteOperation(writer, item);
                }

                writer.WriteLine("krz f1 f5");
                writer.WriteLine("krz f5@ f1 ata 4 f5 krz f5@ xx");
            }
        }

        private void WriteOperation(StreamWriter writer, string item)
        {
            if (item.All(x => char.IsDigit(x)))
            {
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
                writer.WriteLine(FromCompareOperator(item, compareMap[item]));
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
            if (operation == "kak")
            {
                throw new ApplicationException($"Sorry, not support this keyword");
            }

            switch(operandCount)
            {
                case 1:
                    return $"{operation} f5@";
                case 2:
                    return $"{operation} f5@ f5+4@ ata 4 f5";
                case 3:
                    // lat及びlatsnaのみ
                    return $"{operation} f5@ f5+4@ f0 inj f0 f5+4@ f5@";
                default:
                    throw new ApplicationException($"Invalid operation: {operation}");
            }
        }

        private string FromCompareOperator(string operation, int operandCount)
        {
            int count = this.labelCount["leles"];
            this.labelCount["leles"] = ++count;

            StringBuilder buffer = new StringBuilder("fi f5@ f5+4@ ").Append(operation);
            
            buffer.Append(" malkrz leles-niv").Append(count).AppendLine(" xx");
            buffer.Append("krz 0 f5+4@ krz leles-situv").Append(count).AppendLine(" xx");
            buffer.Append("nll leles-niv").Append(count).AppendLine(" krz 1 f5+4@");
            buffer.Append("nll leles-situv").Append(count).Append(" ata 4 f5");
            
            return buffer.ToString();
        }

        private string FromCentOperator(string operation, int operandCount)
        {
            string label;
            switch(operation)
            {
                case "krz":
                case "kRz":
                    return "nta 4 f5 krz f5+4@ f5@";
                case "ach":
                    return "inj f5@ f5+4@ f5@";
                case "roft":
                    return "krz f5+8@ f0 inj f5@ f5+4@ f5+8@ krz f0 f5@";
                case "ycax":
                    return "ata 4 f5";
                case "pielyn":
                    return $"ata f1 f5";
                case "fal":
                    this.labelCount["fal"] += 1;

                    this.jumpLabelStack.Push($"laf{this.labelCount["fal"]}");
                    this.jumpLabelStack.Push($"fal{this.labelCount["fal"]}");

                    return $"nll fal{this.labelCount["fal"]} fi f5@ 0 clo malkrz laf{this.labelCount["fal"]} xx";
                case "laf":
                    if(!this.jumpLabelStack.Peek().StartsWith("fal"))
                    {
                        throw new ApplicationException("'laf' cannot be here");
                    }
                    return $"krz {this.jumpLabelStack.Pop()} xx nll {this.jumpLabelStack.Pop()}";
                case "fi":
                    this.labelCount["fi"] += 1;
                    
                    this.jumpLabelStack.Push($"if{this.labelCount["fi"]}");
                    this.jumpLabelStack.Push($"ol{this.labelCount["fi"]}");

                    return $"fi f5@ 0 clo malkrz ol{this.labelCount["fi"]} xx";
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

                    this.jumpLabelStack.Push($"oicec{this.labelCount["cecio"]}");
                    this.jumpLabelStack.Push($"cecio{this.labelCount["cecio"]}");

                    return $"nll cecio{this.labelCount["cecio"]} fi f5@ f5+4@ llo malkrz oicec{this.labelCount["cecio"]} xx";
                case "oicec":
                    return $"ata 1 f5@ krz {this.jumpLabelStack.Pop()} xx nll {this.jumpLabelStack.Pop()} ata 8 f5";
                default:
                    throw new ApplicationException($"Invalid operation: {operation}");
            }
        }
    }
}

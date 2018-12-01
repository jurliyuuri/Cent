using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cent.Core
{
    class CentToLua64 : CentTranscompiler
    {

        readonly List<string> subroutineNames;
        readonly Dictionary<string, int> funcNames;
        readonly Stack<string> jumpLabelStack;
        readonly Stack<string> callSubroutines;
        StringBuilder indent;

        public CentToLua64(List<string> inFileNames) : base(inFileNames)
        {
            subroutineNames = new List<string>();
            funcNames = new Dictionary<string, int>();
            jumpLabelStack = new Stack<string>();
            callSubroutines = new Stack<string>();
            indent = new StringBuilder("");
        }

        public CentToLua64(string[] inFileNames) : this(inFileNames.ToList())
        {
        }

        protected override void Write(string outFileName)
        {
            using (var writer = new StringWriter())
            {
                bool isXok = false;
                foreach (var subrt in this.subroutines)
                {
                    if (subrt[0] == "xok")
                    {
                        var argc = int.Parse(subrt[2]);
                        this.funcNames.Add(subrt[1], argc);

                        isXok = true;
                    }
                    else
                    {
                        this.subroutineNames.Add(subrt[0]);
                    }
                }

                if(isXok)
                {
                    writer.WriteLine("require(\"centxok\")");
                }

                writer.WriteLine("function to32(num){0}  return num & 0x00000000FFFFFFFF{0}end", Environment.NewLine);
                writer.WriteLine("function to64s(num){0}  return num | ((-(to32(num) >> 31)) & 0xFFFFFFFF00000000){0}end", Environment.NewLine);
                writer.WriteLine("local stack = {}");
                writer.WriteLine("local t1, t2 = nil, nil");
                
                bool isMalef = false;
                foreach (var item in this.operations)
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
                        WriteOperation(writer, item);
                    }
                }

                writer.WriteLine("print(\"[\" .. table.concat(stack, \",\") .. \"]\")");
                
                using (var file = new StreamWriter(outFileName, false, new UTF8Encoding(false)))
                {
                    file.Write(writer.ToString());
                }
            }
        }

        private void WriteOperation(StringWriter writer, string item)
        {
            if (item.All(x => char.IsDigit(x)))
            {
                writer.WriteLine("{1}table.insert(stack, {0} & 0x000000FFFFFF)", item, indent);
            }
            else if (operatorMap.ContainsKey(item))
            {
                writer.WriteLine(FromOperator(item));
            }
            else if (centOperatorMap.ContainsKey(item))
            {
                writer.WriteLine(FromCentOperator(item, centOperatorMap[item]));
            }
            else if (compareMap.ContainsKey(item))
            {
                writer.WriteLine(FromCompareOperator(item, compareMap[item]));
            }
            else if (this.subroutineNames.Contains(item))
            {
                if (this.callSubroutines.Contains(item))
                {
                    throw new ApplicationException("Not support recursive subroutine");
                }

                this.callSubroutines.Push(item);
                writer.WriteLine("{1}-- start {0} --", item, indent);

                foreach (var funcItem in this.subroutines.Where(x => x[0] == item).Single().Skip(1))
                {
                    WriteOperation(writer, funcItem);
                }

                writer.WriteLine("{1}-- end {0} --", item, indent);
                this.callSubroutines.Pop();
            }
            else if (this.funcNames.ContainsKey(item))
            {
                int argc = this.funcNames[item];
                if(argc == 0)
                {
                    writer.WriteLine("{0}t1 = {1}()", indent, item);
                    writer.WriteLine("{0}table.insert(stack, t1)", indent);
                }
                else
                {
                    writer.WriteLine("{0}t1 = {{}}", indent);
                    for (int i = 1; i <= argc; i++)
                    {
                        writer.WriteLine("{0}table.insert(t1, 1, table.remove(stack))", indent);
                    }
                    writer.WriteLine("{0}t1 = {1}(table.unpack(t1))", indent, item);
                    writer.WriteLine("{0}table.insert(stack, t1)", indent);
                }
            }
            else
            {
                throw new ApplicationException($"Unknown word: '{item}'");
            }
        }

        private string FromOperator(string operation)
        {
            if (operation == "kak")
            {
                throw new ApplicationException("Sorry, not support this keyword");
            }

            switch (operation)
            {
                case "nac":
                    return indent + "stack[#stack] = to32(-stack[#stack])";
                case "ata":
                    return indent + "t1, t2 = table.remove(stack), stack[#stack]" + Environment.NewLine +
                        indent + "stack[#stack] = to32(t2 + t1)";
                case "nta":
                    return indent + "t1, t2 = table.remove(stack), stack[#stack]" + Environment.NewLine +
                        indent + "stack[#stack] = to32(t2 - t1)";
                case "ada":
                    return indent + "t1, t2 = table.remove(stack), stack[#stack]" + Environment.NewLine +
                        indent + "stack[#stack] = to32(t2 & t1)";
                case "ekc":
                    return indent + "t1, t2 = table.remove(stack), stack[#stack]" + Environment.NewLine +
                        indent + "stack[#stack] = to32(t2 | t1)";
                case "dto":
                    return indent + "t1, t2 = table.remove(stack), stack[#stack]" + Environment.NewLine +
                        indent + "stack[#stack] = to32(t2 >> t1)";
                case "dro":
                case "dRo":
                    return indent + "t1, t2 = table.remove(stack), stack[#stack]" + Environment.NewLine +
                        indent + "stack[#stack] = to32(t2 << t1)";
                case "dtosna":
                    return indent + "t1, t2 = table.remove(stack), stack[#stack]" + Environment.NewLine +
                        indent + "stack[#stack] = to32(to64s(t2) >> t1)";
                case "dal":
                    return indent + "t1, t2 = table.remove(stack), stack[#stack]" + Environment.NewLine +
                        indent + "stack[#stack] = to32((2 & t1) | ~(t2 | t1))";
                case "lat":
                case "latsna":
                default:
                    throw new ApplicationException($"Invalid operation: {operation}");
            }
        }

        private string FromCompareOperator(string operation, int operandCount)
        {
            switch (operation)
            {
                case "xtlo":
                    return indent + "t1, t2 = to64s(table.remove(stack)), to64s(table.remove(stack))" + Environment.NewLine +
                        indent + "if t2 <= t1 then " + Environment.NewLine +
                        indent + "  table.insert(stack, 1)" + Environment.NewLine +
                        indent + "else" + Environment.NewLine +
                        indent + "  table.insert(stack, 0)" + Environment.NewLine +
                        indent + "end";
                case "xylo":
                    return indent + "t1, t2 = to64s(table.remove(stack)), to64s(table.remove(stack))" + Environment.NewLine +
                        indent + "if t2 < t1 then " + Environment.NewLine +
                        indent + "  table.insert(stack, 1)" + Environment.NewLine +
                        indent + "else" + Environment.NewLine +
                        indent + "  table.insert(stack, 0)" + Environment.NewLine +
                        indent + "end";
                case "clo":
                    return indent + "t1, t2 = table.remove(stack), table.remove(stack)" + Environment.NewLine +
                        indent + "if t2 == t1 then " + Environment.NewLine +
                        indent + "  table.insert(stack, 1)" + Environment.NewLine +
                        indent + "else" + Environment.NewLine +
                        indent + "  table.insert(stack, 0)" + Environment.NewLine +
                        indent + "end";
                case "niv":
                    return indent + "t1, t2 = table.remove(stack), table.remove(stack)" + Environment.NewLine +
                        indent + "if t2 ~= t1 then " + Environment.NewLine +
                        indent + "  table.insert(stack, 1)" + Environment.NewLine +
                        indent + "else" + Environment.NewLine +
                        indent + "  table.insert(stack, 0)" + Environment.NewLine +
                        indent + "end";
                case "llo":
                    return indent + "t1, t2 = to64s(table.remove(stack)), to64s(table.remove(stack))" + Environment.NewLine +
                        indent + "if t2 > t1 then " + Environment.NewLine +
                        indent + "  table.insert(stack, 1)" + Environment.NewLine +
                        indent + "else" + Environment.NewLine +
                        indent + "  table.insert(stack, 0)" + Environment.NewLine +
                        indent + "end";
                case "xolo":
                    return indent + "t1, t2 = to64s(table.remove(stack)), to64s(table.remove(stack))" + Environment.NewLine +
                        indent + "if t2 >= t1 then " + Environment.NewLine +
                        indent + "  table.insert(stack, 1)" + Environment.NewLine +
                        indent + "else" + Environment.NewLine +
                        indent + "  table.insert(stack, 0)" + Environment.NewLine +
                        indent + "end";
                case "xtlonys":
                    return indent + "t1, t2 = table.remove(stack), table.remove(stack)" + Environment.NewLine +
                        indent + "if t2 <= t1 then " + Environment.NewLine +
                        indent + "  table.insert(stack, 1)" + Environment.NewLine +
                        indent + "else" + Environment.NewLine +
                        indent + "  table.insert(stack, 0)" + Environment.NewLine +
                        indent + "end";
                case "xylonys":
                    return indent + "t1, t2 = table.remove(stack), table.remove(stack)" + Environment.NewLine +
                        indent + "if t2 < t1 then " + Environment.NewLine +
                        indent + "  table.insert(stack, 1)" + Environment.NewLine +
                        indent + "else" + Environment.NewLine +
                        indent + "  table.insert(stack, 0)" + Environment.NewLine +
                        indent + "end";
                case "llonys":
                    return indent + "t1, t2 = table.remove(stack), table.remove(stack)" + Environment.NewLine +
                        indent + "if t2 > t1 then " + Environment.NewLine +
                        indent + "  table.insert(stack, 1)" + Environment.NewLine +
                        indent + "else" + Environment.NewLine +
                        indent + "  table.insert(stack, 0)" + Environment.NewLine +
                        indent + "end";
                case "xolonys":
                    return indent + "t1, t2 = table.remove(stack), table.remove(stack)" + Environment.NewLine +
                        indent + "if t2 >= t1 then " + Environment.NewLine +
                        indent + "  table.insert(stack, 1)" + Environment.NewLine +
                        indent + "else" + Environment.NewLine +
                        indent + "  table.insert(stack, 0)" + Environment.NewLine +
                        indent + "end";
                default:
                    throw new ApplicationException($"Invalid operation: {operation}");
            }
        }

        private string FromCentOperator(string operation, int operandCount)
        {
            switch (operation)
            {
                case "krz":
                case "kRz":
                    return indent + "table.insert(stack, stack[#stack])";
                case "ach":
                    return indent + "stack[#stack - 1], stack[#stack] = stack[#stack], stack[#stack - 1]";
                case "roft":
                    return indent + "stack[#stack - 2], stack[#stack - 1], stack[#stack]" +
                        " = stack[#stack - 1], stack[#stack], stack[#stack - 2]";
                case "ycax":
                    return indent + "table.remove(stack)";
                case "pielyn":
                    return indent + "stack = {}";
                case "fal":
                    {
                        string oldIndent = indent.ToString();
                        indent.Append("  ");

                        return oldIndent + "while stack[#stack] ~= 0 do";
                    }
                case "laf":
                    {
                        indent.Remove(indent.Length - 2, 2);

                        return indent + "end";
                    }
                case "fi":
                    {
                        string oldIndent = indent.ToString();
                        indent.Append("  ");

                        return oldIndent + "if stack[#stack] ~= 0 then";
                    }
                case "ol":
                    {
                        string oldIndent = indent.ToString().Remove(indent.Length - 2);

                        return oldIndent + "else";
                    }
                case "if":
                    {
                        indent.Remove(indent.Length - 2, 2);

                        return indent + "end";
                    }
                case "cecio":
                    {
                        string oldIndent = indent.ToString();
                        indent.Append("  ");

                        return oldIndent + "while stack[#stack] <= stack[#stack - 1] do";
                    }
                case "oicec":
                    {
                        indent.Remove(indent.Length - 2, 2);

                        return indent + "  stack[#stack] = stack[#stack] + 1" + Environment.NewLine + 
                            indent + "end";
                    }
                case "kinfit":
                    return indent + "table.insert(stack, #stack)";
                default:
                    throw new ApplicationException($"Invalid operation: {operation}");
            }
        }
    }
}

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Cent.Core
{
    class CentToLua64 : CentTranscompiler
    {
        readonly StringBuilder indent;
        readonly StringBuilder writer;
        
        public CentToLua64(IList<string> inFileNames) : base(inFileNames)
        {
            indent = new StringBuilder("");
            writer = new StringBuilder();
        }

        public CentToLua64(string[] inFileNames) : this(inFileNames.ToList()) { }

        protected override void PreProcess(string outFileName)
        {
            if(this.funcNames.Any())
            {
                this.writer.AppendLine("require(\"centxok\")");
            }

            this.writer.AppendLine("local function to32(num)")
                .AppendLine("  return num & 0x00000000FFFFFFFF")
                .AppendLine("end")
                .AppendLine("local function to64s(num)")
                .AppendLine("  return num | ((-(to32(num) >> 31)) & 0xFFFFFFFF00000000)")
                .AppendLine("end")
                .AppendLine("local stack = {}")
                .AppendLine("local t1, t2 = nil, nil");
            this.indent.Append("  ");
        }

        protected override void PostProcess(string outFileName)
        {
            this.writer.AppendLine("print(\"[\" .. table.concat(stack, \",\") .. \"]\")");

            using (var file = new StreamWriter(outFileName, false, new UTF8Encoding(false)))
            {
                file.Write(this.writer.ToString());
            }
        }

        protected override void Fenxe(string funcName, uint argc)
        {
            if (argc == 0)
            {
                this.writer.Append(this.indent).Append("t1 = ")
                    .Append(funcName).AppendLine("()")
                    .Append(this.indent).AppendLine("table.insert(stack, t1)");
            }
            else
            {
                this.writer.Append(this.indent).AppendLine("t1 = {{}}");
                for (int i = 1; i <= argc; i++)
                {
                    this.writer.Append(this.indent).AppendLine("table.insert(t1, 1, table.remove(stack))");
                }
                this.writer.Append(this.indent).Append("t1 = ")
                    .Append(funcName).AppendLine("(table.unpack(t1))")
                    .Append(this.indent).AppendLine("table.insert(stack, t1)");
            }
        }

        protected override void Value(uint result)
        {
            this.writer.Append(this.indent).Append("table.insert(stack, ")
                .Append(result).AppendLine(" & 0x000000FFFFFF)");
        }

        protected override void Nac()
        {
            this.writer.Append(this.indent).AppendLine("stack[#stack] = to32(~stack[#stack])");
        }

        protected override void Sna()
        {
            this.writer.Append(this.indent).AppendLine("stack[#stack] = to32(-stack[#stack])");
        }

        protected override void Ata()
        {
            this.writer.Append(this.indent).AppendLine("t1, t2 = table.remove(stack), stack[#stack]")
                .Append(this.indent).AppendLine("stack[#stack] = to32(t2 + t1)");
        }

        protected override void Nta()
        {
            this.writer.Append(this.indent).AppendLine("t1, t2 = table.remove(stack), stack[#stack]")
                .Append(this.indent).AppendLine("stack[#stack] = to32(t2 - t1)");
        }

        protected override void Ada()
        {
            this.writer.Append(this.indent).AppendLine("t1, t2 = table.remove(stack), stack[#stack]")
                .Append(this.indent).AppendLine("stack[#stack] = to32(t2 & t1)");
        }

        protected override void Ekc()
        {
            this.writer.Append(this.indent).AppendLine("t1, t2 = table.remove(stack), stack[#stack]")
                .Append(this.indent).AppendLine("stack[#stack] = to32(t2 | t1)");
        }

        protected override void Dto()
        {
            this.writer.Append(this.indent).AppendLine("t1, t2 = table.remove(stack), stack[#stack]")
                .Append(this.indent).AppendLine("stack[#stack] = to32(t2 >> t1)");
        }

        protected override void Dro()
        {
            this.writer.Append(this.indent).AppendLine("t1, t2 = table.remove(stack), stack[#stack]")
                .Append(this.indent).AppendLine("stack[#stack] = to32(t2 << t1)");
        }

        protected override void Dtosna()
        {
            this.writer.Append(this.indent).AppendLine("t1, t2 = table.remove(stack), stack[#stack]")
                .Append(this.indent).AppendLine("stack[#stack] = to32(to64s(t2) >> t1)");
        }

        protected override void Dal()
        {
            this.writer.Append(this.indent).AppendLine("t1, t2 = table.remove(stack), stack[#stack]")
                .Append(this.indent).AppendLine("stack[#stack] =  to32((t2 & t1) | (~t2 & ~t1))");
        }

        protected override void Lat()
        {
            this.writer.Append(this.indent).AppendLine("t1, t2 = stack[#stack], stack[#stack - 1]")
                .Append(this.indent).AppendLine("t1 = t1 * t2")
                .Append(this.indent).AppendLine("stack[#stack], stack[#stack - 1] = to32(t1), to32(t1 >> 32)");
        }

        protected override void Latsna()
        {
            this.writer.Append(this.indent).AppendLine("t1, t2 = stack[#stack], stack[#stack - 1]")
                .Append(this.indent).AppendLine("t1 =  to64s(t1) * to64s(t2)")
                .Append(this.indent).AppendLine("stack[#stack], stack[#stack - 1] = to32(t1), to32(t1 >> 32)");
        }

        protected override void Xtlo()
        {
            this.writer.Append(this.indent).AppendLine("t1, t2 = to64s(table.remove(stack)), to64s(table.remove(stack))")
                .Append(this.indent).AppendLine("if t2 <= t1 then ")
                .Append(this.indent).AppendLine("  table.insert(stack, 1)")
                .Append(this.indent).AppendLine("else")
                .Append(this.indent).AppendLine("  table.insert(stack, 0)")
                .Append(this.indent).AppendLine("end");
        }

        protected override void Xylo()
        {
            this.writer.Append(this.indent).AppendLine("t1, t2 = to64s(table.remove(stack)), to64s(table.remove(stack))")
                .Append(this.indent).AppendLine("if t2 < t1 then ")
                .Append(this.indent).AppendLine("  table.insert(stack, 1)")
                .Append(this.indent).AppendLine("else")
                .Append(this.indent).AppendLine("  table.insert(stack, 0)")
                .Append(this.indent).AppendLine("end");
        }

        protected override void Clo()
        {
            this.writer.Append(this.indent).AppendLine("t1, t2 = table.remove(stack), table.remove(stack)")
                .Append(this.indent).AppendLine("if t2 == t1 then ")
                .Append(this.indent).AppendLine("  table.insert(stack, 1)")
                .Append(this.indent).AppendLine("else")
                .Append(this.indent).AppendLine("  table.insert(stack, 0)")
                .Append(this.indent).AppendLine("end");
        }

        protected override void Niv()
        {
            this.writer.Append(this.indent).AppendLine("t1, t2 = table.remove(stack), table.remove(stack)")
                .Append(this.indent).AppendLine("if t2 ~= t1 then ")
                .Append(this.indent).AppendLine("  table.insert(stack, 1)")
                .Append(this.indent).AppendLine("else")
                .Append(this.indent).AppendLine("  table.insert(stack, 0)")
                .Append(this.indent).AppendLine("end");
        }

        protected override void Llo()
        {
            this.writer.Append(this.indent).AppendLine("t1, t2 = to64s(table.remove(stack)), to64s(table.remove(stack))")
                .Append(this.indent).AppendLine("if t2 > t1 then ")
                .Append(this.indent).AppendLine("  table.insert(stack, 1)")
                .Append(this.indent).AppendLine("else")
                .Append(this.indent).AppendLine("  table.insert(stack, 0)")
                .Append(this.indent).AppendLine("end");
        }

        protected override void Xolo()
        {
            this.writer.Append(this.indent).AppendLine("t1, t2 = to64s(table.remove(stack)), to64s(table.remove(stack))")
                .Append(this.indent).AppendLine("if t2 >= t1 then ")
                .Append(this.indent).AppendLine("  table.insert(stack, 1)")
                .Append(this.indent).AppendLine("else")
                .Append(this.indent).AppendLine("  table.insert(stack, 0)")
                .Append(this.indent).AppendLine("end");
        }

        protected override void Xtlonys()
        {
            this.writer.Append(this.indent).AppendLine("t1, t2 = table.remove(stack), table.remove(stack)")
                .Append(this.indent).AppendLine("if t2 <= t1 then ")
                .Append(this.indent).AppendLine("  table.insert(stack, 1)")
                .Append(this.indent).AppendLine("else")
                .Append(this.indent).AppendLine("  table.insert(stack, 0)")
                .Append(this.indent).AppendLine("end");
        }

        protected override void Xylonys()
        {
            this.writer.Append(this.indent).AppendLine("t1, t2 = table.remove(stack), table.remove(stack)")
                .Append(this.indent).AppendLine("if t2 < t1 then ")
                .Append(this.indent).AppendLine("  table.insert(stack, 1)")
                .Append(this.indent).AppendLine("else")
                .Append(this.indent).AppendLine("  table.insert(stack, 0)")
                .Append(this.indent).AppendLine("end");
        }

        protected override void Llonys()
        {
            this.writer.Append(this.indent).AppendLine("t1, t2 = table.remove(stack), table.remove(stack)")
                .Append(this.indent).AppendLine("if t2 > t1 then ")
                .Append(this.indent).AppendLine("  table.insert(stack, 1)")
                .Append(this.indent).AppendLine("else")
                .Append(this.indent).AppendLine("  table.insert(stack, 0)")
                .Append(this.indent).AppendLine("end");
        }

        protected override void Xolonys()
        {
            this.writer.Append(this.indent).AppendLine("t1, t2 = table.remove(stack), table.remove(stack)")
                .Append(this.indent).AppendLine("if t2 >= t1 then ")
                .Append(this.indent).AppendLine("  table.insert(stack, 1)")
                .Append(this.indent).AppendLine("else")
                .Append(this.indent).AppendLine("  table.insert(stack, 0)")
                .Append(this.indent).AppendLine("end");
        }

        protected override void Tikl()
        {
            this.writer.Append(this.indent).AppendLine("print(table.remove(stack))");
        }

        protected override void Krz()
        {
            this.writer.Append(this.indent).AppendLine("table.insert(stack, stack[#stack])");
        }

        protected override void Ach()
        {
            this.writer.Append(this.indent)
                .AppendLine("stack[#stack - 1], stack[#stack] = stack[#stack], stack[#stack - 1]");
        }

        protected override void Roft()
        {
            this.writer.Append(this.indent)
                .Append("stack[#stack - 2], stack[#stack - 1], stack[#stack]")
                .AppendLine(" = stack[#stack - 1], stack[#stack], stack[#stack - 2]");
        }

        protected override void Ycax()
        {
            this.writer.Append(this.indent).AppendLine("table.remove(stack)");
        }

        protected override void Pielyn()
        {
            this.writer.Append(this.indent).AppendLine("stack = {}");
        }

        protected override void Fal()
        {
            string oldIndent = this.indent.ToString();
            this.indent.Append("  ");

            this.writer.Append(oldIndent).AppendLine("while stack[#stack] ~= 0 do");
        }

        protected override void Laf()
        {
            this.indent.Remove(this.indent.Length - 2, 2);

            this.writer.Append(this.indent).AppendLine("end");
        }

        protected override void Fi()
        {
            string oldIndent = this.indent.ToString();
            this.indent.Append("  ");

            this.writer.Append(oldIndent).AppendLine("if stack[#stack] ~= 0 then");
        }

        protected override void Ol()
        {
            string oldIndent = indent.ToString().Remove(indent.Length - 2);

            this.writer.Append(oldIndent).AppendLine("else");
        }

        protected override void If()
        {
            this.indent.Remove(this.indent.Length - 2, 2);

            this.writer.Append(this.indent).AppendLine("end");
        }

        protected override void Cecio()
        {
            string oldIndent = this.indent.ToString();
            this.indent.Append("  ");

            this.writer.Append(oldIndent).AppendLine("while stack[#stack] <= stack[#stack - 1] do");
        }

        protected override void Oicec()
        {
            this.indent.Remove(this.indent.Length - 2, 2);

            this.writer.Append(this.indent).AppendLine("  stack[#stack] = stack[#stack] + 1")
                .Append(this.indent).AppendLine("end");
        }

        protected override void Kinfit()
        {
            this.writer.Append(this.indent).AppendLine("table.insert(stack, #stack)");
        }

        protected override void Ata1()
        {
            this.writer.Append(this.indent).AppendLine("stack[#stack] = to32(stack[#stack] + 1)");
        }

        protected override void Nta1()
        {
            this.writer.Append(this.indent).AppendLine("stack[#stack] = to32(stack[#stack] - 1)");
        }

        protected override void RoftNia()
        {
            this.writer.Append(this.indent)
                .Append("stack[#stack - 2], stack[#stack - 1], stack[#stack]")
                .AppendLine(" = stack[#stack], stack[#stack - 2], stack[#stack - 1]");
        }
    }
}

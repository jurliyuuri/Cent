using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Cent.Core
{
    class CentToLua64 : CentTranscompiler
    {
        private readonly StringBuilder _indent;
        private readonly StringBuilder _writer;
        
        public CentToLua64(IEnumerable<string> inFileNames) : base(inFileNames, false)
        {
            _indent = new StringBuilder(8);
            _writer = new StringBuilder(1024);
        }

        protected override void PreProcess(string outFileName)
        {
            if(FunctionNames.Any())
            {
                _writer.AppendLine("require(\"centxok\")");
            }

            _writer.AppendLine("local function to32(num)")
                .AppendLine("  return num & 0x00000000FFFFFFFF")
                .AppendLine("end")
                .AppendLine("local function to64s(num)")
                .AppendLine("  return num | ((-(to32(num) >> 31)) & 0xFFFFFFFF00000000)")
                .AppendLine("end")
                .AppendLine("local stack = {}")
                .AppendLine("local t1, t2 = nil, nil");
            _indent.Append("  ");
        }

        protected override void PostProcess(string outFileName)
        {
            _writer.AppendLine("s_main()");
            _writer.AppendLine("print(\"[\" .. table.concat(stack, \",\") .. \"]\")");

            using (var file = new StreamWriter(outFileName, false, new UTF8Encoding(false)))
            {
                file.Write(_writer.ToString());
            }
        }

        protected override void MainroutinePreProcess()
        {
            _writer.Append(_indent).AppendLine("local function s_main()");
            _indent.Append("  ");
        }

        protected override void MainroutinePostProcess()
        {
            _indent.Remove(_indent.Length - 2, 2);
            _writer.Append(_indent).AppendLine("end");
        }

        protected override void SubroutinePreProcess(string name)
        {
            _writer.Append(_indent).Append("local function s_").Append(name).AppendLine("()");
            _indent.Append("  ");
        }

        protected override void SubroutinePostProcess()
        {
            _indent.Remove(_indent.Length - 2, 2);
            _writer.Append(_indent).AppendLine("end");
        }

        protected override void FenxeSubroutine(string subroutineName)
        {
            _writer.Append(_indent).Append(subroutineName).AppendLine("()");
        }

        protected override void Fenxe(string funcName, uint argc)
        {
            if (argc == 0)
            {
                _writer.Append(_indent).Append("t1 = ")
                    .Append(funcName).AppendLine("()")
                    .Append(_indent).AppendLine("table.insert(stack, t1)");
            }
            else
            {
                _writer.Append(_indent).AppendLine("t1 = {{}}");
                for (int i = 1; i <= argc; i++)
                {
                    _writer.Append(_indent).AppendLine("table.insert(t1, 1, table.remove(stack))");
                }
                _writer.Append(_indent).Append("t1 = ")
                    .Append(funcName).AppendLine("(table.unpack(t1))")
                    .Append(_indent).AppendLine("table.insert(stack, t1)");
            }
        }

        protected override void Value(uint result)
        {
            _writer.Append(_indent).Append("table.insert(stack, ")
                .Append(result).AppendLine(" & 0x000000FFFFFF)");
        }

        protected override void Nac()
        {
            _writer.Append(_indent).AppendLine("stack[#stack] = to32(~stack[#stack])");
        }

        protected override void Sna()
        {
            _writer.Append(_indent).AppendLine("stack[#stack] = to32(-stack[#stack])");
        }

        protected override void Ata()
        {
            _writer.Append(_indent).AppendLine("t1, t2 = table.remove(stack), stack[#stack]")
                .Append(_indent).AppendLine("stack[#stack] = to32(t2 + t1)");
        }

        protected override void Nta()
        {
            _writer.Append(_indent).AppendLine("t1, t2 = table.remove(stack), stack[#stack]")
                .Append(_indent).AppendLine("stack[#stack] = to32(t2 - t1)");
        }

        protected override void Ada()
        {
            _writer.Append(_indent).AppendLine("t1, t2 = table.remove(stack), stack[#stack]")
                .Append(_indent).AppendLine("stack[#stack] = to32(t2 & t1)");
        }

        protected override void Ekc()
        {
            _writer.Append(_indent).AppendLine("t1, t2 = table.remove(stack), stack[#stack]")
                .Append(_indent).AppendLine("stack[#stack] = to32(t2 | t1)");
        }

        protected override void Dto()
        {
            _writer.Append(_indent).AppendLine("t1, t2 = table.remove(stack), stack[#stack]")
                .Append(_indent).AppendLine("stack[#stack] = to32(t2 >> t1)");
        }

        protected override void Dro()
        {
            _writer.Append(_indent).AppendLine("t1, t2 = table.remove(stack), stack[#stack]")
                .Append(_indent).AppendLine("stack[#stack] = to32(t2 << t1)");
        }

        protected override void Dtosna()
        {
            _writer.Append(_indent).AppendLine("t1, t2 = table.remove(stack), stack[#stack]")
                .Append(_indent).AppendLine("stack[#stack] = to32(to64s(t2) >> t1)");
        }

        protected override void Dal()
        {
            _writer.Append(_indent).AppendLine("t1, t2 = table.remove(stack), stack[#stack]")
                .Append(_indent).AppendLine("stack[#stack] =  to32((t2 & t1) | (~t2 & ~t1))");
        }

        protected override void Lat()
        {
            _writer.Append(_indent).AppendLine("t1, t2 = stack[#stack], stack[#stack - 1]")
                .Append(_indent).AppendLine("t1 = t1 * t2")
                .Append(_indent).AppendLine("stack[#stack], stack[#stack - 1] = to32(t1), to32(t1 >> 32)");
        }

        protected override void Latsna()
        {
            _writer.Append(_indent).AppendLine("t1, t2 = stack[#stack], stack[#stack - 1]")
                .Append(_indent).AppendLine("t1 =  to64s(t1) * to64s(t2)")
                .Append(_indent).AppendLine("stack[#stack], stack[#stack - 1] = to32(t1), to32(t1 >> 32)");
        }

        protected override void Xtlo()
        {
            _writer.Append(_indent).AppendLine("t1, t2 = to64s(table.remove(stack)), to64s(table.remove(stack))")
                .Append(_indent).AppendLine("if t2 <= t1 then ")
                .Append(_indent).AppendLine("  table.insert(stack, 1)")
                .Append(_indent).AppendLine("else")
                .Append(_indent).AppendLine("  table.insert(stack, 0)")
                .Append(_indent).AppendLine("end");
        }

        protected override void Xylo()
        {
            _writer.Append(_indent).AppendLine("t1, t2 = to64s(table.remove(stack)), to64s(table.remove(stack))")
                .Append(_indent).AppendLine("if t2 < t1 then ")
                .Append(_indent).AppendLine("  table.insert(stack, 1)")
                .Append(_indent).AppendLine("else")
                .Append(_indent).AppendLine("  table.insert(stack, 0)")
                .Append(_indent).AppendLine("end");
        }

        protected override void Clo()
        {
            _writer.Append(_indent).AppendLine("t1, t2 = table.remove(stack), table.remove(stack)")
                .Append(_indent).AppendLine("if t2 == t1 then ")
                .Append(_indent).AppendLine("  table.insert(stack, 1)")
                .Append(_indent).AppendLine("else")
                .Append(_indent).AppendLine("  table.insert(stack, 0)")
                .Append(_indent).AppendLine("end");
        }

        protected override void Niv()
        {
            _writer.Append(_indent).AppendLine("t1, t2 = table.remove(stack), table.remove(stack)")
                .Append(_indent).AppendLine("if t2 ~= t1 then ")
                .Append(_indent).AppendLine("  table.insert(stack, 1)")
                .Append(_indent).AppendLine("else")
                .Append(_indent).AppendLine("  table.insert(stack, 0)")
                .Append(_indent).AppendLine("end");
        }

        protected override void Llo()
        {
            _writer.Append(_indent).AppendLine("t1, t2 = to64s(table.remove(stack)), to64s(table.remove(stack))")
                .Append(_indent).AppendLine("if t2 > t1 then ")
                .Append(_indent).AppendLine("  table.insert(stack, 1)")
                .Append(_indent).AppendLine("else")
                .Append(_indent).AppendLine("  table.insert(stack, 0)")
                .Append(_indent).AppendLine("end");
        }

        protected override void Xolo()
        {
            _writer.Append(_indent).AppendLine("t1, t2 = to64s(table.remove(stack)), to64s(table.remove(stack))")
                .Append(_indent).AppendLine("if t2 >= t1 then ")
                .Append(_indent).AppendLine("  table.insert(stack, 1)")
                .Append(_indent).AppendLine("else")
                .Append(_indent).AppendLine("  table.insert(stack, 0)")
                .Append(_indent).AppendLine("end");
        }

        protected override void Xtlonys()
        {
            _writer.Append(_indent).AppendLine("t1, t2 = table.remove(stack), table.remove(stack)")
                .Append(_indent).AppendLine("if t2 <= t1 then ")
                .Append(_indent).AppendLine("  table.insert(stack, 1)")
                .Append(_indent).AppendLine("else")
                .Append(_indent).AppendLine("  table.insert(stack, 0)")
                .Append(_indent).AppendLine("end");
        }

        protected override void Xylonys()
        {
            _writer.Append(_indent).AppendLine("t1, t2 = table.remove(stack), table.remove(stack)")
                .Append(_indent).AppendLine("if t2 < t1 then ")
                .Append(_indent).AppendLine("  table.insert(stack, 1)")
                .Append(_indent).AppendLine("else")
                .Append(_indent).AppendLine("  table.insert(stack, 0)")
                .Append(_indent).AppendLine("end");
        }

        protected override void Llonys()
        {
            _writer.Append(_indent).AppendLine("t1, t2 = table.remove(stack), table.remove(stack)")
                .Append(_indent).AppendLine("if t2 > t1 then ")
                .Append(_indent).AppendLine("  table.insert(stack, 1)")
                .Append(_indent).AppendLine("else")
                .Append(_indent).AppendLine("  table.insert(stack, 0)")
                .Append(_indent).AppendLine("end");
        }

        protected override void Xolonys()
        {
            _writer.Append(_indent).AppendLine("t1, t2 = table.remove(stack), table.remove(stack)")
                .Append(_indent).AppendLine("if t2 >= t1 then ")
                .Append(_indent).AppendLine("  table.insert(stack, 1)")
                .Append(_indent).AppendLine("else")
                .Append(_indent).AppendLine("  table.insert(stack, 0)")
                .Append(_indent).AppendLine("end");
        }

        protected override void Tikl()
        {
            _writer.Append(_indent).AppendLine("print(table.remove(stack))");
        }

        protected override void Krz()
        {
            _writer.Append(_indent).AppendLine("table.insert(stack, stack[#stack])");
        }

        protected override void Ach()
        {
            _writer.Append(_indent)
                .AppendLine("stack[#stack - 1], stack[#stack] = stack[#stack], stack[#stack - 1]");
        }

        protected override void Roft()
        {
            _writer.Append(_indent)
                .Append("stack[#stack - 2], stack[#stack - 1], stack[#stack]")
                .AppendLine(" = stack[#stack - 1], stack[#stack], stack[#stack - 2]");
        }

        protected override void Ycax()
        {
            _writer.Append(_indent).AppendLine("table.remove(stack)");
        }

        protected override void Pielyn()
        {
            _writer.Append(_indent).AppendLine("stack = {}");
        }

        protected override void Fal()
        {
            string oldIndent = _indent.ToString();
            _indent.Append("  ");

            _writer.Append(oldIndent).AppendLine("while stack[#stack] ~= 0 do");
        }

        protected override void Laf()
        {
            _indent.Remove(_indent.Length - 2, 2);

            _writer.Append(_indent).AppendLine("end");
        }

        protected override void Fi()
        {
            string oldIndent = _indent.ToString();
            _indent.Append("  ");

            _writer.Append(oldIndent).AppendLine("if stack[#stack] ~= 0 then");
        }

        protected override void Ol()
        {
            string oldIndent = _indent.ToString().Remove(_indent.Length - 2);

            _writer.Append(oldIndent).AppendLine("else");
        }

        protected override void If()
        {
            _indent.Remove(_indent.Length - 2, 2);

            _writer.Append(_indent).AppendLine("end");
        }

        protected override void Cecio()
        {
            string oldIndent = _indent.ToString();
            _indent.Append("  ");

            _writer.Append(oldIndent).AppendLine("while stack[#stack] <= stack[#stack - 1] do");
        }

        protected override void Oicec()
        {
            _indent.Remove(_indent.Length - 2, 2);

            _writer.Append(_indent).AppendLine("  stack[#stack] = stack[#stack] + 1")
                .Append(_indent).AppendLine("end");
        }

        protected override void Kinfit()
        {
            _writer.Append(_indent).AppendLine("table.insert(stack, #stack)");
        }

        protected override void Ata1()
        {
            _writer.Append(_indent).AppendLine("stack[#stack] = to32(stack[#stack] + 1)");
        }

        protected override void Nta1()
        {
            _writer.Append(_indent).AppendLine("stack[#stack] = to32(stack[#stack] - 1)");
        }

        protected override void RoftNia()
        {
            _writer.Append(_indent)
                .Append("stack[#stack - 2], stack[#stack - 1], stack[#stack]")
                .AppendLine(" = stack[#stack], stack[#stack - 2], stack[#stack - 1]");
        }

        protected override void Lat32()
        {
            _writer.Append(_indent).AppendLine("t1, t2 = table.remove(stack), stack[#stack]")
                .Append(_indent).AppendLine("stack[#stack] = to32(t2 * t1)");
        }

        protected override void Latsna32()
        {
            _writer.Append(_indent).AppendLine("t1, t2 = table.remove(stack), stack[#stack]")
                .Append(_indent).AppendLine("stack[#stack] = to32(to64s(t2) * to64s(t1))");
        }

    }
}

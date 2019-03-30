using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cent.Core
{
    class CentToWin64Nasm : CentTranscompiler
    {
        private const string indent = "  ";

        readonly Stack<string> jumpLabelStack;
        readonly Dictionary<string, int> labelCount;
        readonly StringBuilder writer;

        public CentToWin64Nasm(List<string> inFileNames) : base(inFileNames)
        {
            jumpLabelStack = new Stack<string>();
            labelCount = new Dictionary<string, int>()
            {
                ["cecio"] = 0,
                ["fal"] = 0,
                ["fi"] = 0,
            };
            this.writer = new StringBuilder();
        }

        public CentToWin64Nasm(string[] inFileNames) : this(inFileNames.ToList())
        {
        }

        protected override void PreProcess(string outFileName)
        {
            this.writer.AppendLine("bits 64")
                .AppendLine("extern printf");

            // インポートする関数の設定
            foreach (var func in this.funcNames)
            {
                this.writer.Append("extern ").AppendLine(func.Key);
            }

            this.writer.AppendLine("global main")
                .AppendLine("section .data")
                .Append(indent).AppendLine("disp: db '%u',13,10,0")
                .AppendLine("section .text")
                .AppendLine("main:")
                .Append(indent).AppendLine("push rbp")
                .Append(indent).AppendLine("mov rbp, rsp")
                .Append(indent).AppendLine("mov rbx, 0");
        }

        protected override void PostProcess(string outFileName)
        {
            this.writer.Append(indent).AppendLine("mov rax, 0")
                .Append(indent).AppendLine("leave")
                .Append(indent).AppendLine("ret");

            using (var file = new StreamWriter(outFileName, false, new UTF8Encoding(false)))
            {
                file.Write(this.writer.ToString());
            }
        }

        protected override void Fenxe(string funcName, uint argc)
        {
            if (argc == 0)
            {
            }
            else
            {
            }

            throw new NotSupportedException("");
        }

        protected override void Value(uint result)
        {
            this.writer.Append(indent).AppendLine("sub rsp, 8")
                .Append(indent).AppendFormat("push {0}", result).AppendLine()
                .Append(indent).AppendLine("add rbx, 1");
        }

        protected override void Nac()
        {
            this.writer.Append(indent).AppendLine("mov eax, [rsp]")
                .Append(indent).AppendLine("not eax")
                .Append(indent).AppendLine("mov [rsp], eax");
        }

        protected override void Sna()
        {
            this.writer.Append(indent).AppendLine("mov eax, [rsp]")
                .Append(indent).AppendLine("neg eax")
                .Append(indent).AppendLine("mov [rsp], eax");
        }

        protected override void Ata1()
        {
            this.writer.Append(indent).AppendLine("mov eax, [rsp]")
                .Append(indent).AppendLine("inc eax")
                .Append(indent).AppendLine("mov [rsp], eax");
        }

        protected override void Nta1()
        {
            this.writer.Append(indent).AppendLine("mov eax, [rsp]")
                .Append(indent).AppendLine("dec eax")
                .Append(indent).AppendLine("mov [rsp], eax");
        }

        protected override void Ata()
        {
            this.writer.Append(indent).AppendLine("mov eax, [rsp+16]")
                .Append(indent).AppendLine("add eax, [rsp]")
                .Append(indent).AppendLine("add rsp, 16")
                .Append(indent).AppendLine("mov [rsp], eax")
                .Append(indent).AppendLine("sub rbx, 1");
        }

        protected override void Nta()
        {
            this.writer.Append(indent).AppendLine("mov eax, [rsp+16]")
                .Append(indent).AppendLine("sub eax, [rsp]")
                .Append(indent).AppendLine("add rsp, 16")
                .Append(indent).AppendLine("mov [rsp], eax")
                .Append(indent).AppendLine("sub rbx, 1");
        }

        protected override void Ada()
        {
            this.writer.Append(indent).AppendLine("mov eax, [rsp+16]")
                .Append(indent).AppendLine("and eax, [rsp]")
                .Append(indent).AppendLine("add rsp, 16")
                .Append(indent).AppendLine("mov [rsp], eax")
                .Append(indent).AppendLine("sub rbx, 1");
        }

        protected override void Ekc()
        {
            this.writer.Append(indent).AppendLine("mov eax, [rsp+16]")
                .Append(indent).AppendLine("or eax, [rsp]")
                .Append(indent).AppendLine("add rsp, 16")
                .Append(indent).AppendLine("mov [rsp], eax")
                .Append(indent).AppendLine("sub rbx, 1");
        }

        protected override void Dto()
        {
            this.writer.Append(indent).AppendLine("mov eax, [rsp+16]")
                .Append(indent).AppendLine("shr eax, [rsp]")
                .Append(indent).AppendLine("add rsp, 16")
                .Append(indent).AppendLine("mov [rsp], eax")
                .Append(indent).AppendLine("sub rbx, 1");
        }

        protected override void Dro()
        {
            this.writer.Append(indent).AppendLine("mov eax, [rsp+16]")
                .Append(indent).AppendLine("shl eax, [rsp]")
                .Append(indent).AppendLine("add rsp, 16")
                .Append(indent).AppendLine("mov [rsp], eax")
                .Append(indent).AppendLine("sub rbx, 1");
        }

        protected override void Dtosna()
        {
            this.writer.Append(indent).AppendLine("mov eax, [rsp+16]")
                .Append(indent).AppendLine("sar eax, [rsp]")
                .Append(indent).AppendLine("add rsp, 16")
                .Append(indent).AppendLine("mov [rsp], eax")
                .Append(indent).AppendLine("sub rbx, 1");
        }

        protected override void Dal()
        {
            this.writer.Append(indent).AppendLine("mov eax, [rsp+16]")
                .Append(indent).AppendLine("xor eax, [rsp]")
                .Append(indent).AppendLine("not eax")
                .Append(indent).AppendLine("add rsp, 16")
                .Append(indent).AppendLine("mov [rsp], eax")
                .Append(indent).AppendLine("sub rbx, 1");
        }

        protected override void Lat()
        {
            this.writer.Append(indent).AppendLine("mov eax, [rsp+16]")
                .Append(indent).AppendLine("mul dword [rsp]")
                .Append(indent).AppendLine("mov [rsp+16], edx")
                .Append(indent).AppendLine("mov [rsp], eax");
        }

        protected override void Latsna()
        {
            this.writer.Append(indent).AppendLine("mov eax, [rsp+16]")
                .Append(indent).AppendLine("imul dword [rsp]")
                .Append(indent).AppendLine("mov [rsp+16], edx")
                .Append(indent).AppendLine("mov [rsp], eax");
        }

        protected override void Xtlo()
        {
            this.writer.Append(indent).AppendLine("mov eax, [rsp+16]")
                .Append(indent).AppendLine("cmp eax, [rsp]")
                .Append(indent).AppendLine("setle al")
                .Append(indent).AppendLine("movzx eax, al")
                .Append(indent).AppendLine("add rsp, 16")
                .Append(indent).AppendLine("mov [rsp], eax")
                .Append(indent).AppendLine("sub rbx, 1");
        }

        protected override void Xylo()
        {
            this.writer.Append(indent).AppendLine("mov eax, [rsp+16]")
                .Append(indent).AppendLine("cmp eax, [rsp]")
                .Append(indent).AppendLine("setl al")
                .Append(indent).AppendLine("movzx eax, al")
                .Append(indent).AppendLine("add rsp, 16")
                .Append(indent).AppendLine("mov [rsp], eax")
                .Append(indent).AppendLine("sub rbx, 1");
        }

        protected override void Clo()
        {
            this.writer.Append(indent).AppendLine("mov eax, [rsp+16]")
                .Append(indent).AppendLine("cmp eax, [rsp]")
                .Append(indent).AppendLine("sete al")
                .Append(indent).AppendLine("movzx eax, al")
                .Append(indent).AppendLine("add rsp, 16")
                .Append(indent).AppendLine("mov [rsp], eax")
                .Append(indent).AppendLine("sub rbx, 1");
        }

        protected override void Niv()
        {
            this.writer.Append(indent).AppendLine("mov eax, [rsp+16]")
                .Append(indent).AppendLine("cmp eax, [rsp]")
                .Append(indent).AppendLine("setne al")
                .Append(indent).AppendLine("movzx eax, al")
                .Append(indent).AppendLine("add rsp, 16")
                .Append(indent).AppendLine("mov [rsp], eax")
                .Append(indent).AppendLine("sub rbx, 1");
        }

        protected override void Llo()
        {
            this.writer.Append(indent).AppendLine("mov eax, [rsp+16]")
                .Append(indent).AppendLine("cmp eax, [rsp]")
                .Append(indent).AppendLine("setg al")
                .Append(indent).AppendLine("movzx eax, al")
                .Append(indent).AppendLine("add rsp, 16")
                .Append(indent).AppendLine("mov [rsp], eax")
                .Append(indent).AppendLine("sub rbx, 1");
        }

        protected override void Xolo()
        {
            this.writer.Append(indent).AppendLine("mov eax, [rsp+16]")
                .Append(indent).AppendLine("cmp eax, [rsp]")
                .Append(indent).AppendLine("setge al")
                .Append(indent).AppendLine("movzx eax, al")
                .Append(indent).AppendLine("add rsp, 16")
                .Append(indent).AppendLine("mov [rsp], eax")
                .Append(indent).AppendLine("sub rbx, 1");
        }

        protected override void Xtlonys()
        {
            this.writer.Append(indent).AppendLine("mov eax, [rsp+16]")
                .Append(indent).AppendLine("cmp eax, [rsp]")
                .Append(indent).AppendLine("setbe al")
                .Append(indent).AppendLine("movzx eax, al")
                .Append(indent).AppendLine("add rsp, 16")
                .Append(indent).AppendLine("mov [rsp], eax")
                .Append(indent).AppendLine("sub rbx, 1");
        }

        protected override void Xylonys()
        {
            this.writer.Append(indent).AppendLine("mov eax, [rsp+16]")
                .Append(indent).AppendLine("cmp eax, [rsp]")
                .Append(indent).AppendLine("setb al")
                .Append(indent).AppendLine("movzx eax, al")
                .Append(indent).AppendLine("add rsp, 16")
                .Append(indent).AppendLine("mov [rsp], eax")
                .Append(indent).AppendLine("sub rbx, 1");
        }

        protected override void Llonys()
        {
            this.writer.Append(indent).AppendLine("mov eax, [rsp+16]")
                .Append(indent).AppendLine("cmp eax, [rsp]")
                .Append(indent).AppendLine("seta al")
                .Append(indent).AppendLine("movzx eax, al")
                .Append(indent).AppendLine("add rsp, 16")
                .Append(indent).AppendLine("mov [rsp], eax")
                .Append(indent).AppendLine("sub rbx, 1");
        }

        protected override void Xolonys()
        {
            this.writer.Append(indent).AppendLine("mov eax, [rsp+16]")
                .Append(indent).AppendLine("cmp eax, [rsp]")
                .Append(indent).AppendLine("setae al")
                .Append(indent).AppendLine("movzx eax, al")
                .Append(indent).AppendLine("add rsp, 16")
                .Append(indent).AppendLine("mov [rsp], eax")
                .Append(indent).AppendLine("sub rbx, 1");
        }

        protected override void Tikl()
        {
            this.writer.Append(indent).AppendLine("mov edx, [rsp]")
                .Append(indent).AppendLine("lea rcx, [disp]")
                .Append(indent).AppendLine("sub rsp, 32")
                .Append(indent).AppendLine("call printf")
                .Append(indent).AppendLine("add rsp, 48")
                .Append(indent).AppendLine("sub rbx, 1");
        }

        protected override void Krz()
        {
            this.writer.Append(indent).AppendLine("mov eax, [rsp]")
                .Append(indent).AppendLine("sub rsp, 16")
                .Append(indent).AppendLine("mov [rsp], eax")
                .Append(indent).AppendLine("add rbx, 1");
        }

        protected override void Ach()
        {
            this.writer.Append(indent).AppendLine("mov edx, [rsp]")
                .Append(indent).AppendLine("mov eax, [rsp+16]")
                .Append(indent).AppendLine("mov [rsp], eax")
                .Append(indent).AppendLine("mov [rsp+16], edx");
        }

        protected override void Roft()
        {
            this.writer.Append(indent).AppendLine("mov ecx, [rsp]")
                .Append(indent).AppendLine("mov edx, [rsp+16]")
                .Append(indent).AppendLine("mov eax, [rsp+32]")
                .Append(indent).AppendLine("mov [rsp], eax")
                .Append(indent).AppendLine("mov [rsp+16], ecx")
                .Append(indent).AppendLine("mov [rsp+32], edx");
        }

        protected override void Ycax()
        {
            this.writer.Append(indent).AppendLine("add rsp, 16")
                .Append(indent).AppendLine("sub rbx, 1");
        }

        protected override void Pielyn()
        {
            this.writer.Append(indent).AppendLine("shl rbx, 4")
                .Append(indent).AppendLine("add rsp, rbx")
                .Append(indent).AppendLine("mov rbx, 0");
        }

        protected override void Fal()
        {
            int count = this.labelCount["fal"]++;
            string falLabel = $"_fal{count}";
            string lafLabel = $"_laf{count}";

            this.jumpLabelStack.Push(lafLabel);
            this.jumpLabelStack.Push(falLabel);

            this.writer.Append(falLabel).AppendLine(":")
                .Append(indent).AppendLine("cmp dword [rsp], 0")
                .Append(indent).Append("je ").AppendLine(lafLabel);
        }

        protected override void Laf()
        {
            if (!this.jumpLabelStack.Peek().StartsWith("_fal"))
            {
                throw new ApplicationException("'laf' cannot be here");
            }

            this.writer.Append(indent).Append("jmp ").AppendLine(this.jumpLabelStack.Pop())
                .Append(this.jumpLabelStack.Pop()).AppendLine(":");
        }

        protected override void Fi()
        {
            int count = this.labelCount["fi"]++;
            string olLabel = $"_ol{count}";
            string ifLabel = $"_if{count}";

            this.jumpLabelStack.Push(ifLabel);
            this.jumpLabelStack.Push(olLabel);

            this.writer.Append(indent).AppendLine("cmp dword [rsp], 0")
                .Append(indent).Append("je ").AppendLine(olLabel);
        }

        protected override void Ol()
        {
            if (!this.jumpLabelStack.Peek().StartsWith("_ol"))
            {
                throw new ApplicationException("'ol' cannot be here");
            }

            string label = this.jumpLabelStack.Pop();
            this.writer.Append(indent).Append("jmp ").AppendLine(this.jumpLabelStack.Peek())
                .Append(label).AppendLine(":");
        }

        protected override void If()
        {
            string label = this.jumpLabelStack.Pop();

            if (!(label.StartsWith("_ol") || label.StartsWith("_if")))
            {
                throw new ApplicationException("'if' cannot be here");
            }

            if (label.StartsWith("_ol"))
            {
                this.jumpLabelStack.Pop();
            }

            this.writer.Append(label).AppendLine(":");
        }

        protected override void Cecio()
        {
            int count = this.labelCount["cecio"]++;
            string oicecLabel = $"_oicec{count}";
            string cecioLabel = $"_cecio{count}";

            this.jumpLabelStack.Push(oicecLabel);
            this.jumpLabelStack.Push(cecioLabel);

            this.writer.Append(cecioLabel).AppendLine(":")
                .Append(indent).AppendLine("mov eax, [rsp]")
                .Append(indent).AppendLine("cmp eax, [rsp+16]")
                .Append(indent).Append("jg ").AppendLine(oicecLabel);
        }

        protected override void Oicec()
        {
            if (!this.jumpLabelStack.Peek().StartsWith("_cecio"))
            {
                throw new ApplicationException("'oicec' cannot be here");
            }

            this.writer.Append(indent).AppendLine("add dword [rsp], 1")
                .Append(indent).Append("jmp ").AppendLine(this.jumpLabelStack.Pop())
                .Append(this.jumpLabelStack.Pop()).AppendLine(":")
                .Append(indent).AppendLine("add rsp, 32");
        }

        protected override void Kinfit()
        {
            this.writer.Append(indent).AppendLine("mov rax, rbx")
                .Append(indent).AppendLine("sub rsp, 8")
                .Append(indent).AppendLine("push rax")
                .Append(indent).AppendLine("add rbx, 1");
        }
    }
}

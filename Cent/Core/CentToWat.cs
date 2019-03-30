using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Cent.Core
{
    class CentToWat : CentTranscompiler
    {
        readonly Stack<string> jumpLabelStack;
        readonly Dictionary<string, int> labelCount;

        readonly StringBuilder indent;
        readonly StringBuilder writer;

        public CentToWat(List<string> inFileNames) : base(inFileNames)
        {
            jumpLabelStack = new Stack<string>();
            labelCount = new Dictionary<string, int>()
            {
                ["cecio"] = 0,
                ["fal"] = 0,
                ["fi"] = 0,
                ["leles"] = 0,
            };

            indent = new StringBuilder("");
            writer = new StringBuilder();
        }

        public CentToWat(string[] inFileNames) : this(inFileNames.ToList())
        {
        }

        protected override void PreProcess(string outFileName)
        {
            this.writer.AppendLine("(module");

            // インポートする関数の設定
            foreach (var func in this.funcNames)
            {

                this.writer.AppendFormat("  (import \"cent\" \"{0}\" (func ${0} (param", func.Key);
                for (uint i = 0; i < func.Value; i++)
                {
                    this.writer.Append(" i32");
                }
                this.writer.AppendLine(") (result i32)))");
            }

            this.writer.AppendLine("  (import \"native\" \"out\" (func $out (param i32)))")
                .AppendLine("  (memory $memory (export \"memory\") 1)")
                .AppendLine("  (func (export \"main\") (result i32) (local $count i32) (local $tmp32 i32) (local $tmp64 i64)")
                .AppendLine("    i32.const -4 set_local $count");

            this.indent.Append("    ");
        }

        protected override void PostProcess(string outFileName)
        {
            this.writer.AppendLine("    get_local $count i32.const 4 i32.div_s i32.const 1 i32.add")
                .AppendLine("  )").AppendLine(")");

            using (var file = new StreamWriter(outFileName, false, new UTF8Encoding(false)))
            {
                file.Write(this.writer.ToString());
            }
        }
        
        protected override void Fenxe(string funcName, uint argc)
        {
            if (argc != 0)
            {
                this.writer.Append(this.indent)
                    .AppendFormat("get_local $count i32.const {0}", argc * 4)
                    .AppendLine(" i32.sub set_local $count");
            }

            for (var i = 1; i <= argc; i++)
            {
                this.writer.Append(this.indent)
                    .AppendFormat("get_local $count i32.const {0} i32.add i32.load ;;  [{1}]", i * 4, i)
                    .AppendLine();
            }

            writer.Append(this.indent).Append("call $").AppendLine(funcName)
                .Append(this.indent).AppendLine("set_local $tmp32")
                .Append(this.indent).AppendLine("get_local $count i32.const 4 i32.add tee_local $count get_local $tmp32 i32.store");
        }

        protected override void Value(uint result)
        {
            this.writer.Append(this.indent).AppendLine("get_local $count i32.const 4 i32.add tee_local $count")
                .Append(this.indent).Append("i32.const ").Append(result).AppendLine(" i32.store");
        }

        protected override void Nac()
        {
            this.writer.Append(this.indent).AppendLine("get_local $count get_local $count i32.load")
                .Append(this.indent).AppendLine("i32.const -1 i32.xor i32.store");
        }

        protected override void Sna()
        {
            this.writer.Append(this.indent).AppendLine("get_local $count get_local $count i32.load")
                .Append(this.indent).AppendLine("i32.const -1 i32.xor i32.const 1 i32.add i32.store");
        }

        protected override void Ata1()
        {
            this.writer.Append(this.indent).Append("get_local $count get_local $count i32.load")
                .Append(this.indent).AppendLine("i32.const 1 i32.add i32.store");
        }

        protected override void Nta1()
        {
            this.writer.Append(this.indent).Append("get_local $count get_local $count i32.load")
                .Append(this.indent).AppendLine("i32.const 1 i32.sub i32.store");
        }

        protected override void Ata()
        {
            this.writer.Append(this.indent).AppendLine("get_local $count i32.const 4 i32.sub tee_local $count")
                .Append(this.indent).AppendLine("get_local $count i32.load")
                .Append(this.indent).AppendLine("get_local $count i32.const 4 i32.add i32.load")
                .Append(this.indent).AppendLine("i32.add i32.store");
        }

        protected override void Nta()
        {
            this.writer.Append(this.indent).AppendLine("get_local $count i32.const 4 i32.sub tee_local $count")
                .Append(this.indent).AppendLine("get_local $count i32.load")
                .Append(this.indent).AppendLine("get_local $count i32.const 4 i32.add i32.load")
                .Append(this.indent).AppendLine("i32.sub i32.store");
        }

        protected override void Ada()
        {
            this.writer.Append(this.indent).AppendLine("get_local $count i32.const 4 i32.sub tee_local $count")
                .Append(this.indent).AppendLine("get_local $count i32.load")
                .Append(this.indent).AppendLine("get_local $count i32.const 4 i32.add i32.load")
                .Append(this.indent).AppendLine("i32.and i32.store");
        }

        protected override void Ekc()
        {
            this.writer.Append(this.indent).AppendLine("get_local $count i32.const 4 i32.sub tee_local $count")
                .Append(this.indent).AppendLine("get_local $count i32.load")
                .Append(this.indent).AppendLine("get_local $count i32.const 4 i32.add i32.load")
                .Append(this.indent).AppendLine("i32.or i32.store");
        }

        protected override void Dto()
        {
            this.writer.Append(this.indent).AppendLine("get_local $count i32.const 4 i32.sub tee_local $count")
                .Append(this.indent).AppendLine("get_local $count i32.load")
                .Append(this.indent).AppendLine("get_local $count i32.const 4 i32.add i32.load")
                .Append(this.indent).AppendLine("i32.shr_u i32.store");
        }

        protected override void Dro()
        {
            this.writer.Append(this.indent).AppendLine("get_local $count i32.const 4 i32.sub tee_local $count")
                .Append(this.indent).AppendLine("get_local $count i32.load")
                .Append(this.indent).AppendLine("get_local $count i32.const 4 i32.add i32.load")
                .Append(this.indent).AppendLine("i32.shl i32.store");
        }

        protected override void Dtosna()
        {
            this.writer.Append(this.indent).AppendLine("get_local $count i32.const 4 i32.sub tee_local $count")
                .Append(this.indent).AppendLine("get_local $count i32.load")
                .Append(this.indent).AppendLine("get_local $count i32.const 4 i32.add i32.load")
                .Append(this.indent).AppendLine("i32.shr_s i32.store");
        }

        protected override void Dal()
        {
            this.writer.Append(this.indent).AppendLine("get_local $count i32.const 4 i32.sub tee_local $count")
                .Append(this.indent).AppendLine("get_local $count i32.load")
                .Append(this.indent).AppendLine("get_local $count i32.const 4 i32.add i32.load")
                .Append(this.indent).AppendLine("i32.xor i32.const -1 i32.xor i32.store");
        }

        protected override void Lat()
        {
            this.writer.Append(this.indent).AppendLine("get_local $count i32.const 4 i32.sub i32.load i64.extend_u/i32")
                .Append(this.indent).AppendLine("get_local $count i32.load i64.extend_u/i32")
                .Append(this.indent).AppendLine("i64.mul set_local $tmp64")
                .Append(this.indent).AppendLine("get_local $count i32.const 4 i32.sub get_local $tmp64 i32.wrap/i64 i32.store")
                .Append(this.indent).AppendLine("get_local $count get_local $tmp64 i64.const 32 i64.shr_u i32.wrap/i64 i32.store");
        }

        protected override void Latsna()
        {
            this.writer.Append(this.indent).AppendLine("get_local $count i32.const 4 i32.sub i32.load i64.extend_s/i32")
                .Append(this.indent).AppendLine("get_local $count i32.load i64.extend_s/i32")
                .Append(this.indent).AppendLine("i64.mul set_local $tmp64")
                .Append(this.indent).AppendLine("get_local $count i32.const 4 i32.sub get_local $tmp64 i32.wrap/i64 i32.store")
                .Append(this.indent).AppendLine("get_local $count get_local $tmp64 i64.const 32 i64.shr_u i32.wrap/i64 i32.store");
        }

        protected override void Xtlo()
        {
            this.writer.Append(this.indent).AppendLine("get_local $count i32.const 4 i32.sub tee_local $count")
                .Append(this.indent).AppendLine("get_local $count i32.load")
                .Append(this.indent).AppendLine("get_local $count i32.const 4 i32.add i32.load")
                .Append(this.indent).AppendLine("i32.ge_s i32.store");
        }

        protected override void Xylo()
        {
            this.writer.Append(this.indent).AppendLine("get_local $count i32.const 4 i32.sub tee_local $count")
                .Append(this.indent).AppendLine("get_local $count i32.load")
                .Append(this.indent).AppendLine("get_local $count i32.const 4 i32.add i32.load")
                .Append(this.indent).AppendLine("i32.gt_s i32.store");
        }

        protected override void Clo()
        {
            this.writer.Append(this.indent).AppendLine("get_local $count i32.const 4 i32.sub tee_local $count")
                .Append(this.indent).AppendLine("get_local $count i32.load")
                .Append(this.indent).AppendLine("get_local $count i32.const 4 i32.add i32.load")
                .Append(this.indent).AppendLine("i32.eq i32.store");
        }

        protected override void Niv()
        {
            this.writer.Append(this.indent).AppendLine("get_local $count i32.const 4 i32.sub tee_local $count")
                .Append(this.indent).AppendLine("get_local $count i32.load")
                .Append(this.indent).AppendLine("get_local $count i32.const 4 i32.add i32.load")
                .Append(this.indent).AppendLine("i32.ne i32.store");
        }

        protected override void Llo()
        {
            this.writer.Append(this.indent).AppendLine("get_local $count i32.const 4 i32.sub tee_local $count")
                .Append(this.indent).AppendLine("get_local $count i32.load")
                .Append(this.indent).AppendLine("get_local $count i32.const 4 i32.add i32.load")
                .Append(this.indent).AppendLine("i32.lt_s i32.store");
        }

        protected override void Xolo()
        {
            this.writer.Append(this.indent).AppendLine("get_local $count i32.const 4 i32.sub tee_local $count")
                .Append(this.indent).AppendLine("get_local $count i32.load")
                .Append(this.indent).AppendLine("get_local $count i32.const 4 i32.add i32.load")
                .Append(this.indent).AppendLine("i32.le_s i32.store");
        }

        protected override void Xtlonys()
        {
            this.writer.Append(this.indent).AppendLine("get_local $count i32.const 4 i32.sub tee_local $count")
                .Append(this.indent).AppendLine("get_local $count i32.load")
                .Append(this.indent).AppendLine("get_local $count i32.const 4 i32.add i32.load")
                .Append(this.indent).AppendLine("i32.ge_u i32.store");
        }

        protected override void Xylonys()
        {
            this.writer.Append(this.indent).AppendLine("get_local $count i32.const 4 i32.sub tee_local $count")
                .Append(this.indent).AppendLine("get_local $count i32.load")
                .Append(this.indent).AppendLine("get_local $count i32.const 4 i32.add i32.load")
                .Append(this.indent).AppendLine("i32.gt_u i32.store");
        }

        protected override void Llonys()
        {
            this.writer.Append(this.indent).AppendLine("get_local $count i32.const 4 i32.sub tee_local $count")
                .Append(this.indent).AppendLine("get_local $count i32.load")
                .Append(this.indent).AppendLine("get_local $count i32.const 4 i32.add i32.load")
                .Append(this.indent).AppendLine("i32.lt_u i32.store");
        }

        protected override void Xolonys()
        {
            this.writer.Append(this.indent).AppendLine("get_local $count i32.const 4 i32.sub tee_local $count")
                .Append(this.indent).AppendLine("get_local $count i32.load")
                .Append(this.indent).AppendLine("get_local $count i32.const 4 i32.add i32.load")
                .Append(this.indent).AppendLine("i32.le_u i32.store");
        }

        protected override void Tikl()
        {
            this.writer.Append(this.indent).AppendLine("get_local $count i32.load")
                .Append(this.indent).AppendLine("call $out")
                .Append(this.indent).AppendLine("get_local $count i32.const 4 i32.sub set_local $count");
        }

        protected override void Krz()
        {
            this.writer.Append(this.indent).AppendLine("get_local $count i32.const 4 i32.add tee_local $count")
                .Append(this.indent).AppendLine("get_local $count i32.const 4 i32.sub i32.load i32.store");
        }

        protected override void Ach()
        {
            this.writer.Append(this.indent).AppendLine("get_local $count i32.const 4 i32.sub")
                .Append(this.indent).AppendLine("get_local $count i32.load")
                .Append(this.indent).AppendLine("get_local $count")
                .Append(this.indent).AppendLine("get_local $count i32.const 4 i32.sub i32.load")
                .Append(this.indent).AppendLine("i32.store i32.store");
        }

        protected override void Roft()
        {
            this.writer.Append(this.indent).AppendLine("get_local $count i32.const 8 i32.sub")
                .Append(this.indent).AppendLine("get_local $count i32.const 4 i32.sub i32.load")
                .Append(this.indent).AppendLine("get_local $count i32.const 4 i32.sub")
                .Append(this.indent).AppendLine("get_local $count i32.load")
                .Append(this.indent).AppendLine("get_local $count")
                .Append(this.indent).AppendLine("get_local $count i32.const 8 i32.sub i32.load")
                .Append(this.indent).AppendLine("i32.store i32.store i32.store");
        }

        protected override void Ycax()
        {
            this.writer.Append(this.indent).AppendLine("get_local $count i32.const 4 i32.sub set_local $count");
        }

        protected override void Pielyn()
        {
            this.writer.Append(this.indent).AppendLine("i32.const -4 set_local $count");
        }

        protected override void Fal()
        {
            string oldIndent = this.indent.ToString();
            this.indent.Append("  ");

            int count = this.labelCount["fal"]++;
            string fal = "$fal" + count;
            string laf = "$laf" + count;
            this.jumpLabelStack.Push(fal);

            this.writer.Append(oldIndent).AppendFormat("block {0} loop {1}", laf, fal).AppendLine()
                .Append(this.indent).AppendLine("get_local $count i32.load i32.eqz")
                .Append(this.indent).Append("br_if ").AppendLine(laf);
        }

        protected override void Laf()
        {
            string oldIndent = this.indent.ToString();
            this.indent.Remove(this.indent.Length - 2, 2);

            string fal = this.jumpLabelStack.Pop();

            if (fal.StartsWith("$fal"))
            {
                this.writer.Append(oldIndent).AppendFormat("br {0}", fal).AppendLine()
                    .Append(this.indent).AppendLine("end end");
            }
            else
            {
                throw new ApplicationException("'laf' cannot be here");
            }
        }

        protected override void Fi()
        {
            this.writer.Append(this.indent).AppendLine("get_local $count i32.load if");

            this.indent.Append("  ");
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

            int count = this.labelCount["cecio"]++;
            string cecio = "$cecio" + count;
            string oicec = "$oicec" + count;
            this.jumpLabelStack.Push(cecio);

            this.writer.Append(oldIndent).AppendFormat("block {0} loop {1}", oicec, cecio).AppendLine()
                .Append(this.indent).AppendLine("get_local $count i32.const 4 i32.sub i32.load get_local $count i32.load i32.lt_s")
                .Append(this.indent).AppendLine("if get_local $count i32.const 8 i32.sub set_local $count")
                .Append(this.indent).AppendLine("  br ").AppendLine(oicec)
                .Append(this.indent).AppendLine("end");
        }

        protected override void Oicec()
        {
            string oldIndent = this.indent.ToString();
            this.indent.Remove(this.indent.Length - 2, 2);

            string cecio = this.jumpLabelStack.Pop();
            if (cecio.StartsWith("$cecio"))
            {
                this.writer.Append(oldIndent).AppendLine("get_local $count get_local $count i32.load i32.const 1 i32.add i32.store")
                    .Append(oldIndent).Append("br ").AppendLine(cecio)
                    .Append(this.indent).AppendLine("end end");
            }
            else
            {
                throw new ApplicationException("'oicec' cannot be here");
            }
        }

        protected override void Kinfit()
        {
            this.writer.Append(this.indent).AppendLine("get_local $count i32.const 4 i32.add tee_local $count")
                .Append(this.indent).AppendLine("get_local $count i32.const 4 i32.div_s i32.store");
        }
    }
}

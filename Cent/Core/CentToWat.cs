using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cent.Core
{
    class CentToWat : CentTranscompiler
    {
        readonly List<string> subroutineNames;
        readonly Dictionary<string, int> funcNames;
        readonly Stack<string> jumpLabelStack;
        readonly Stack<string> callSubroutines;
        readonly Dictionary<string, int> labelCount;

        public CentToWat(List<string> inFileNames) : base(inFileNames)
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
        }

        public CentToWat(string[] inFileNames) : this(inFileNames.ToList())
        {
        }

        protected override void Write(string outFileName)
        {
            using (var writer = new StringWriter())
            {
                writer.WriteLine("(module");

                foreach (var subrt in this.subroutines)
                {
                    if (subrt[0] == "xok")
                    {
                        writer.Write("  (import \"cent\" \"{0}\" (func ${0} (param", subrt[1]);
                        var argc = int.Parse(subrt[2]);
                        for (int i = 0; i < argc; i++)
                        {
                            writer.Write(" i32");
                        }
                        writer.WriteLine(")))");
                        this.funcNames.Add(subrt[1], argc);
                    }
                    else
                    {
                        this.subroutineNames.Add(subrt[0]);
                    }
                }
                
                writer.WriteLine("  (memory $memory (export \"memory\") 1)");
                writer.WriteLine("  (func (export \"main\") (result i32) (local $count i32) (local $tmp32 i32) (local $tmp64 i64)");
                writer.WriteLine("    i32.const -4 set_local $count");

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
                
                writer.WriteLine("    get_local $count i32.const 4 i32.div_s i32.const 1 i32.add");
                writer.WriteLine("  )\n)");


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
                writer.WriteLine("    get_local $count i32.const 4 i32.add tee_local $count");
                writer.WriteLine("    i32.const {0} i32.store ;; {0}", item);
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
                throw new ApplicationException($"Unknown word: '{item}'");
            }
            else if (this.funcNames.ContainsKey(item))
            {
                int argc = this.funcNames[item];

                if (argc == 0)
                {
                    writer.WriteLine("    get_local $count get_local $count");
                }
                else
                {
                    writer.WriteLine("    get_local $count i32.const {0} i32.sub tee_local $count get_local $count", argc * 4);
                }

                for (var i = 1; i <= argc; i++)
                {
                    writer.WriteLine("    get_local $count i32.const {0} i32.add i32.load ;;  {1}", i * 4, i);
                }
                writer.WriteLine("    call ${0} ;; fenxe {0}", item);
                writer.WriteLine("    i32.store i32.const 4 i32.add set_local $count ;; zalizales");
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
                    return "    get_local $count get_local $count i32.load\n" +
                        "    i32.const 0xffffffff i32.xor i32.const 1 i32.add i32.store ;; nac";
                case "ata":
                    return "    get_local $count i32.const 4 i32.sub tee_local $count\n" +
                        "    get_local $count i32.load\n" +
                        "    get_local $count i32.const 4 i32.add i32.load\n" +
                        "    i32.add i32.store ;; ata";
                case "nta":
                    return "    get_local $count i32.const 4 i32.sub tee_local $count\n" +
                        "    get_local $count i32.load\n" +
                        "    get_local $count i32.const 4 i32.add i32.load\n" +
                        "    i32.sub i32.store ;; nta";
                case "ada":
                    return "    get_local $count i32.const 4 i32.sub tee_local $count\n" +
                        "    get_local $count i32.load\n" +
                        "    get_local $count i32.const 4 i32.add i32.load\n" +
                        "    i32.and i32.store ;; ada";
                case "ekc":
                    return "    get_local $count i32.const 4 i32.sub tee_local $count\n" +
                        "    get_local $count i32.load\n" +
                        "    get_local $count i32.const 4 i32.add i32.load\n" +
                        "    i32.or i32.store ;; ekc";
                case "dto":
                    return "    get_local $count i32.const 4 i32.sub tee_local $count\n" +
                        "    get_local $count i32.load\n" +
                        "    get_local $count i32.const 4 i32.add i32.load\n" +
                        "    i32.shr_u i32.store ;; dto";
                case "dro":
                case "dRo":
                    return "    get_local $count i32.const 4 i32.sub tee_local $count\n" +
                        "    get_local $count i32.load\n" +
                        "    get_local $count i32.const 4 i32.add i32.load\n" +
                        "    i32.shl i32.store ;; dro";
                case "dtosna":
                    return "    get_local $count i32.const 4 i32.sub tee_local $count\n" +
                        "    get_local $count i32.load\n" +
                        "    get_local $count i32.const 4 i32.add i32.load\n" +
                        "    i32.shr_s i32.store ;; dtosna";
                case "dal":
                    return "    get_local $count i32.const 4 i32.sub tee_local $count\n" +
                        "    get_local $count i32.load\n" +
                        "    get_local $count i32.const 4 i32.add i32.load\n" +
                        "    i32.xor i32.const 0xffffffff i32.xor i32.store ;; dal";
                case "lat":
                    return "    get_local $count i32.const 4 i32.sub i32.load i64.extend_u/i32\n" +
                        "    get_local $count i32.load i64.extend_u/i32\n" +
                        "    i64.mul set_local $tmp64\n" +
                        "    get_local $count i32.const 4 i32.sub get_local $tmp64 i32.wrap/i64 i32.store" +
                        "    get_local $count get_local $tmp64 i64.const 32 i64.shr_u i32.wrap/i64 i32.store ;; lat";
                case "latsna":
                    return "    get_local $count i32.const 4 i32.sub i32.load i64.extend_s/i32\n" +
                        "    get_local $count i32.load i64.extend_s/i32\n" +
                        "    i64.mul set_local $tmp64\n" +
                        "    get_local $count i32.const 4 i32.sub get_local $tmp64 i32.wrap/i64 i32.store" +
                        "    get_local $count get_local $tmp64 i64.const 32 i64.shr_s i32.wrap/i64 i32.store ;; latsna";
                default:
                    throw new ApplicationException($"Invalid operation: {operation}");
            }
        }

        private string FromCompareOperator(string operation, int operandCount)
        {
            switch(operation)
            {
                case "xtlo":
                    return "    get_local $count i32.const 4 i32.sub tee_local $count\n" +
                        "    get_local $count i32.load\n" +
                        "    get_local $count i32.const 4 i32.add i32.load\n" +
                        "    i32.ge_s i32.store ;; xtlo";
                case "xylo":
                    return "    get_local $count i32.const 4 i32.sub tee_local $count\n" +
                        "    get_local $count i32.load\n" +
                        "    get_local $count i32.const 4 i32.add i32.load\n" +
                        "    i32.gt_s i32.store ;; xylo";
                case "clo":
                    return "    get_local $count i32.const 4 i32.sub tee_local $count\n" +
                        "    get_local $count i32.load\n" +
                        "    get_local $count i32.const 4 i32.add i32.load\n" +
                        "    i32.eq i32.store ;; clo";
                case "niv":
                    return "    get_local $count i32.const 4 i32.sub tee_local $count\n" +
                        "    get_local $count i32.load\n" +
                        "    get_local $count i32.const 4 i32.add i32.load\n" +
                        "    i32.ne i32.store ;; niv";
                case "llo":
                    return "    get_local $count i32.const 4 i32.sub tee_local $count\n" +
                        "    get_local $count i32.load\n" +
                        "    get_local $count i32.const 4 i32.add i32.load\n" +
                        "    i32.lt_s i32.store ;; llo";
                case "xolo":
                    return "    get_local $count i32.const 4 i32.sub tee_local $count\n" +
                        "    get_local $count i32.load\n" +
                        "    get_local $count i32.const 4 i32.add i32.load\n" +
                        "    i32.le_s i32.store ;; xolo";
                case "xtlonys":
                    return "    get_local $count i32.const 4 i32.sub tee_local $count\n" +
                        "    get_local $count i32.load\n" +
                        "    get_local $count i32.const 4 i32.add i32.load\n" +
                        "    i32.ge_u i32.store ;; xtlonys";
                case "xylonys":
                    return "    get_local $count i32.const 4 i32.sub tee_local $count\n" +
                        "    get_local $count i32.load\n" +
                        "    get_local $count i32.const 4 i32.add i32.load\n" +
                        "    i32.gt_u i32.store ;; xylonys";
                case "llonys":
                    return "    get_local $count i32.const 4 i32.sub tee_local $count\n" +
                        "    get_local $count i32.load\n" +
                        "    get_local $count i32.const 4 i32.add i32.load\n" +
                        "    i32.lt_u i32.store ;; llonys";
                case "xolonys":
                    return "    get_local $count i32.const 4 i32.sub tee_local $count\n" +
                        "    get_local $count i32.load\n" +
                        "    get_local $count i32.const 4 i32.add i32.load\n" +
                        "    i32.le_u i32.store ;; xolonys";
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
                    return "    get_local $count i32.const 4 i32.add tee_local $count\n" +
                        "    get_local $count i32.const 4 i32.sub i32.load i32.store ;; krz";
                case "ach":
                    return "    get_local $count i32.const 4 i32.sub\n" +
                        "    get_local $count i32.load\n" +
                        "    get_local $count\n" +
                        "    get_local $count i32.const 4 i32.sub i32.load\n" +
                        "    i32.store i32.store ;; ach";
                case "roft":
                    return "    get_local $count i32.const 8 i32.sub\n" +
                        "    get_local $count i32.const 4 i32.sub i32.load\n" +
                        "    get_local $count i32.const 4 i32.sub\n" +      
                        "    get_local $count i32.load\n" +
                        "    get_local $count\n" +
                        "    get_local $count i32.const 8 i32.sub i32.load\n" +
                        "    i32.store i32.store i32.store ;; roft";
                case "ycax":
                    return "    get_local $count i32.const 4 i32.sub set_local $count ;; ycax";
                case "pielyn":
                    return "    i32.const -4 set_local $count ;; pielyn";
                case "fal":
                    {
                        int count = this.labelCount["fal"]++;
                        string fal = "$fal" + count;
                        string laf = "$laf" + count;
                        this.jumpLabelStack.Push(fal);

                        return "    block " + laf + "\n"+
                            "    loop " + fal + "\n" +
                            "    get_local $count i32.load i32.eqz\n" +
                            "    br_if "+ laf + ";; fal";
                    }
                case "laf":
                    {
                        string fal = this.jumpLabelStack.Pop();

                        if (fal.StartsWith("$fal"))
                        {
                            return "    br " + fal + "\n" +
                                "    end\n" +
                                "    end ;; laf";
                        }
                        else
                        {
                            throw new ApplicationException("'laf' cannot be here");
                        }
                    }
                case "fi":
                    return "    get_local $count i32.load if ;; fi";
                case "ol":
                    return "    else ;; ol";
                case "if":
                    return "    end ;; if";
                case "cecio":
                    {
                        int count = this.labelCount["cecio"]++;
                        string cecio = "$cecio" + count;
                        string oicec = "$oicec" + count;
                        this.jumpLabelStack.Push(cecio);

                        return "    block " + oicec + "\n" +
                            "    loop " + cecio + "\n" +
                            "    get_local $count i32.const 4 i32.sub i32.load get_local $count i32.load i32.lt_s\n" +
                            "    br_if " + oicec + " ;; cecio";
                    }
                case "oicec":
                    {
                        string cecio = this.jumpLabelStack.Pop();
                        if (cecio.StartsWith("$cecio"))
                        {
                            return "    br "+ cecio + "\n" +
                                "    end\n" +
                                "    end ;; oicec";
                        }
                        else
                        {
                            throw new ApplicationException("'oicec' cannot be here");
                        }
                    }
                case "kinfit":
                    return "    get_local $count i32.const 4 i32.add tee_local $count\n" +
                        "    get_local $count i32.const 4 i32.div_s i32.store ;; kinfit";
                default:
                    throw new ApplicationException($"Invalid operation: {operation}");
            }
        }
    }
}

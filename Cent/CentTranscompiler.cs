using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cent
{
    abstract class CentTranscompiler
    {
        protected static readonly IReadOnlyDictionary<string, int> operatorMap;

        protected static readonly IReadOnlyDictionary<string, int> centOperatorMap;

        protected static readonly IReadOnlyDictionary<string, int> compareMap;

        static CentTranscompiler ()
        {
            operatorMap = new System.Collections.ObjectModel.ReadOnlyDictionary<string, int>(
                new Dictionary<string, int>
                {
                    ["nac"] = 1,["ata"] = 2, ["nta"] = 2,
                    ["kak"] = 2, ["ada"] = 2, ["ekc"] = 2,
                    ["dal"] = 2, ["dto"] = 2, ["dtosna"] = 2,
                    ["dro"] = 2, ["dRo"] = 2,
                    ["lat"] = 3, ["latsna"] = 3,
                });

            centOperatorMap = new System.Collections.ObjectModel.ReadOnlyDictionary<string, int>(
                new Dictionary<string, int>
                {
                    ["krz"] = 1, ["kRz"] = 1, ["ach"] = 2,
                    ["roft"] = 1, ["ycax"] = 1, ["pielyn"] = 0,
                    ["fal"] = 1, ["laf"] = 0,
                    ["fi"] = 1, ["ol"] = 0, ["if"] = 0,
                    ["cecio"] = 2, ["oicec"] = 0, 
                    //["krzq"] = 2, ["kRzq"] = 2, ["achq"] = 2,
                    //["roftq"] = 1,
                });

            compareMap = new System.Collections.ObjectModel.ReadOnlyDictionary<string, int>(
                new Dictionary<string, int>
                {
                    ["xtlo"] = 2, ["xylo"] = 2,
                    ["clo"] = 2, ["niv"] = 2,
                    ["llo"] = 2, ["xolo"] = 2,
                    ["xtlonys"] = 2, ["xylonys"] = 2,
                    ["llonys"] = 2, ["xolonys"] = 2,
                });
        }

        protected List<string> operations;
        protected List<List<string>> functions;
        
        public IReadOnlyList<string> InFileNames { get; }

        public CentTranscompiler(IList<string> inFileNames)
        {
            this.operations = new List<string>();
            this.functions = new List<List<string>>();
            
            this.InFileNames = new System.Collections.ObjectModel.ReadOnlyCollection<string>(inFileNames.ToList());
        }

        public CentTranscompiler(string[] inFiles) : this(inFiles.ToList()) { }

        public void Run(string outFileName)
        {
            if(!this.InFileNames.All(x => x.EndsWith(".cent")))
            {
                throw new ApplicationException("Included to be not cent files");
            }

            foreach (var inFile in this.InFileNames)
            {
                Read(inFile);
            }
            
            int falCount;
            int lafCount;
            int fiCount;
            int olCount;
            int ifCount;
            int cecioCount;
            int oicecCount;

            foreach (var func in this.functions)
            {
                falCount = 0;
                lafCount = 0;
                fiCount = 0;
                olCount = 0;
                ifCount = 0;
                cecioCount = 0;
                oicecCount = 0;
                
                var funcName = func[0];
                if (funcName.All(x => char.IsDigit(x)) || funcName.Any(x => x == '<' || x == '>')
                    || operatorMap.ContainsKey(funcName) || centOperatorMap.ContainsKey(funcName)
                    || compareMap.ContainsKey(funcName))
                {
                    throw new ApplicationException($"Invalid cesrva name: {func[0]}");
                }

                foreach (var item in func.Skip(1))
                {
                    switch(item)
                    {
                        case "fal":
                            falCount++;
                            break;
                        case "laf":
                            lafCount++;
                            break;
                        case "fi":
                            fiCount++;
                            break;
                        case "ol":
                            olCount++;
                            break;
                        case "if":
                            ifCount++;
                            break;
                        case "cecio":
                            cecioCount++;
                            break;
                        case "oicec":
                            oicecCount++;
                            break;
                    }
                }

                if(falCount != lafCount)
                {
                    throw new ApplicationException("count of 'laf' don't equals count of 'fal'.");
                }

                if (fiCount != ifCount)
                {
                    throw new ApplicationException("count of 'if' don't equals count of 'fi'.");
                }

                if (fiCount != olCount)
                {
                    throw new ApplicationException("count of 'ol' don't equals count of 'fi'.");
                }

                if(cecioCount != oicecCount)
                {
                    throw new ApplicationException("count of 'oicec' don't equals count of 'cecio'.");
                }
            }

            falCount = 0;
            lafCount = 0;
            fiCount = 0;
            olCount = 0;
            ifCount = 0;
            cecioCount = 0;
            oicecCount = 0;

            foreach (var item in this.operations)
            {
                switch (item)
                {
                    case "fal":
                        falCount++;
                        break;
                    case "laf":
                        lafCount++;
                        break;
                    case "fi":
                        fiCount++;
                        break;
                    case "ol":
                        olCount++;
                        break;
                    case "if":
                        ifCount++;
                        break;
                    case "cecio":
                        cecioCount++;
                        break;
                    case "oicec":
                        oicecCount++;
                        break;
                }
            }

            if (falCount != lafCount)
            {
                throw new ApplicationException("count of 'laf' don't equals to count of 'fal'.");
            }

            if (fiCount != ifCount)
            {
                throw new ApplicationException("count of 'if' don't equals to count of 'fi'.");
            }

            if (fiCount < olCount)
            {
                throw new ApplicationException("count of 'ol' don't less than or equals to count of 'fi'.");
            }

            if (cecioCount != oicecCount)
            {
                throw new ApplicationException("count of 'oicec' don't equals to count of 'cecio'.");
            }

            Console.WriteLine("main: {0}", this.operations.Aggregate(new StringBuilder("["),
                (x, y) => x.Append(y).Append(", "),
                x => x.Append("]").ToString()));

            Console.WriteLine("functions: {0}", this.functions.Aggregate(new StringBuilder("["),
                (x, y) => x.Append(
                    y.Aggregate(new StringBuilder("["),
                        (z, w) => z.Append(w).Append(", "),
                        z => z.Append("]").ToString())
                    ).Append(", "),
                x => x.Append("]").ToString()));

            Write(outFileName);
        }

        private void Read(string inFile)
        {
            bool isFunc = false;
            List<string> funcList = new List<string>();
            
            using(var reader = new StreamReader(inFile, new UTF8Encoding(false)))
            {
                StringBuilder buffer = new StringBuilder();

                void AppendLast()
                {
                    if (buffer.Length > 0)
                    {
                        if (isFunc)
                        {
                            funcList.Add(buffer.ToString());
                        }
                        else
                        {
                            this.operations.Add(buffer.ToString());
                        }
                    }

                    buffer.Clear();
                }

                while(!reader.EndOfStream)
                {
                    char c = Convert.ToChar(reader.Read());

                    if(char.IsWhiteSpace(c))
                    {
                        AppendLast();
                    }
                    else if(c == '\'')
                    {
                        c = Convert.ToChar(reader.Read());
                        if(c == '-')
                        {
                            bool hyphen;
                            c = '\0';

                            do
                            {
                                if (c == '-')
                                {
                                    hyphen = true;
                                }
                                else
                                {
                                    hyphen = false;
                                }

                                if(reader.EndOfStream)
                                {
                                    throw new ApplicationException("Not found \"-'\"");
                                }
                                c = Convert.ToChar(reader.Read());
                            }
                            while (!(hyphen && c == '\''));

                            AppendLast();
                        }
                        else
                        {
                            buffer.Append('\'').Append(c);
                        }
                    }
                    else if(c == '<')
                    {
                        if(isFunc)
                        {
                            throw new ApplicationException("Cannot defined cersva in cersva");
                        }
                        AppendLast();
                        isFunc = true;
                    }
                    else if(c == '>')
                    {
                        if (!isFunc)
                        {
                            throw new ApplicationException("Invalind word: '>'");
                        }
                        AppendLast();
                        this.functions.Add(funcList);
                        funcList = new List<string>();
                        isFunc = false;
                    }
                    else 
                    {
                        buffer.Append(c);
                    }
                }
            }

            if(isFunc)
            {
                throw new ApplicationException("Not found '>'");
            }
        }

        abstract protected void Write(string outFileName);
    }
}

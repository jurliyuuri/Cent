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
                    ["kinfit"] = 0,
                    ["krzq"] = 2, ["kRzq"] = 2, ["achq"] = 2,
                    ["roftq"] = 1,
                    ["malef"] = 0, ["felam"] = 0
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
        protected List<List<string>> subroutines;
        
        public IReadOnlyList<string> InFileNames { get; }

        public CentTranscompiler(IList<string> inFileNames)
        {
            this.operations = new List<string>();
            this.subroutines = new List<List<string>>();
            
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

            CheckCentProgramme();

            Console.WriteLine("main: {0}", this.operations.Aggregate(new StringBuilder("["),
                (x, y) => x.Append(y).Append(", "),
                x => x.Append("]").ToString()));

            Console.WriteLine("subroutines: {0}", this.subroutines.Aggregate(new StringBuilder("["),
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

            if (!File.Exists(inFile))
            {
                throw new ApplicationException($"Not found '{inFile}'");
            }
            
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
                            throw new ApplicationException("Cannot defined subroutine in subroutine");
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
                        this.subroutines.Add(funcList);
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

        private void CheckCentProgramme()
        {
            foreach (var subroutine in this.subroutines)
            {
                if(!subroutine.Any())
                {
                    throw new ApplicationException("Subroutine name expected");
                }

                string subroutineName = subroutine[0];

                if (subroutineName == "xok")
                {
                    if (!subroutine.Any(x => x != "xok"))
                    {
                        throw new ApplicationException("Function name expected");
                    }

                    subroutineName = subroutine[1];
                    if (subroutine.Count != 3)
                    {
                        throw new ApplicationException("Invalid arguments of 'xok'");
                    }

                    CheckSubroutineName(subroutineName);

                    if(subroutine[2].Any(x => !char.IsDigit(x)))
                    {
                        throw new ApplicationException($"Invalid argument's count: {subroutine[2]}");
                    }
                }
                else
                {
                    CheckSubroutineName(subroutineName);
                    CheckCode(subroutine.Skip(1));
                }
            }

            CheckCode(this.operations);
        }

        private void CheckSubroutineName(string subroutineName)
        {
            if (char.IsDigit(subroutineName[0]) || subroutineName.Any(x => x == '<' || x == '>')
                || operatorMap.ContainsKey(subroutineName) || centOperatorMap.ContainsKey(subroutineName)
                || compareMap.ContainsKey(subroutineName))
            {
                throw new ApplicationException($"Invalid subroutine name: {subroutineName}");
            }
        }

        private void CheckCode(IEnumerable<string> code)
        {
            int falCount = 0;
            int lafCount = 0;
            int fiCount = 0;
            int olCount = 0;
            int ifCount = 0;
            int cecioCount = 0;
            int oicecCount = 0;
            int malefCount = 0;
            int felamCount = 0;

            foreach (var item in code)
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
                    case "malef":
                        malefCount++;
                        break;
                    case "felam":
                        felamCount++;
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

            if (malefCount != felamCount)
            {
                throw new ApplicationException("count of 'felam' don't equals to count of 'malef'.");
            }
        }

        abstract protected void Write(string outFileName);
    }
}

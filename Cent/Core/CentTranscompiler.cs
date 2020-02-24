﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Cent.Core
{
    public abstract class CentTranscompiler
    {
        protected static readonly IReadOnlyDictionary<string, int> operatorMap;
        protected static readonly IReadOnlyDictionary<string, int> centOperatorMap;
        protected static readonly IReadOnlyDictionary<string, int> compareMap;

        static CentTranscompiler()
        {
            operatorMap = new System.Collections.ObjectModel.ReadOnlyDictionary<string, int>(
                new Dictionary<string, int>
                {
                    ["nac"] = 1, ["sna"] = 1,
                    ["ata"] = 2, ["nta"] = 2,
                    ["kak"] = 2, ["ada"] = 2, ["ekc"] = 2,
                    ["dal"] = 2, ["dto"] = 2, ["dtosna"] = 2,
                    ["dro"] = 2, ["dRo"] = 2,
                    ["lat"] = 2, ["latsna"] = 2,
                });

            centOperatorMap = new System.Collections.ObjectModel.ReadOnlyDictionary<string, int>(
                new Dictionary<string, int>
                {
                    ["tikl"] = 1,
                    ["krz"] = 1, ["kRz"] = 1, ["ach"] = 2,
                    ["roft"] = 1, ["ycax"] = 1, ["pielyn"] = 0,
                    ["fal"] = 1, ["laf"] = 0,
                    ["fi"] = 1, ["ol"] = 0, ["if"] = 0,
                    ["cecio"] = 2, ["oicec"] = 0,
                    ["kinfit"] = 0,
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

        public IReadOnlyList<string> InFileNames { get; }
        public bool IsOptimized { get; set; }
        public Stack<string> CallSubroutines { get; }
        public Dictionary<string, uint> FunctionNames { get; }

        private readonly List<string> _tokens;
        private readonly List<List<string>> _subroutines;
        private readonly List<string> _subroutineNames;
        private bool _isTopMain;

        protected CentTranscompiler(IEnumerable<string> inFileNames, bool isTopMain = true)
        {
            InFileNames = new System.Collections.ObjectModel.ReadOnlyCollection<string>(inFileNames.ToList());
            FunctionNames = new Dictionary<string, uint>();
            CallSubroutines = new Stack<string>();

            _tokens = new List<string>();
            _subroutines = new List<List<string>>();
            _subroutineNames = new List<string>();
            _isTopMain = isTopMain;
        }

        public void Output(string outFileName)
        {
            if(!InFileNames.All(x => x.EndsWith(".cent")))
            {
                throw new ApplicationException("Included to be not cent files");
            }

            foreach (var inFile in InFileNames)
            {
                Lexer(inFile);
            }

            CheckCentProgramme();
            ListingTokens();
            if(IsOptimized)
            {
                List<string> tokens = Optimize(_tokens);
                _tokens.Clear();
                _tokens.AddRange(tokens);

                List<List<string>> subroutines = new List<List<string>>();
                foreach (var subroutine in _subroutines)
                {
                    if (subroutine.First() != "xok")
                    {
                        subroutines.Add(Optimize(subroutine));
                    }
                }
                _subroutines.Clear();
                _subroutines.AddRange(subroutines);

                ListingTokens();
            }
            Write(outFileName);
        }

        /// <summary>
        /// 字句解析部分
        /// </summary>
        /// <param name="inFile">読み込むファイル名</param>
        private void Lexer(string inFile)
        {
            bool isFunc = false;
            List<string> funcTokens = new List<string>();

            if (!File.Exists(inFile))
            {
                throw new ApplicationException($"Not found '{inFile}'");
            }
            
            using(var reader = new StreamReader(inFile, new UTF8Encoding(false)))
            {
                StringBuilder buffer = new StringBuilder();

                // 追加先を判断して最後に追加するローカル関数
                void AppendLast()
                {
                    if (buffer.Length > 0)
                    {
                        if (isFunc)
                        {
                            funcTokens.Add(buffer.ToString());
                        }
                        else
                        {
                            _tokens.Add(buffer.ToString());
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
                        _subroutines.Add(funcTokens);
                        funcTokens = new List<string>();
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

        /// <summary>
        /// コードが正しいかチェック
        /// </summary>
        private void CheckCentProgramme()
        {
            foreach (var subroutine in _subroutines)
            {
                // サブルーチン定義が空
                if(!subroutine.Any())
                {
                    throw new ApplicationException("Subroutine name expected");
                }

                string subroutineName = subroutine[0];

                // サブルーチン定義が外部関数のインポート定義の場合
                if (subroutineName == "xok")
                {
                    // xok以外が無い
                    if (!subroutine.Any(x => x != "xok"))
                    {
                        throw new ApplicationException("Function name expected");
                    }

                    subroutineName = subroutine[1];

                    // インポート定義の引数がxokを含めて3つ以外
                    if (subroutine.Count != 3)
                    {
                        throw new ApplicationException("Invalid arguments of 'xok'");
                    }

                    // インポート関数名のチェック
                    CheckSubroutineName(subroutineName);

                    // インポート定義の第2引数が10進数値ではない
                    if(subroutine[2].Any(x => !char.IsDigit(x)))
                    {
                        throw new ApplicationException($"Invalid argument's count: {subroutine[2]}");
                    }
                }
                else
                {
                    // サブルーチン関数名のチェック
                    CheckSubroutineName(subroutineName);
                    // サブルーチンのコードチェック
                    CheckCode(subroutine.Skip(1));
                }
            }

            // コードチェック
            CheckCode(_tokens);
        }

        // サブルーチンの関数名が使用可能なものかどうかのチェック
        private void CheckSubroutineName(string subroutineName)
        {
            if (char.IsDigit(subroutineName[0]) || subroutineName.Any(x => x == '<' || x == '>')
                || operatorMap.ContainsKey(subroutineName) || centOperatorMap.ContainsKey(subroutineName)
                || compareMap.ContainsKey(subroutineName))
            {
                throw new ApplicationException($"Invalid subroutine name: {subroutineName}");
            }
        }

        /// <summary>
        /// 制御文の対応チェック
        /// </summary>
        /// <param name="code"></param>
        private void CheckCode(IEnumerable<string> code)
        {
            Stack<string> stack = new Stack<string>();
            Dictionary<string, string> needWord = new Dictionary<string, string>
            {
                ["fal"] = "laf",
                ["fi"] = "if",
                ["ol"] = "if",
                ["cecio"] = "oicec",
            };

            foreach (var item in code)
            {
                string top;
                switch (item)
                {
                    case "fal":
                    case "fi":
                    case "cecio":
                        stack.Push(item);
                        break;
                    case "laf":
                    case "if":
                    case "oicec":
                        top = stack.Pop();
                        if (needWord[top] != item)
                        {
                            throw new ApplicationException($"invalid keyword: need: {needWord[top]}, value: {item}");
                        }
                        break;
                    case "ol":
                        top = stack.Pop();
                        if (top != "fi")
                        {
                            throw new ApplicationException($"invalid keyword: need: {needWord[top]}, value: {item}");
                        }
                        else
                        {
                            stack.Push("ol");
                        }
                        break;
                }
            }

            if (stack.Any())
            {
                throw new ApplicationException("end of control statement");
            }
        }

        private List<string> Optimize(List<string> tokenList)
        {
            List<string> tokens = new List<string>();
            string prev = string.Empty;

            foreach (var token in tokenList)
            {
                if ((token == "if" && prev == "fi") ||
                    (token == "ach" && prev == "ach") ||
                    (token == "nac" && prev == "nac") ||
                    (token == "sna" && prev == "sna"))
                {
                    tokens.RemoveAt(tokens.Count - 1);
                    prev = tokens.LastOrDefault() ?? string.Empty;
                }
                else if (token == "laf" && prev == "fal")
                {
                    tokens.RemoveAt(tokens.Count - 1);
                    prev = tokens.LastOrDefault() ?? string.Empty;
                }
                else if (token == "roft")
                {
                    switch(prev)
                    {
                        case "roft":
                            tokens[tokens.Count - 1] = "<>roft-nia";
                            prev = "<>roft-nia";
                            break;
                        case "<>roft-nia":
                            tokens.RemoveAt(tokens.Count - 1);
                            prev = tokens.LastOrDefault() ?? string.Empty;
                            break;
                        default:
                            tokens.Add(token);
                            prev = token;
                            break;
                    }
                }
                else if (token == "kinfit" && (prev == string.Empty || prev == "pielyn"))
                {
                    tokens.Add("0");
                    prev = "0";
                }
                else if (token == "sna" && prev == "nac")
                {
                    if (tokens[tokens.Count - 2] == "<>nta1")
                    {
                        tokens.RemoveRange(tokens.Count - 2, 2);
                        prev = tokens.LastOrDefault() ?? string.Empty;
                    }
                    else
                    {
                        tokens[tokens.Count - 1] = "<>ata1";
                        prev = "<>ata1";
                    }
                }
                else if (token == "nac" && prev == "sna")
                {
                    if (tokens[tokens.Count - 2] == "<>ata1")
                    {
                        tokens.RemoveRange(tokens.Count - 2, 2);
                        prev = tokens.LastOrDefault() ?? string.Empty;
                    }
                    else
                    {
                        tokens[tokens.Count - 1] = "<>nta1";
                        prev = "<>nta1";
                    }
                }
                else if (token == "ycax")
                {
                    if (prev == "lat")
                    {
                        tokens[tokens.Count - 1] = "<>lat32";
                        prev = "<>lat32";
                    }
                    else if (prev == "latsna")
                    {
                        tokens[tokens.Count - 1] = "<>latsna32";
                        prev = "<>latsna32";
                    }
                    else if (prev.All(char.IsDigit))
                    {
                        tokens.RemoveAt(tokens.Count - 1);
                        prev = tokens.LastOrDefault() ?? string.Empty;
                    }
                }
                else if (prev.All(char.IsDigit))
                {
                    switch (token)
                    {
                        case "ata":
                        case "nta":
                            if (prev == "0")
                            {
                                tokens.RemoveAt(tokens.Count - 1);
                                prev = tokens.LastOrDefault() ?? string.Empty;
                            }
                            else
                            {
                                tokens.Add(token);
                                prev = token;
                            }
                            break;
                        case "lat":
                        case "latsna":
                            if (prev == "1")
                            {
                                tokens[tokens.Count - 1] = "0";
                                prev = "0";
                            }
                            else if (prev == "0")
                            {
                                tokens[tokens.Count - 1] = "ycax";
                                tokens.Add("0");
                                tokens.Add("0");
                                prev = "0";
                            }
                            else
                            {
                                tokens.Add(token);
                                prev = token;
                            }
                            break;
                        case "krz":
                            tokens.Add(prev);
                            break;
                        case "pielyn":
                            {
                                int index = tokens.FindLastIndex(x => !x.All(char.IsDigit));
                                int count = tokens.Count - index - 1;

                                if (count > 0)
                                {
                                    tokens.RemoveRange(index + 1, count);
                                }
                            }
                            tokens.Add(token);
                            prev = token;
                            break;
                        default:
                            tokens.Add(token);
                            prev = token;
                            break;
                    }
                }
                else
                {
                    tokens.Add(token);
                    prev = token;
                }
            }

            return tokens;
        }
        
        /// <summary>
        /// 変換先出力
        /// </summary>
        /// <param name="outFileName"></param>
        protected void Write(string outFileName)
        {
            foreach (var subrt in _subroutines)
            {
                if (subrt[0] == "xok")
                {
                    FunctionNames.Add(subrt[1], uint.Parse(subrt[2]));
                }
                else
                {
                    _subroutineNames.Add(subrt[0]);
                }
            }
            
            PreProcess(outFileName);

            if(_isTopMain)
            {
                DefineMainroutine();

                foreach (var subroutine in _subroutines.Where(x => x[0] != "xok"))
                {
                    DefineSubroutine(subroutine);
                }
            }
            else
            {
                foreach (var subroutine in _subroutines.Where(x => x[0] != "xok"))
                {
                    DefineSubroutine(subroutine);
                }

                DefineMainroutine();
            }

            PostProcess(outFileName);
        }

        private void WriteTokens(string token)
        {
            // 全て10進数値であれば数値
            if (uint.TryParse(token, out uint result))
            {
                Value(result);
            }
            // サブルーチンに登録されている名称であれば
            else if (_subroutineNames.Contains(token))
            {
                FenxeSubroutine(token);
            }
            // 外部関数に登録されている名称であれば
            else if (FunctionNames.ContainsKey(token))
            {
                Fenxe(token, FunctionNames[token]);
            }
            else
            {
                switch (token)
                {
                    case "nac":
                        Nac();
                        break;
                    case "sna":
                        Sna();
                        break;
                    case "ata":
                        Ata();
                        break;
                    case "nta":
                        Nta();
                        break;
                    case "ada":
                        Ada();
                        break;
                    case "ekc":
                        Ekc();
                        break;
                    case "dto":
                        Dto();
                        break;
                    case "dro":
                    case "dRo":
                        Dro();
                        break;
                    case "dtosna":
                        Dtosna();
                        break;
                    case "dal":
                        Dal();
                        break;
                    case "lat":
                        Lat();
                        break;
                    case "latsna":
                        Latsna();
                        break;
                    case "xtlo":
                        Xtlo();
                        break;
                    case "xylo":
                        Xylo();
                        break;
                    case "clo":
                        Clo();
                        break;
                    case "niv":
                        Niv();
                        break;
                    case "llo":
                        Llo();
                        break;
                    case "xolo":
                        Xolo();
                        break;
                    case "xtlonys":
                        Xtlonys();
                        break;
                    case "xylonys":
                        Xylonys();
                        break;
                    case "llonys":
                        Llonys();
                        break;
                    case "xolonys":
                        Xolonys();
                        break;
                    case "krz":
                    case "kRz":
                        Krz();
                        break;
                    case "ach":
                        Ach();
                        break;
                    case "roft":
                        Roft();
                        break;
                    case "ycax":
                        Ycax();
                        break;
                    case "pielyn":
                        Pielyn();
                        break;
                    case "fal":
                        Fal();
                        break;
                    case "laf":
                        Laf();
                        break;
                    case "fi":
                        Fi();
                        break;
                    case "ol":
                        Ol();
                        break;
                    case "if":
                        If();
                        break;
                    case "cecio":
                        Cecio();
                        break;
                    case "oicec":
                        Oicec();
                        break;
                    case "kinfit":
                        Kinfit();
                        break;
                    case "tikl":
                        Tikl();
                        break;
                    case "<>ata1":
                        Ata1();
                        break;
                    case "<>nta1":
                        Nta1();
                        break;
                    case "<>roft-nia":
                        RoftNia();
                        break;
                    case "<>lat32":
                        Lat32();
                        break;
                    case "<>latsna32":
                        Latsna32();
                        break;
                    case "lykl":
                    default:
                        throw new ApplicationException($"Unknown word: '{token}'");
                }
            }
        }

        private void DefineMainroutine()
        {
            MainroutinePreProcess();
            foreach (var token in _tokens)
            {
                WriteTokens(token);
            }
            MainroutinePostProcess();
        }

        private void DefineSubroutine(List<string> subroutine)
        {
            string name = subroutine.First();
            SubroutinePreProcess(name);
            foreach (var token in subroutine.Skip(1))
            {
                WriteTokens(token);
            }
            SubroutinePostProcess();
        }
        
        protected abstract void PreProcess(string outFileName);
        protected abstract void PostProcess(string outFileName);

        protected abstract void MainroutinePreProcess();
        protected abstract void MainroutinePostProcess();

        protected abstract void SubroutinePreProcess(string name);
        protected abstract void SubroutinePostProcess();

        protected abstract void Value(uint result);
        protected abstract void FenxeSubroutine(string subroutineName);
        protected abstract void Fenxe(string funcName, uint argc);
        protected abstract void Nac();
        protected abstract void Sna();
        protected abstract void Ata();
        protected abstract void Nta();
        protected abstract void Ada();
        protected abstract void Ekc();
        protected abstract void Dto();
        protected abstract void Dro();
        protected abstract void Dtosna();
        protected abstract void Dal();
        protected abstract void Lat();
        protected abstract void Latsna();
        protected abstract void Xtlo();
        protected abstract void Xylo();
        protected abstract void Clo();
        protected abstract void Niv();
        protected abstract void Llo();
        protected abstract void Xolo();
        protected abstract void Xtlonys();
        protected abstract void Xylonys();
        protected abstract void Llonys();
        protected abstract void Xolonys();
        protected abstract void Krz();
        protected abstract void Ach();
        protected abstract void Roft();
        protected abstract void Ycax();
        protected abstract void Pielyn();
        protected abstract void Fal();
        protected abstract void Laf();
        protected abstract void Fi();
        protected abstract void Ol();
        protected abstract void If();
        protected abstract void Cecio();
        protected abstract void Oicec();
        protected abstract void Kinfit();
        protected abstract void Tikl();

        protected virtual void Ata1()
        {
            Nac();
            Sna();
        }

        protected virtual void Nta1()
        {
            Sna();
            Nac();
        }

        protected virtual void RoftNia()
        {
            Roft();
            Roft();
        }

        protected virtual void Lat32()
        {
            Lat();
            Ycax();
        }

        protected virtual void Latsna32()
        {
            Latsna();
            Ycax();
        }
        
        /// <summary>
        /// 字句解析・コードチェック後のコード出力(デバッグ用)
        /// </summary>
        private void ListingTokens()
        {
            Console.WriteLine("main: {0}", _tokens.Aggregate(new StringBuilder("["),
                (x, y) => x.Append(y).Append(", "),
                x => x.Append("]").ToString()));

            Console.WriteLine("subroutines: {0}", _subroutines.Aggregate(new StringBuilder("["),
                (x, y) => x.Append(
                    y.Aggregate(new StringBuilder("["),
                        (z, w) => z.Append(w).Append(", "),
                        z => z.Append("]").ToString())
                    ).Append(", "),
                x => x.Append("]").ToString()));
        }

    }
}

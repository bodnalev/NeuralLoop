using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Linq;

namespace NeuralLoop
{
    
    class Translator
    {
        public static List<BinaryWord> collection;
        public static Dictionary<string, BinaryWord> nouns;
        
        private void LoadDictionary()
        {
            collection = new List<BinaryWord>();
            nouns = new Dictionary<string, BinaryWord>();
            using (StreamReader reader = new StreamReader(@"dictionary.txt"))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    BinaryWord bw = new BinaryWord(line);
                    collection.Add(bw);
                    if (bw.type == BinaryWord.WordClass.Noun)
                    {
                        nouns.Add(bw.word, bw);
                    }
                }
            }
        }

        public void ReadFile()
        {
            LoadDictionary();

            using (StreamReader wordsReader = new StreamReader(@"words.txt"))
            {
                using (StreamWriter writer = new StreamWriter(@"dictionary.txt"))
                {
                    string wordsLine;
                    while ((wordsLine = wordsReader.ReadLine()) != null)
                    {
                        string[] ar = wordsLine.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        BinaryWord bw = GenerateBinaryWord(ar[0]);
                        if (bw != null)
                        {
                            collection.Add(bw);
                            if (bw.type == BinaryWord.WordClass.Noun)
                            {
                                nouns.Add(bw.word, bw);
                            }
                            writer.WriteLine(bw.ToString());
                        }
                    }
                }
            }
        }

        public BinaryWord GenerateBinaryWord(string word)
        {
            try
            {
                HttpWebRequest dicReq = (HttpWebRequest)WebRequest.Create("http://www.dictionary.com/misspelling?term=" + word);
                dicReq.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                using (HttpWebResponse dicRes = (HttpWebResponse)dicReq.GetResponse())
                {
                    using (StreamReader dicReader = new StreamReader(dicRes.GetResponseStream()))
                    {
                        return BinaryWordFromReader(dicReader, word);
                    }
                }
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    WebResponse res = ex.Response;
                    using (StreamReader reader = new StreamReader(res.GetResponseStream()))
                    {
                        string dicLine;
                        while ((dicLine = reader.ReadLine()) != null)
                        {
                            if (dicLine.Contains("There are no results for:"))
                            {
                                return null;
                            }
                            if (dicLine.Contains("Did you mean"))
                            {
                                int start = dicLine.IndexOf("com/browse/") + 11;
                                int end = dicLine.IndexOf('"', start - 2);
                                string newWord = dicLine.Substring(start, end - start);
                                if (!newWord.Contains("%"))
                                {
                                    return GenerateBinaryWord(newWord);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception occured in GenerateBinaryWord: " + e.Message);
            }

            return null;
        }

        public BinaryWord BinaryWordFromReader(StreamReader dicReader, string word)
        {
            string dicLine;
            bool britDic = false;
            bool firstClass = true, firstDef = true;
            string className = null, defWord = null;

            while ((dicLine = dicReader.ReadLine()) != null)
            {
                if (firstClass && dicLine.Contains("dbox-pg"))
                {
                    try
                    {
                        int start = dicLine.IndexOf("dbox-pg") + 9;
                        int end = dicLine.IndexOf('<', start);
                        className = dicLine.Substring(start, end - start);

                        firstClass = false;
                    }
                    catch (Exception e)
                    { }

                }

                if ((firstDef || defWord == null) && dicLine.Contains("def-content"))
                {
                    dicLine = dicReader.ReadLine();
                    if (dicLine.Contains("def-sub-list"))
                    {
                        dicLine = dicReader.ReadLine();
                    }

                    firstDef = false;

                    string[] defSplit = dicLine.Split(new char[] { '>', '(', ')', ',', '.', ':', ';', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < defSplit.Length; i++)
                    {
                        if (nouns.ContainsKey(defSplit[i]))
                        {
                            defWord = defSplit[i];
                            break;
                        }
                    }
                }

                if (!britDic && dicLine.Contains("British Dictionary definitions for"))
                {
                    britDic = true;
                    firstClass = true;
                    firstDef = true;
                }
            }

            Console.WriteLine("Word: " + word + "\t Class: " + className + "\t Definition: " + defWord);

            BinaryWord bw = new BinaryWord(word, className, defWord);
            return bw;
        }

        public string SpellChecker(string word)
        {
            try
            {
                HttpWebRequest dicReq = (HttpWebRequest)WebRequest.Create("http://www.dictionary.com/misspelling?term=" + word);
                dicReq.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                using (HttpWebResponse dicRes = (HttpWebResponse)dicReq.GetResponse())
                {
                    using (StreamReader dicReader = new StreamReader(dicRes.GetResponseStream()))
                    {
                        return word;
                    }
                }
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    WebResponse res = ex.Response;
                    using (StreamReader reader = new StreamReader(res.GetResponseStream()))
                    {
                        string dicLine;
                        while ((dicLine = reader.ReadLine()) != null)
                        {
                            if (dicLine.Contains("There are no results for:"))
                            {
                                return null;
                            }
                            if (dicLine.Contains("Did you mean"))
                            {
                                int start = dicLine.IndexOf("com/browse/") + 11;
                                int end = dicLine.IndexOf('"', start - 2);
                                string newWord = dicLine.Substring(start, end - start);
                                if (!newWord.Contains("%"))
                                {
                                    return newWord;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception occured: "+e.Message);
            }

            return null;
        }
        
        public List<string> Synonyms(string word)
        {
            List<string> res = new List<string>();

            HttpWebRequest dicReq = (HttpWebRequest)WebRequest.Create("http://www.thesaurus.com/browse/" + word);
            dicReq.AllowAutoRedirect = true;
            dicReq.MaximumAutomaticRedirections = 3;
            dicReq.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (HttpWebResponse dicRes = (HttpWebResponse)dicReq.GetResponse())
            {
                using (StreamReader dicReader = new StreamReader(dicRes.GetResponseStream()))
                {
                    string dicLine;
                    while ((dicLine = dicReader.ReadLine()) != null)
                    {
                        if (dicLine.Contains("no thesaurus results"))
                        {
                            dicReader.Close();
                            return null;
                        }
                        if (dicLine.Contains("relevant-3") || dicLine.Contains("relevant-2"))
                        {
                            int start = dicLine.IndexOf("com/browse/") + 11;
                            int end = dicLine.IndexOf('"', start - 2);
                            string similar = dicLine.Substring(start, end - start);
                            if (!similar.Contains("%"))
                            {
                                res.Add(similar);
                            }
                        }
                    }
                }
            }
            return res;
        }

        public class BinaryWord
        {
            public enum WordClass { Noun, Verb, AdjeAdve, DeterConjPrepPronInterjPuncUnkn }
            public WordClass type;
            
            //Variables
            public string word;
            public int dataNumber = 0;

            public bool[] header; //2 size
            public bool[] data; //18 size
            public bool[] complete; //20 size
            

            //Constructors
            public BinaryWord(string word, string wordClass, string defWord)
            {
                this.word = word;
                switch (wordClass)
                {
                    case "noun":
                        type = WordClass.Noun;
                        header = new bool[] { true, false };
                        break;
                    case "verb":
                        type = WordClass.Verb;
                        header = new bool[] { false, true };
                        break;
                    case "adjective":
                    case "adverb":
                        type = WordClass.AdjeAdve;
                        header = new bool[] { false, false };
                        break;
                    default:
                        type = WordClass.DeterConjPrepPronInterjPuncUnkn;
                        header = new bool[] { true, true };
                        break;
                }

                SetData(defWord, type);

                dataNumber = GetNumberFromData(data);

                complete = Concate(header, data);
            }

            public BinaryWord(string source)
            {
                try
                {
                    string[] spl = source.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    word = spl[0];
                    dataNumber = int.Parse(spl[1]);
                    data = GetDataFromNumber(dataNumber);

                    switch (spl[3])
                    {
                        case "10":
                            type = WordClass.Noun;
                            header = new bool[] { true, false };
                            break;
                        case "01":
                            type = WordClass.Verb;
                            header = new bool[] { false, true };
                            break;
                        case "00":
                            type = WordClass.AdjeAdve;
                            header = new bool[] { false, false };
                            break;
                        default:
                            type = WordClass.DeterConjPrepPronInterjPuncUnkn;
                            header = new bool[] { true, true };
                            break;
                    }
                    complete = Concate(header, data);
                }
                catch (Exception e){}
            }


            //Functions
            private void SetData(string def, WordClass type)
            {
                BinaryWord defBinary;
                if (def == null || nouns.TryGetValue(def, out defBinary))
                {
                    bool[] randomData;
                    if (!FindRandomlyDifferent(10, 18, 14, type, out randomData))
                    {
                        if (!FindRandomlyDifferent(10, 18, 16, type, out randomData))
                        {
                            if (!FindNextDifferent(18, 16, type, out randomData))
                            {
                                throw new OutOfMemoryException();
                            }
                        }
                    }
                    data = randomData;
                }
                else
                {
                    
                }
            }

            private bool[] Concate(bool[] first, bool[] second)
            {
                bool[] ret = new bool[first.Length + second.Length];
                first.CopyTo(ret, 0);
                second.CopyTo(ret, first.Length);
                return ret;
            }

            private bool FindNextDifferent(int length, int differentPart, WordClass type, out bool[] data)
            {
                data = new bool[length];
                for (int i = 0; i < (int)Math.Pow(2, length); i++)
                {
                    data = GetDataFromNumber(i);
                    if (SameAll(data, differentPart, type))
                    {
                        return true;
                    }
                }
                return false;
            }

            private bool FindRandomlyDifferent(int attempts, int length, int differentPart, WordClass type, out bool[] data)
            {
                int attempt = 0;
                do
                {
                    attempt++;
                    data = GetRandom(length);
                }
                while (attempt < attempts && !SameAll(data, differentPart, type));
                if (attempt == attempts)
                {
                    return false;
                }
                return true;
            }

            private bool[] GetRandom(int length)
            {
                Random r = new Random();
                bool[] ret = new bool[length];
                for (int i = 0; i < ret.Length; i++)
                {
                    ret[i] = r.NextDouble() >= 0.5f;
                }
                return ret;
            }

            private bool SameUntil(bool[] data1, bool[] data2, int digits)
            {
                for (int i = 0; i < data1.Length && i <= digits && i<data2.Length; i++)
                {
                    if (data1[i] != data2[i])
                    {
                        return false;
                    }
                }
                return true;
            }

            private bool SameAll(bool[] data, int digits, WordClass type)
            {
                foreach (BinaryWord bw in collection)
                {
                    if (SameUntil(data, bw.data, digits) && type == bw.type)
                    {
                        return false;
                    }
                }
                return true;
            }

            private bool[] GetDataFromNumber(int number)
            {
                return Convert.ToString(number, 2).PadLeft(18, '0').Select(s => s.Equals('1')).ToArray();
            }

            private int GetNumberFromData(bool[] data)
            {
                int mul = 1;
                int res = 0;
                for (int i = 0; i < data.Length; i++)
                {
                    if (data[i])
                    {
                        res += mul;
                    }
                    mul *= 2;
                }
                return res;
            }
            
            public override string ToString()
            {
                return word + " " + dataNumber + " " + (header[0] ? '1' : '0') + (header[1] ? '1' : '0');
            }
        }
    }
}

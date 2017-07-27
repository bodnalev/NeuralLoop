using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace NeuralLoop
{
    
    class Translator
    {
        public List<BinaryWord> collection;

        public void ReadPage()
        {
            HttpWebRequest wordsReq =(HttpWebRequest)WebRequest.Create("http://norvig.com/ngrams/count_1w.txt");
            
            using (WebResponse wordsRes = wordsReq.GetResponse())
            {
                using (StreamReader wordsReader = new StreamReader(wordsRes.GetResponseStream()))
                {
                    string wordsLine;
                    while ((wordsLine = wordsReader.ReadLine()) != null)
                    {
                        string[] ar = wordsLine.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        string word = SpellChecker(ar[0]);
                        if (word == null)
                        {
                            continue;
                        }
                        Console.WriteLine(word);
                    }
                }
            }
        }

        public void ReadFile()
        {
            collection = new List<BinaryWord>();

            using (StreamReader wordsReader = new StreamReader(@"words.txt"))
            {
                string wordsLine;
                while ((wordsLine = wordsReader.ReadLine()) != null)
                {
                    string[] ar = wordsLine.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    string word = SpellChecker(ar[0]);
                    if (word == null)
                    {
                        continue;
                    }
                    Console.WriteLine(word);
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
                        BinaryWord bw = new BinaryWord();
                        bw.word = word;

                        List<string> classes = new List<string>();

                        string dicLine;
                        bool normal = false, britDic = false;

                        while ((dicLine = dicReader.ReadLine()) != null)
                        {
                            //Finds out the word classes
                            if (dicLine.Contains("luna-data-header"))
                            {
                                dicLine = dicReader.ReadLine();
                                try
                                {
                                    int start = dicLine.IndexOf("dbox-pg") + 9;
                                    int end = dicLine.IndexOf('<', start);
                                    string newWord = dicLine.Substring(start, end - start);
                                    classes.Add(newWord.Split()[0]);
                                }
                                catch (Exception e)
                                { }
                            }

                            if (!normal && dicLine.Contains("def-content"))
                            {
                                normal = true;
                                Console.WriteLine(dicLine);
                            }

                            if (!britDic && dicLine.Contains("British Dictionary definitions for"))
                            {
                                normal = false;
                                britDic = true;
                            }

                        }

                        bw.SetClass(classes);


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
                                    return null;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception occured: " + e.Message);
            }

            return null;
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
            
            public string word;

            private bool[] header; //2 size
            private bool[] data; //18 size
            private bool[] complete; //20 size

            public void SetClass(List<string> classes)
            {
                if (classes.Contains("determiner") || classes.Contains("conjunction") ||
                    classes.Contains("preposition") || classes.Contains("pronoun"))
                {
                    type = WordClass.DeterConjPrepPronInterjPuncUnkn;
                    header = new bool[] { true, true };
                    Console.WriteLine("1st DeterConjPrepPron  InterjPuncUnkn");
                    return;
                }
                for (int i = 0; i < classes.Count; i++)
                {
                    if (classes[i] == "noun")
                    {
                        type = WordClass.Noun;
                        header = new bool[] {true, false};
                        Console.WriteLine(classes[i]);
                        return;
                    }
                    if (classes[i] == "verb")
                    {
                        type = WordClass.Verb;
                        header = new bool[] {false, true };
                        Console.WriteLine(classes[i]);
                        return;
                    }
                    if (classes[i] == "adjective" || classes[i] == "adverb")
                    {
                        type = WordClass.AdjeAdve;
                        header = new bool[] { false, false };
                        Console.WriteLine(classes[i]);
                        return;
                    }
                    if (classes[i] == "interjection")
                    {
                        type = WordClass.DeterConjPrepPronInterjPuncUnkn;
                        header = new bool[] { true, true };
                        Console.WriteLine(classes[i]);
                        return;
                    }
                }
                type = WordClass.DeterConjPrepPronInterjPuncUnkn;
                header = new bool[] { true, true };
                Console.WriteLine("last Unkn from DeterConjPrepPronInterjPuncUnkn");
            }
        }
    }
}

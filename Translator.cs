using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace NeuralLoop
{
    /// <summary>
    /// Creates the link between the english words and words available for the program
    /// </summary>
    class Translator
    {
        /// <summary>
        /// Stores the words in this list
        /// </summary>
        public static Dictionary<string, BinaryWord> collection;
        /// <summary>
        /// Stores the nouns (they are special as they are used in the definitions)
        /// </summary>
        public static Dictionary<string, BinaryWord> nouns;

        /// <summary>
        /// Loads the words from the dictionary.txt
        /// </summary>
        public string LoadDictionary()
        {
            collection = new Dictionary<string, BinaryWord>();
            nouns = new Dictionary<string, BinaryWord>();
            int num = 0;
            string last = "";
            using (StreamReader reader = new StreamReader(@"D:\Edu\Programming\ML\NeuralLoop\NeuralLoop\dictionary.txt"))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    BinaryWord bw = new BinaryWord(line);
                    collection.Add(bw.word, bw);
                    if (bw.type == BinaryWord.WordClass.Noun)
                    {
                        nouns.Add(bw.word, bw);
                    }
                    last = bw.word;
                    num++;
                }
            }
            Console.WriteLine("Loaded " + num + " words from dictionary.txt \n" +
                "Last word is: " + last);
            return last;
        }

        /// <summary>
        /// Reads the words.txt to extend the dictionary.txt with the generated BinaryWords
        /// </summary>
        public void ReadWordList()
        {
            string lastWord = LoadDictionary();

            using (StreamReader wordsReader = new StreamReader(@".\words.txt"))
            {
                using (StreamWriter writer = new StreamWriter(@"D:\Edu\Programming\ML\NeuralLoop\NeuralLoop\dictionary.txt", true))
                {
                    string wordsLine;
                    bool alreadyIn = true;
                    while ((wordsLine = wordsReader.ReadLine()) != null)
                    {
                        string[] ar = wordsLine.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        string currWord = ar[0].ToLower();

                        Console.WriteLine("CW: " + currWord);

                        if (currWord.Equals(lastWord) || lastWord.Equals(""))
                        {
                            alreadyIn = false;
                        }

                        if (alreadyIn)
                        {
                            continue;
                        }
                        BinaryWord bw = GenerateBinaryWord(currWord, false);
                        if (bw != null && !collection.ContainsKey(bw.word))
                        {
                            collection.Add(bw.word, bw);
                            if (bw.type == BinaryWord.WordClass.Noun)
                            {
                                nouns.Add(bw.word, bw);
                            }
                            writer.WriteLine(bw.ToString());
                            writer.Flush();
                        }
                    }
                }
            }
        }

        public void AddWord(string word)
        {
            using (StreamWriter writer = new StreamWriter(@"D:\Edu\Programming\ML\NeuralLoop\NeuralLoop\dictionary.txt", true))
            {
                word = word.ToLower();
                BinaryWord bw = new BinaryWord(word, BinaryWord.WordClass.DeterConjPrepPronInterjPuncUnkn, null);
                collection.Add(word,bw);
                writer.WriteLine(bw.ToString());
                writer.Flush();
            }
        }

        /// <summary>
        /// Generates a BinaryWord from a given word
        /// </summary>
        /// <param name="word">The BinaryWord will be the representative of this word</param>
        /// <returns>The returned BinaryWord </returns>
        public BinaryWord GenerateBinaryWord(string word, bool sec)
        {
            try
            {
                HttpWebRequest dicReq = (HttpWebRequest)WebRequest.Create("http://www.dictionary.com/misspelling?term=" + word);
                dicReq.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                
                using (HttpWebResponse dicRes = (HttpWebResponse)dicReq.GetResponse())
                {
                    using (StreamReader dicReader = new StreamReader(dicRes.GetResponseStream()))
                    {
                        if (collection.ContainsKey(word))
                        {
                            return null;
                        }
                        return BinaryWordFromReader(dicReader, word);
                    }
                }
            }
            catch (WebException ex)
            {
                if (sec)
                {
                    return null;
                }
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
                                string newWord = dicLine.Substring(start, end - start).ToLower();
                                if (!newWord.Contains("%"))
                                {
                                    if (collection.ContainsKey(newWord))
                                    {
                                        return null;
                                    }
                                    return GenerateBinaryWord(newWord, true);
                                }
                            }
                        }
                    }
                }
            }
            /*catch (Exception e)
            {
                Console.WriteLine("Exception occured in GenerateBinaryWord: ");
                Console.WriteLine("Message: "+e.Message);
                Console.WriteLine("Target Site: "+e.TargetSite);
            }*/

            return null;
        }
        
        private BinaryWord BinaryWordFromReader(StreamReader dicReader, string word)
        {
            string dicLine;
            bool britDic = false;
            bool firstDef = true;
            string defLine = null, defWord = null;
            List<string> classes = new List<string>();
            int length = 1;

            while ((dicLine = dicReader.ReadLine()) != null)
            {
                if (dicLine.Contains("dbox-pg"))
                {
                    try
                    {
                        int start = dicLine.IndexOf("dbox-pg") + 9;
                        int end = dicLine.IndexOf('<', start);
                        classes.Add(dicLine.Substring(start, end - start).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0]);
                    }
                    catch (Exception e)
                    { }

                }

                if (dicLine.Contains("def-content"))
                {
                    if (!britDic && (!firstDef || defLine == null))
                    {
                        dicLine = dicReader.ReadLine();
                        if (dicLine.Contains("def-sub-list"))
                        {
                            dicLine = dicReader.ReadLine();
                        }

                        defLine = dicLine;

                        firstDef = false;
                    }
                    if (britDic)
                    {
                        if (firstDef)
                        {
                            dicLine = dicReader.ReadLine();
                            if (dicLine.Contains("def-sub-list"))
                            {
                                dicLine = dicReader.ReadLine();
                            }

                            defLine = dicLine;

                            firstDef = false;
                        }
                        else if(length < 4)
                        {
                            dicLine = dicReader.ReadLine();
                            if (dicLine.Contains("def-sub-list"))
                            {
                                dicLine = dicReader.ReadLine();
                            }

                            defLine += dicLine;
                            length++;
                        }
                    }
                }

                if (!britDic && dicLine.Contains("British Dictionary definitions for"))
                {
                    //Console.WriteLine("British dictionary separator");
                    britDic = true;
                    classes = new List<string>();
                    firstDef = true;
                }
            }

            try
            {
                string[] defSplit = defLine.Split(new char[] { '>', '(', ')', ',', '.', ':', ';', '!', '?', ' '}, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < defSplit.Length; i++)
                {
                    if (nouns.ContainsKey(defSplit[i]))
                    {
                        defWord = defSplit[i];
                        break;
                    }
                }
            }
            catch (NullReferenceException ex)
            {}
            

            BinaryWord bw = new BinaryWord(word, BinaryWord.FindClass(classes), defWord);
            return bw;
        }
        
        /// <summary>
        /// Checks spelling mistakes
        /// </summary>
        /// <param name="word">The word we want to check</param>
        /// <returns>The correct word, null if we can not find any</returns>
        public static string SpellChecker(string word)
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
        
        /// <summary>
        /// Finds a list of synonims
        /// </summary>
        /// <param name="word">The original word</param>
        /// <returns>The list returned</returns>
        public static List<string> Synonyms(string word)
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

        public static string StringFromArray(bool[] arr)
        {
            BinaryWord match20 = null, match18 = null, match16 = null, match14 = null;
            foreach (KeyValuePair<string, BinaryWord> entry in collection)
            {
                if (match20 == null && EqualArrays(arr, entry.Value.complete, 20))
                {
                    match20 = entry.Value;
                }
                if (match18 == null && EqualArrays(arr, entry.Value.complete, 18))
                {
                    match18 = entry.Value;
                }
                if (match16 == null && EqualArrays(arr, entry.Value.complete, 16))
                {
                    match16 = entry.Value;
                }
                if (match14 == null && EqualArrays(arr, entry.Value.complete, 14))
                {
                    match14 = entry.Value;
                }
            }
            if (match20 != null)
            {
                return match20.word;
            }
            if (match18 != null)
            {
                return match18.word;
            }
            if (match16 != null)
            {
                return match16.word;
            }
            if (match14 != null)
            {
                return match14.word;
            }
            return null;
        }

        public static bool[] ArrayFromString(string word)
        {
            word = SpellChecker(word);
            BinaryWord bw = null;
            if (collection.TryGetValue(word, out bw))
            {
                return bw.complete;
            }
            return null;
        }

        private static bool EqualArrays(bool[] fir, bool[] sec, int indecies)
        {
            for (int i = 1; i <= indecies && i <= fir.Length && i <= sec.Length; i++)
            {
                if (fir[fir.Length - i] != sec[sec.Length - i])
                {
                    return false;
                }
            }
            return true;
        }

    }
}

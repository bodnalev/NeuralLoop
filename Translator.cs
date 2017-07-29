using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Linq;

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
        public void LoadDictionary()
        {
            collection = new Dictionary<string, BinaryWord>();
            nouns = new Dictionary<string, BinaryWord>();
            int num = 0;
            using (StreamReader reader = new StreamReader(@"dictionary.txt"))
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
                    num++;
                }
            }
            Console.WriteLine("Loaded " + num + " words from dictionary.txt");
        }

        /// <summary>
        /// Reads the words.txt to extend the dictionary.txt with the generated BinaryWords
        /// </summary>
        public void ReadWordList()
        {
            LoadDictionary();

            using (StreamReader wordsReader = new StreamReader(@".\words.txt"))
            {
                using (StreamWriter writer = new StreamWriter(@"D:\Edu\Programming\ML\NeuralLoop\NeuralLoop\dictionary.txt", true))
                {
                    string wordsLine;
                    while ((wordsLine = wordsReader.ReadLine()) != null)
                    {
                        string[] ar = wordsLine.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        BinaryWord bw = GenerateBinaryWord(ar[0]);
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

        /// <summary>
        /// Generates a BinaryWord from a given word
        /// </summary>
        /// <param name="word">The BinaryWord will be the representative of this word</param>
        /// <returns>The returned BinaryWord </returns>
        public BinaryWord GenerateBinaryWord(string word)
        {
            Console.WriteLine("GenerateBinaryWord: "+word);
            try
            {
                HttpWebRequest dicReq = (HttpWebRequest)WebRequest.Create("http://www.dictionary.com/misspelling?term=" + word);
                dicReq.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                Console.WriteLine("Webrequest");

                using (HttpWebResponse dicRes = (HttpWebResponse)dicReq.GetResponse())
                {
                    using (StreamReader dicReader = new StreamReader(dicRes.GetResponseStream()))
                    {
                        if (collection.ContainsKey(word))
                        {
                            Console.WriteLine("contains key: "+word);
                            return null;
                        }
                        return BinaryWordFromReader(dicReader, word);
                    }
                }
            }
            catch (WebException ex)
            {
                Console.WriteLine("exception, we look the response");
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
                                Console.WriteLine("no results for misspelling");
                                return null;
                            }
                            if (dicLine.Contains("Did you mean"))
                            {
                                int start = dicLine.IndexOf("com/browse/") + 11;
                                int end = dicLine.IndexOf('"', start - 2);
                                string newWord = dicLine.Substring(start, end - start);
                                if (!newWord.Contains("%"))
                                {
                                    Console.WriteLine("New corrected word is: "+newWord);
                                    if (collection.ContainsKey(newWord))
                                    {
                                        Console.WriteLine("alread in: "+newWord);
                                        return null;
                                    }
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
        
        private BinaryWord BinaryWordFromReader(StreamReader dicReader, string word)
        {
            Console.WriteLine("StreamReader called for analizing");
            string dicLine;
            bool britDic = false;
            bool firstDef = true;
            string defWord = null;
            List<string> classes = new List<string>();

            while ((dicLine = dicReader.ReadLine()) != null)
            {
                if (dicLine.Contains("dbox-pg"))
                {
                    try
                    {
                        Console.WriteLine("int the dbox-pg (class definition)");
                        int start = dicLine.IndexOf("dbox-pg") + 9;
                        int end = dicLine.IndexOf('<', start);
                        classes.Add(dicLine.Substring(start, end - start).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0]);
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

                    Console.WriteLine("in the de-content (definition word line)");
                    Console.WriteLine(dicLine);

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
                    Console.WriteLine("British dictionary separator");
                    britDic = true;
                    classes = new List<string>();
                    firstDef = true;
                }
            }

            BinaryWord bw = new BinaryWord(word, BinaryWord.FindClass(classes), defWord);
            return bw;
        }

        /// <summary>
        /// Checks spelling mistakes
        /// </summary>
        /// <param name="word">The word we want to check</param>
        /// <returns>The correct word, null if we can not find any</returns>
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
        
        /// <summary>
        /// Finds a list of synonims
        /// </summary>
        /// <param name="word">The original word</param>
        /// <returns>The list returned</returns>
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
        
    }
}

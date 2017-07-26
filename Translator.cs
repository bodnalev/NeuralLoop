using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace NeuralLoop
{
    
    class Translator
    {
        public Dictionary<string, bool[]> collection;

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
            collection = new Dictionary<string, bool[]>();

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

        public string BaseWord(string word)
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

        class BinaryWord
        {

        }
    }
}

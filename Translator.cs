using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace NeuralLoop
{
    //
    class Translator
    {
        public void Read()
        {


            WebRequest wordsReq = WebRequest.Create("http://norvig.com/ngrams/count_1w.txt");
            WebResponse wordsRes = wordsReq.GetResponse();
            StreamReader wordsReader = new StreamReader(wordsRes.GetResponseStream());
            string wordsLine;
            while ((wordsLine = wordsReader.ReadLine()) != null)
            {
                string[] ar = wordsLine.Split(new char[]{'\t'}, StringSplitOptions.RemoveEmptyEntries);
                string word = ar[0];
                WebRequest dicReq = WebRequest.Create("http://www.thesaurus.com/browse/" + word);
                WebResponse dicRes = dicReq.GetResponse();
                StreamReader dicReader = new StreamReader(dicRes.GetResponseStream());
                string dicLine;
                while ((dicLine = dicReader.ReadLine()) != null)
                {
                    if (dicLine.Contains("no thesaurus results"))
                    {
                        break;
                    }
                    if (dicLine.Contains("common-word"))
                    {
                        Console.WriteLine(dicLine);
                    }
                }
                dicReader.Close();
            }
            wordsReader.Close();
        }

        public string SpellChecker(string word)
        {
            try
            {
                HttpWebRequest dicReq = (HttpWebRequest)WebRequest.Create("http://www.dictionary.com/browse/" + word + "?s=ts");
                HttpWebResponse dicRes = (HttpWebResponse)dicReq.GetResponse();
                StreamReader dicReader = new StreamReader(dicRes.GetResponseStream());
                string dicLine;
                while ((dicLine = dicReader.ReadLine()) != null)
                {
                    if (dicLine.Contains("There are no results for:"))
                    {
                        dicReader.Close();
                        return "no word";
                    }
                }
                dicReader.Close();
                return word;
            }
            catch (WebException ex)
            {
                try
                {
                    HttpWebRequest dicReq = (HttpWebRequest)WebRequest.Create("http://www.dictionary.com/misspelling?term=" + word + "&s=ts");
                    HttpWebResponse dicRes = (HttpWebResponse)dicReq.GetResponse();
                    StreamReader dicReader = new StreamReader(dicRes.GetResponseStream());
                    string dicLine;
                    while ((dicLine = dicReader.ReadLine()) != null)
                    {
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
                    dicReader.Close();
                    return "end of exception inside";
                }
                catch (WebException e)
                {
                    return "end of exception outside";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return "last return";
        }

        public bool inDictionary(string word)
        {

            try
            {
                HttpWebRequest dicReq =(HttpWebRequest) WebRequest.Create("http://www.dictionary.com/browse/" + word+ "?s=ts");
                dicReq.AllowAutoRedirect = true;
                dicReq.MaximumAutomaticRedirections = 2;
                HttpWebResponse dicRes =(HttpWebResponse) dicReq.GetResponse();
                StreamReader dicReader = new StreamReader(dicRes.GetResponseStream());
                string dicLine;
                while ((dicLine = dicReader.ReadLine()) != null)
                {
                    if (dicLine.Contains("There are no results for:"))
                    {
                        dicReader.Close();
                        return false;
                    }
                }
                dicReader.Close();
                return true;
            }
            catch (WebException ex)
            {
                Console.WriteLine(ex.Message);
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    Console.WriteLine("Status Code : {0}", ((HttpWebResponse)ex.Response).StatusCode);
                    Console.WriteLine("Status Description : {0}", ((HttpWebResponse)ex.Response).StatusDescription);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return false;
            
        }

        public List<string> Synonyms(string word)
        {
            List<string> res = new List<string>();
            WebRequest dicReq = WebRequest.Create("http://www.thesaurus.com/browse/" + word);
            WebResponse dicRes = dicReq.GetResponse();
            StreamReader dicReader = new StreamReader(dicRes.GetResponseStream());
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
                    int start = dicLine.IndexOf("com/browse/")+11;
                    int end = dicLine.IndexOf('"',start-2);
                    string similar = dicLine.Substring(start, end-start);
                    if (!similar.Contains("%"))
                    {
                        res.Add(similar);
                    }
                }
            }
            dicReader.Close();
            return res;
        }


    }
}

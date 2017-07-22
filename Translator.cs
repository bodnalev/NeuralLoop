using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace NeuralLoop
{
    //
    class Translator
    {
        public Dictionary<String, BinaryWord> collection;

        BinaryWord getWord(string s)
        {
            BinaryWord word;
            if (!collection.TryGetValue(s, out word))
            {
                word = new BinaryWord();
                WebRequest request = WebRequest.Create("http://www.dictionary.com/browse/"+s);
                WebResponse response = request.GetResponse();
                Stream data = response.GetResponseStream();
                string html = String.Empty;
                using (StreamReader sr = new StreamReader(data))
                {
                    html = sr.ReadToEnd();
                }
                //find content
                
            }
            return word;
        }



    }
}

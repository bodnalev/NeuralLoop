using System;
using System.Collections.Generic;
using NeuralLoop.Network;

namespace NeuralLoop
{
    class Program
    {
        static void Main(string[] args)
        {
            Translator tr = new Translator();
            /*List<string> synonims = tr.Synonyms("hope");
            foreach (string s in synonims)
            {
                Console.WriteLine(s);
            }*/

            Console.WriteLine(tr.SpellChecker("something"));
            Console.WriteLine(tr.SpellChecker("samething"));
            Console.WriteLine(tr.SpellChecker("smething"));
            Console.WriteLine(tr.SpellChecker("asdsomeasdthingasd"));


            Console.Read();
        }
    }
}

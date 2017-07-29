using System;

namespace NeuralLoop
{
    class Program
    {
        static void Main(string[] args)
        {
            Translator tr = new Translator();
            tr.ReadWordList();

            //tr.LoadDictionary();
            //tr.GenerateBinaryWord("days", false);

            //tr.GenerateBinaryWord("introduced");

            Console.Read();
        }
    }
}

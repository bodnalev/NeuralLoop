using System;

namespace NeuralLoop
{
    class Program
    {
        static void Main(string[] args)
        {
            Translator tr = new Translator();
            //tr.ReadWordList();

            tr.LoadDictionary();
            tr.GenerateBinaryWord("rome");

            //tr.GenerateBinaryWord("introduced");

            Console.Read();
        }
    }
}

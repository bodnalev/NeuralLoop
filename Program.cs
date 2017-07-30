using System;

namespace NeuralLoop
{
    class Program
    {
        static void Main(string[] args)
        {
            Translator tr = new Translator();
            tr.ReadWordList();

            Console.Read();
        }
    }
}

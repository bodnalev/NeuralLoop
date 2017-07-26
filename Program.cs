using System;
using System.Diagnostics;
using System.Collections.Generic;
using NeuralLoop.Network;

namespace NeuralLoop
{
    class Program
    {
        static void Main(string[] args)
        {
            Translator tr = new Translator();
            tr.ReadFile();



            Console.Read();
        }
    }
}

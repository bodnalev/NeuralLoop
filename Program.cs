using System;
using System.Collections.Generic;
using NeuralLoop.Network;

namespace NeuralLoop
{
    class Program
    {
        static void Main(string[] args)
        {
            int[] neuronNumbers = new int[] { 100000, 100000, 100000 };
            int[] synapseNumbers = new int[] { 700, 700, 700 };
            NeuralNetwork nn = new NeuralNetwork(neuronNumbers, synapseNumbers);
        }
    }
}

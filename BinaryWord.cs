using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;

namespace NeuralLoop
{
    /// <summary>
    /// Stores a word in binary format
    /// </summary>
    class BinaryWord
    {
        //TODO: finish toVector and parameters
        public int number;
        public bool[] type;

        public Vector<float> toVector()
        {
            return Vector<float>.Build.Dense(1,1);
        }
    }
}

using System;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Distributions;

using System.Collections.Generic;

namespace NeuralLoop.Network
{
    /// <summary>
    /// Represent the Neural Networks
    /// </summary>
    class NeuralNetwork
    {
        /// <summary>
        /// Stores the neuron numbers in the different layers including the input at [0] and the output at [Lenght-1]
        /// </summary>
        public int[] neuronNumbers;
        /// <summary>
        /// Stores the matrices for each layer
        /// </summary>
        public Matrix<float>[] matrices;
        /// <summary>
        /// Stores the biases for each layer
        /// </summary>
        public Vector<float>[] biases;

        /// <summary>
        /// Network parameters for randomizing the values
        /// </summary>
        public double randomMin = -0.1, randomMax = 0.4;

        /// <summary>
        /// Reducing the synapse numbers by setting their expected value
        /// </summary>
        public int[] expectedSynapses;
        private double[] synapseProbability;


        /// <summary>
        /// Constructs the network based on neuronNumbers,
        /// initializes the matrix and bias values with small random variables between randomMin and randomMax
        /// </summary>
        /// <param name="neuronNumbers">The array containing the neuron numbers in each layer (including the input)</param>
        /// <param name="expectedSynapses">The expected synapse number in each neuron per layer</param>
        /// <param name="isLin">The array storing wether each layer uses the linear transfer function</param>
        public NeuralNetwork(int[] neuronNumbers, int[] expectedSynapses)
        {
            ContinuousUniform ran = new ContinuousUniform(randomMin, randomMax);
            Bernoulli ber;
            this.neuronNumbers = neuronNumbers;
            this.expectedSynapses = expectedSynapses;
            
            //setting up the synapse probability
            synapseProbability = new double[expectedSynapses.Length];
            for (int i = 0; i < expectedSynapses.Length; i++)
            {
                if (expectedSynapses != null && expectedSynapses[i] < neuronNumbers[i + 1])
                {
                    synapseProbability[i] = (double)expectedSynapses[i] / neuronNumbers[i + 1];
                }
                else
                {
                    synapseProbability[i] = 1;
                }
            }

            matrices = new Matrix<float>[neuronNumbers.Length - 1];
            biases = new Vector<float>[neuronNumbers.Length - 1];

            for (int i = 0; i < neuronNumbers.Length - 1; i++)
            {
                matrices[i] = Matrix<float>.Build.Random(neuronNumbers[i + 1], neuronNumbers[i], ran);
                if (synapseProbability[i] < 1)
                {
                    ber = new Bernoulli(synapseProbability[i]);
                    matrices[i].CoerceZero(x => ber.Sample()==0 );
                }

                biases[i] = Vector<float>.Build.Dense(neuronNumbers[i + 1], -.2f);
            }
        }
        
        /// <summary>
        /// Constructs the network based on neuronNumbers,
        /// initializes the matrix and bias values with small random variables between randomMin and randomMax
        /// </summary>
        /// <param name="neuronNumbers">The array containing the neuron numbers in each layer (including the input)</param>
        public NeuralNetwork(int[] neuronNumbers) : this(neuronNumbers, null) {}


        public Vector<float> UnsupervisedLoop(Vector<float> input, float alpha)
        {
            int M = matrices.Length;

            /*
             * Calculate the mid values, the response of each layer between the first and last
             * to use this in the training
             */
            Vector<float>[] midValues = new Vector<float>[M + 1];

            for (int i = 0; i < M; i++)
            {
                midValues[i] = input.Clone();
                input = VectorFunction.lStep((matrices[i] * input) + biases[i]);
            }
            midValues[M] = input.Clone();

            for (int i = 0; i < matrices.Length; i++)
            {
                matrices[i] = matrices[i] +alpha * 
                    (Vector<float>.OuterProduct(VectorFunction.RemValue(midValues[i], 20), 
                    VectorFunction.RemValue(midValues[i + 1], 20)));
            }

            return input;
        }

        /*
        /// <summary>
        /// Trains the network with the given input/output pairs
        /// </summary>
        /// <param name="input">The input vector</param>
        /// <param name="output">The expected output vector</param>
        /// <param name="alpha">The training parameter</param>
        public void SupervisedBPTrain(Vector<short> input, Vector<short> output, short alpha)
        {
            int M = matrices.Length;
            
            Vector<short>[] midValues = new Vector<short>[M + 1];

            for (int i = 0; i < M; i++)
            {
                midValues[i] = input.Clone();
                input = isLin[i] ? (matrices[i] * input) + biases[i] : VectorFunction.lsim((matrices[i] * input) + biases[i]);
            }
            midValues[M] = input.Clone();

            
            Vector<short>[] sens = new Vector<short>[M];

            sens[M - 1] = -2 * VectorFunction.diffSimp(midValues[M]).PointwiseMultiply(output - midValues[M]);
            for (int i = M - 2; i >= 0; i--)
            {
                sens[i] = (isLin[i] ?
                    //If the transfer function is linear then use the identity diagonal matrix
                    Matrix<short>.Build.DiagonalIdentity(midValues[i + 1].Count) :
                    //If the transfer function is log-sigmoid then use the diffSimp to calculate the derivative
                    Matrix<short>.Build.Diagonal((VectorFunction.diffSimp(midValues[i + 1])).ToArray()))
                    * (matrices[i + 1].Transpose()) * sens[i + 1];
            }
            

            for (int i = 0; i < M; i++)
            {
                matrices[i] = matrices[i] - alpha * sens[i].OuterProduct(midValues[i]);
                biases[i] = biases[i] - alpha * sens[i];
            }

        }*/

        
        
        /// <summary>
        /// Calculates the response of the network for a given input
        /// </summary>
        /// <param name="input">The given input</param>
        /// <returns></returns>
        public Vector<float> Response(Vector<float> input)
        {
            for (int i = 0; i < matrices.Length; i++)
            {
                input = VectorFunction.lStep((matrices[i] * input)/100 + biases[i]);
            }
            return input;
        }




        /// <summary>
        /// Extends the network's neuron numbers at a given layer.
        /// </summary>
        /// <param name="extra">The extra neurons</param>
        /// <param name="atLayer">The layer to extend</param>
        /// <param name="defValue">The default value to fill</param>
        public void ExtendAt(int extra, int atLayer, float defValue)
        {
            ContinuousUniform rand = new ContinuousUniform(randomMin, randomMax);
            neuronNumbers[atLayer] += extra;

            Matrix<float> newMatrix;

            if (atLayer > 0)
            {
                Vector<float> newBias = Vector<float>.Build.Random(neuronNumbers[atLayer], rand);
                newBias.SetSubVector(0, biases[atLayer - 1].Count, biases[atLayer - 1]);
                biases[atLayer - 1] = newBias;

                newMatrix = Matrix<float>.Build.Random(neuronNumbers[atLayer - 1], neuronNumbers[atLayer], rand);
                newMatrix.SetSubMatrix(0, 0, matrices[atLayer - 1]);
                matrices[atLayer - 1] = newMatrix;
            }

            if (atLayer < matrices.Length)
            {
                newMatrix = Matrix<float>.Build.Random(neuronNumbers[atLayer], neuronNumbers[atLayer + 1], rand);
                newMatrix.SetSubMatrix(0, 0, matrices[atLayer]);
                matrices[atLayer] = newMatrix;
            }
        }

    }
}

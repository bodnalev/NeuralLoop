using System;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Distributions;

namespace NeuralLoop.Network
{
    /// <summary>
    /// Storest the important functions used by the Network class
    /// </summary>
    class VectorFunction
    {

        /// <summary>
        /// Simple hard limit transfer function for a single variable
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public static float step(float f)
        {
            if (f > 0)
            {
                return 1;
            }
            return 0;
        }

        /// <summary>
        /// Simple log-sigmoid transfer function for a single variable
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public static float lsim(float f)
        {
            return 1 / (1 + (float)Math.Exp(-f));
        }

        /// <summary>
        /// Simple hard limit transfer function for a vector
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Vector<float> step(Vector<float> v)
        {
            float[] ar = v.AsArray();
            for (int i = 0; i < ar.Length; i++)
            {
                ar[i] = ar[i] > 0 ? 1 : 0;
            }
            return Vector<float>.Build.Dense(ar);
        }

        /// <summary>
        /// Simple log-sigmoid transfer function for a vector
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Vector<float> lsim(Vector<float> v)
        {
            return (Vector<float>.Exp(-v) + Vector<float>.Build.Dense(v.Count, 1)).DivideByThis(1);
        }

        /// <summary>
        /// For a = lsim(n) then da/dn = (1-a)*a, it calculates(1-a)*a for a vector
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Vector<float> diffSimp(Vector<float> v)
        {
            return (Vector<float>.Build.Dense(v.Count, 1) - v).PointwiseMultiply(v);
        }

    }
}

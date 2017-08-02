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

        public static Vector<short> RemValue(Vector<short> v, short value)
        {
            short[] ar = v.AsArray();
            for (int i = 0; i < ar.Length; i++)
            {
                ar[i] =(short) (ar[i] - value);
            }
            return Vector<short>.Build.Dense(ar);
        }

        public static short lStep(short s)
        {
            if (s > 100)
            {
                return 100;
            }
            else if (s < 0)
            {
                return 0;
            }
            return s;
        }

        public static Vector<short> lStep(Vector<short> v)
        {
            short[] ar = v.AsArray();
            for (int i = 0; i < ar.Length; i++)
            {
                ar[i] = lStep(ar[i]);
            }
            return Vector<short>.Build.Dense(ar);
        }

        /// <summary>
        /// Simple hard limit transfer function for a single variable
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public static short step(short f)
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
        public static short lsim(short f)
        {
            return (short) (1 / (1 + (short)Math.Exp(-f)));
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
        public static Vector<short> lsim(Vector<short> v)
        {
            return (Vector<short>.Exp(-v) + Vector<short>.Build.Dense(v.Count, 1)).DivideByThis(1);
        }

        /// <summary>
        /// For a = lsim(n) then da/dn = (1-a)*a, it calculates(1-a)*a for a vector
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Vector<short> diffSimp(Vector<short> v)
        {
            return (Vector<short>.Build.Dense(v.Count, 1) - v).PointwiseMultiply(v);
        }

    }
}

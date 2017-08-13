/// <summary>
/// a Metamath proof verifier class
/// written in C# by Chris Capel.
/// (29-Oct-2010)
/// </summary>

using System;
using System.Collections.Generic;

namespace NeuralLoop.Mathematics.MMVerifier
{
    static class Extension
    {
        public static T Do<T, U>(this U me, Func<U, T> action)
        {
            return action(me);
        }
        public static IEnumerable<T> UC<T>(this IEnumerable<T> me)
        {
            return (IEnumerable<T>)me;
        }
    }
}

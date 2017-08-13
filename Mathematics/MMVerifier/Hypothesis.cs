/// <summary>
/// a Metamath proof verifier class
/// written in C# by Chris Capel.
/// (29-Oct-2010)
/// </summary>

using System.Linq;

namespace NeuralLoop.Mathematics.MMVerifier
{
    public abstract class Hypothesis : MMStatement
    {
        public SymbolString Statement;
        public override bool Equals(object obj)
        {
            return obj is EHyp && Statement.SequenceEqual(((EHyp)obj).Statement);
        }
        public override int GetHashCode()
        {
            return Statement.GetHashCode();
        }
    }
}

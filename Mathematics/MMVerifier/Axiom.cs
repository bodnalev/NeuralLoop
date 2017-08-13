/// <summary>
/// a Metamath proof verifier class
/// written in C# by Chris Capel.
/// (29-Oct-2010)
/// </summary>

using System.Collections.Generic;

namespace NeuralLoop.Mathematics.MMVerifier
{
    public class Axiom : MMStatement
    {
        public SymbolString Result = new SymbolString();
        public List<Hypothesis> Hypotheses = new List<Hypothesis>();
        public HashSet<Distinct> Distinct;
    }
}

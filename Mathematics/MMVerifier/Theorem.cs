
/// <summary>
/// a Metamath proof verifier class
/// written in C# by Chris Capel.
/// (29-Oct-2010)
/// </summary>

using System.Collections.Generic;

namespace NeuralLoop.Mathematics.MMVerifier
{
    public class Theorem : Axiom
    {
        public List<MMStatement> Proof = new List<MMStatement>();
    }
}

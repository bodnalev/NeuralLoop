/// <summary>
/// a Metamath proof verifier class
/// written in C# by Chris Capel.
/// (29-Oct-2010)
/// </summary>


using System.Collections.Generic;

namespace NeuralLoop.Mathematics.MMVerifier
{
    public class Frame
    {
        public HashSet<string> C = new HashSet<string>();
        public HashSet<string> V = new HashSet<string>();
        public HashSet<Distinct> D = new HashSet<Distinct>();
        public List<Hypothesis> Hyps = new List<Hypothesis>();
        public Dictionary<string, MMStatement> Statements = new Dictionary<string, MMStatement>();
    }
}

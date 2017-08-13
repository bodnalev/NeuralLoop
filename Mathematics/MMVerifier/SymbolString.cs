/// <summary>
/// a Metamath proof verifier class
/// written in C# by Chris Capel.
/// (29-Oct-2010)
/// </summary>


using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeuralLoop.Mathematics.MMVerifier
{
    public class SymbolString : List<string>
    {
        public SymbolString(IEnumerable<string> items) : base(items) { }
        public SymbolString() { }
        public override string ToString()
        {
            return this.Aggregate(new StringBuilder(), (sb, s) => sb.Append(s + " "),
                sb => {
                    sb.Length -= 1;
                    return sb.ToString();
                });
        }
    }
}

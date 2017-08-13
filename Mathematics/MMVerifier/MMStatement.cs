/// <summary>
/// a Metamath proof verifier class
/// written in C# by Chris Capel.
/// (29-Oct-2010)
/// </summary>


using System.IO;

namespace NeuralLoop.Mathematics.MMVerifier
{
    public abstract class MMStatement
    {
        public FileInfo File;
        public int Line;
        public int Column;
        public int ByteOffset;
        public string Name;
    }
}

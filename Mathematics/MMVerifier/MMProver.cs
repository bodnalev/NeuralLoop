using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralLoop.Mathematics.MMVerifier
{
    class MMProver : MMFile
    {
        private static TokenReader r;

        public MMProver(string address)
        {
            r = new TokenReader(new StreamReader(address));
        }

        public void Read()
        {
            fs.Push();
            string label = null;
            SymbolString stat = null;
            while (true)
            {
                string tok = ReadC(r);
                if (tok == null || tok == "$}") break;
                else if (tok == "$c")
                    ReadStat(r).ForEach(fs.AddC);
                else if (tok == "$v")
                    ReadStat(r).ForEach(fs.AddV);
                else if (tok == "$f")
                {
                    if (label == null)
                        throw new Exception("$f must have label");
                    stat = ReadStat(r);
                    if (stat.Count != 2)
                        throw new Exception("$f must have 2 symbols");
                    fs.AddF(label, stat);
                    label = null;
                }
                else if (tok == "$a")
                {
                    if (label == null)
                        throw new Exception("$a must have label");
                    stat = ReadStat(r);
                    fs.AddA(label, stat);
                    label = null;
                }
                else if (tok == "$e")
                {
                    if (label == null)
                        throw new Exception("$e must have label");
                    stat = ReadStat(r);
                    fs.AddE(label, stat);
                    label = null;
                }
                else if (tok == "$p")
                {
                    if (label == null)
                        throw new Exception("$p must have label");
                    stat = ReadStat(r);
                    int i = stat.IndexOf("$=");
                    if (i == -1)
                        throw new Exception("$p must contain proof after $=");

                    

                    var result = stat.Take(i);
                    var proofString = stat.Skip(i + 1);
                    Theorem t = new Theorem { Name = label, Result = new SymbolString(result) };
                    fs.AddP(label, t);
                    if (proofString.First() == "(")
                    {
                        t.Proof = UncompressProof(fs, t, proofString).ToList();
                    }
                    else
                    {
                        t.Proof = GetStatements(fs, proofString).ToList();
                    }
                    if (t.Proof.Count(n => n is Axiom && ((Axiom)n).Hypotheses == null) > 0)
                        throw new Exception();


                    //Notif
                    Console.WriteLine("We've reached a theorem Name: " + label);
                    Console.WriteLine("Statement: " + stat);
                    Console.Read();


                    //List of known statements:
                    Console.WriteLine("Statements we know so far:");
                    foreach (var statem in Statements)
                    {
                        Console.Write(statem.Name + " ");
                    }
                    Console.WriteLine();
                    Console.Read();

                    Console.WriteLine("Proof of the theorem is: ");
                    foreach (var step in t.Proof)
                    {
                        Console.Write(step.Name + " ");
                    }
                    Console.WriteLine();
                    Console.Read();

                    Statements.Add(t);
                    label = null;
                }
                else if (tok == "$d")
                {
                    stat = ReadStat(r);
                    fs.AddD(stat);
                }
                else if (tok == "${") Read();
                else if (tok[0] != '$')
                {
                    label = tok;
                }
                else
                    throw new Exception("Unexpected token " + tok);

            }
            fs.Pop();
        }

        private void AttemptProve(Theorem th)
        {

        }

    }
}

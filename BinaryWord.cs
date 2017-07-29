using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralLoop
{
    /// <summary>
    /// Represents a word with all the relevant information and function to set up properly
    /// </summary>
    public class BinaryWord
    {
        public enum WordClass { Noun, Verb, AdjeAdve, DeterConjPrepPronInterjPuncUnkn }
        public WordClass type;

        public static WordClass FindClass(List<string> classes)
        {
            if (classes.Contains("determiner") || classes.Contains("conjunction") 
                || classes.Contains("preposition") || classes.Contains("pronoun"))
            {
                return WordClass.DeterConjPrepPronInterjPuncUnkn;
            }
            foreach(string s in classes)
            {
                if (s.Equals("noun"))
                    return WordClass.Noun;
                if (s.Equals("verb"))
                    return WordClass.Verb;
                if (s.Equals("adjective"))
                    return WordClass.AdjeAdve;
                if (s.Equals("adverb"))
                    return WordClass.AdjeAdve;
            }
            return WordClass.DeterConjPrepPronInterjPuncUnkn;
        }



        //Variables
        public string word;
        public string defWord;
        public int dataNumber = 0;

        public bool[] header;       //2
        public bool[] data;         //18
        public bool[] complete;     //20


        //Constructors
        public BinaryWord(string word, WordClass type, string defWord)
        {
            this.word = word;
            this.defWord = defWord;
            this.type = type;
            switch (type)
            {
                case WordClass.Noun:
                    header = new bool[] { true, false };
                    break;
                case WordClass.Verb:
                    header = new bool[] { false, true };
                    break;
                case WordClass.AdjeAdve:
                    header = new bool[] { false, false };
                    break;
                default:
                    header = new bool[] { true, true };
                    break;
            }

            SetData(defWord, type);

            dataNumber = GetNumberFromData(data);

            complete = Concate(header, data);

            Console.WriteLine(ToConsole());
        }

        public BinaryWord(string source)
        {
            try
            {
                string[] spl = source.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                word = spl[0];
                defWord = "";
                dataNumber = int.Parse(spl[1]);
                data = GetDataFromNumber(dataNumber);

                switch (spl[2])
                {
                    case "10":
                        type = WordClass.Noun;
                        header = new bool[] { true, false };
                        break;
                    case "01":
                        type = WordClass.Verb;
                        header = new bool[] { false, true };
                        break;
                    case "00":
                        type = WordClass.AdjeAdve;
                        header = new bool[] { false, false };
                        break;
                    default:
                        type = WordClass.DeterConjPrepPronInterjPuncUnkn;
                        header = new bool[] { true, true };
                        break;
                }
                complete = Concate(header, data);

                Console.WriteLine(ToConsole());

            }
            catch (Exception e) { }
        }
        

        //Functions
        //Public
        private void SetData(string def, WordClass type)
        {
            BinaryWord defBinary;
            bool[] randomData;
            if (def == null || Translator.nouns.TryGetValue(def, out defBinary))
            {
                if (!FindRandomlyDifferent(10, 18, 14, type, out randomData))
                {
                    if (!FindRandomlyDifferent(20, 18, 16, type, out randomData))
                    {
                        if (!FindNextDifferent(18, 18, type, out randomData))
                        {
                            throw new OutOfMemoryException();
                        }
                    }
                }

            }
            else
            {
                bool[] oldData = defBinary.data;
                if (!FindRandomlyDifferent(oldData, 10, 4, type, out randomData))
                {
                    if (!FindRandomlyDifferent(oldData, 20, 6, type, out randomData))
                    {
                        if (!FindNextDifferent(oldData, 6, type, out randomData))
                        {
                            throw new OutOfMemoryException();
                        }
                    }
                }
            }
            data = randomData;
        }

        public bool[] GetDataFromNumber(int number)
        {
            return Convert.ToString(number, 2).PadLeft(18, '0').Select(s => s.Equals('1')).ToArray();
        }

        public int GetNumberFromData(bool[] data)
        {
            int mul = 1;
            int res = 0;
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i])
                {
                    res += mul;
                }
                mul *= 2;
            }
            return res;
        }

        public override string ToString()
        {
            return word + " " + dataNumber + " " + (header[0] ? '1' : '0') + (header[1] ? '1' : '0');
        }

        public string ToConsole()
        {
            string s = word + "\t" + defWord + "\t";
            for (int i = 0; i < complete.Length; i++)
            {
                s += complete[i] ? "1" : "0";
            }
            return s;
        }

        public string ToConsole(string message)
        {
            string s = word + "\t" + defWord + "\t";
            for (int i = 0; i < complete.Length; i++)
            {
                s += complete[i] ? "1" : "0";
            }
            s += "\t" + message;
            return s;
        }

        //Private
        private bool FindNextDifferent(int length, int differentPart, WordClass type, out bool[] data)
        {
            data = new bool[length];
            for (int i = 0; i < (int)Math.Pow(2, length); i++)
            {
                data = GetDataFromNumber(i);
                if (SameAll(data, differentPart, type))
                {
                    return true;
                }
            }
            return false;
        }

        private bool FindNextDifferent(bool[] oldData, int lengthAtEnd, WordClass type, out bool[] data)
        {
            data = new bool[lengthAtEnd];
            for (int i = 0; i < (int)Math.Pow(2, lengthAtEnd); i++)
            {
                data = GetDataFromNumber(i);
                data = Concate(oldData.Take<bool>(oldData.Length - lengthAtEnd).ToArray<bool>(), data);
                if (SameAll(data, data.Length, type))
                {
                    return true;
                }
            }
            return false;
        }

        private bool FindRandomlyDifferent(int attempts, int length, int differentPart, WordClass type, out bool[] data)
        {
            int attempt = 0;
            do
            {
                attempt++;
                data = GetRandom(length);
            }
            while (attempt < attempts && !SameAll(data, differentPart, type));
            if (attempt == attempts)
            {
                return false;
            }
            return true;
        }

        private bool FindRandomlyDifferent(bool[] oldData, int attempts, int lengthAtEnd, WordClass type, out bool[] data)
        {
            int attempt = 0;
            do
            {
                attempt++;
                data = GetRandom(lengthAtEnd);
                data = Concate(oldData.Take<bool>(oldData.Length - lengthAtEnd).ToArray<bool>(), data);
            }
            while (attempt < attempts && !SameAll(data, data.Length, type));
            if (attempt == attempts)
            {
                return false;
            }
            return true;
        }

        private bool[] GetRandom(int length)
        {
            Random r = new Random();
            bool[] ret = new bool[length];
            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] = r.NextDouble() >= 0.5f;
            }
            return ret;
        }

        private bool SameUntil(bool[] data1, bool[] data2, int digits)
        {
            for (int i = 0; i < data1.Length && i <= digits && i < data2.Length; i++)
            {
                if (data1[i] != data2[i])
                {
                    return false;
                }
            }
            return true;
        }

        private bool SameAll(bool[] data, int digits, WordClass type)
        {
            foreach (KeyValuePair<string, BinaryWord> entry in Translator.collection)
            {
                if (SameUntil(data, entry.Value.data, digits) && type == entry.Value.type)
                {
                    return false;
                }
            }
            return true;
        }

        private bool[] Concate(bool[] first, bool[] second)
        {
            bool[] ret = new bool[first.Length + second.Length];
            first.CopyTo(ret, 0);
            second.CopyTo(ret, first.Length);
            return ret;
        }

    }
}

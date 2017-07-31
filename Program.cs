using System;
using System.Windows.Forms;

namespace NeuralLoop
{
    class Program
    {
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Conversation());
        }
    }
}

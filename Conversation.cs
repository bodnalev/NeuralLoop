using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using NeuralLoop.Network;
using MathNet.Numerics.LinearAlgebra;

namespace NeuralLoop
{
    public partial class Conversation : Form
    {

        BackgroundWorker bw = new BackgroundWorker();
        NeuralNetwork nn;
        private Vector<short> nextInput;

        private string convHistory;
        private List<string> messageQueue;
        private List<string> inputSentenceQueue;
        private List<string> responseSentenceQueue;

        private bool canShowResponse = false;

        public Conversation()
        {
            InitializeComponent();
            convHistory = "";
            messageQueue = new List<string>();
            inputSentenceQueue = new List<string>();
            responseSentenceQueue = new List<string>();
            nn = new NeuralNetwork(new int[] { 10000, 10000 }, new int[] { 6000 });
            bw.DoWork += bw_DoWork;
            bw.RunWorkerCompleted += bw_Completed;



            nextInput = ProcessInput(Vector<short>.Build.Dense(10000));
            bw.RunWorkerAsync(nextInput);
        }

        private void inputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string s = inputBox.Text;
                string[] ar = s.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
                inputBox.Text = "";
            }
        }

        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            Vector<short> input =(Vector<short>) e.Argument;

            Vector<short> result = nn.UnsupervisedLoop(input, 1);

            e.Result = result;
        }

        private void bw_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                nextInput = ProcessInput(nextInput);
            }
            else
            {
                Vector<short> res = (Vector<short>)e.Result;
                ShowResMessage(res);
                nextInput = ProcessInput(res);
            }

            bw.RunWorkerAsync(nextInput);
        }

        private Vector<short> ProcessInput(Vector<short> original)
        {
            conversationBox.Text = convHistory;
            return null;
        }

        private void ShowResMessage(Vector<short> res)
        {
            Vector<short> resSub = res.SubVector(20, 20);
            bool[] comp = new bool[20];
            for (int i = 0; i < 20; i++)
            {
                comp[i] = (resSub.At(i) >= 50);
            }
            string s = Translator.StringFromArray(comp);

            responseSentenceQueue.Add(s);

            conversationBox.Text = convHistory;
        }

    }
}

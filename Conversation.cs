using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using NeuralLoop.Network;
using MathNet.Numerics.LinearAlgebra;
using System.Diagnostics;
using System.IO;
using NeuralLoop.Language;

namespace NeuralLoop
{
    public partial class Conversation : Form
    {

        private BackgroundWorker networkWorker = new BackgroundWorker();
        private BackgroundWorker loadingWorker = new BackgroundWorker();
        private NeuralNetwork nn;
        private Vector<float> nextInput = null;
        
        private List<KeyValuePair<string, bool>> inputSentenceQueue, responseBuffer, inputBuffer, convHistory;
        
        Stopwatch sw;

        private bool running = false;

        private bool loaded = false;

        //specialChars dec: 33-64

        public Conversation()
        {
            InitializeComponent();


            inputSentenceQueue = new List<KeyValuePair<string, bool>>();
            responseBuffer = new List<KeyValuePair<string, bool>>();
            convHistory = new List<KeyValuePair<string, bool>>();
            inputBuffer = new List<KeyValuePair<string, bool>>();
            
            nn = new NeuralNetwork(3000, 3000);


            networkWorker.DoWork += networkWorker_DoWork;
            networkWorker.RunWorkerCompleted += networkWorker_Completed;

            loadingWorker.DoWork += loadingWorker_DoWork;
            loadingWorker.RunWorkerCompleted += loadingWorker_Completed;
            
            sw = new Stopwatch();

            saveButton.Enabled = false;
        }

        private void inputBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (!loaded)
                {
                    return;
                }

                string inS = inputBox.Text.ToLower();
                List<string> sentence = new List<string>() {inS}, temp;

                //separate the common special characters (,.?!&@ etc)

                for (int i = 33; i <= 64; i++)
                {
                    temp = new List<string>();
                    foreach (string s in sentence)
                    {
                        string[] arr = s.Split(new char[] { (char)i }, StringSplitOptions.RemoveEmptyEntries);
                        temp.Add(arr[0]);
                        for (int j = 1; j < arr.Length; j++)
                        {
                            temp.Add(((char)i).ToString());
                            if (j < arr.Length)
                            {
                                temp.Add(arr[j]);
                            }
                        }
                    }
                    sentence = temp;
                }

                //Split using whitespaces

                temp = new List<string>();
                foreach (string s in sentence)
                {
                    string[] arr = s.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
                    temp.AddRange(arr);
                }
                sentence = temp;

                //check if it is a complete sentence

                if (sentence[sentence.Count - 1] != "." && sentence[sentence.Count - 1] != "?" && sentence[sentence.Count - 1] != "!")
                {
                    sentence.Add(".");
                }

                //Decide the source user and add to the queue accordingly

                if ((sentence[0].ToLower() == "program" || sentence[0].ToLower() == "p" || sentence[0].ToLower() == "prog"
                    || sentence[0].ToLower() == "computer" || sentence[0].ToLower() == "c" || sentence[0].ToLower() == "comp") &&
                    sentence[1] == ":")
                {
                    for (int k = 2; k < sentence.Count; k++)
                    {
                        inputSentenceQueue.Add(new KeyValuePair<string, bool>(sentence[k], false));
                    }
                }
                else if ((sentence[0].ToLower() == "user" || sentence[0].ToLower() == "u" || sentence[0].ToLower() == "us"
                    || sentence[0].ToLower() == "me" || sentence[0].ToLower() == "m") &&
                    sentence[1] == ":")
                {
                    for (int k = 2; k < sentence.Count; k++)
                    {
                        inputSentenceQueue.Add(new KeyValuePair<string, bool>(sentence[k], true));
                    }
                }
                else
                {
                    for (int k = 0; k < sentence.Count; k++)
                    {
                        inputSentenceQueue.Add(new KeyValuePair<string, bool>(sentence[k], true));
                    }
                }
                
                //clear input box

                inputBox.Text = "";
            }
        }

        private void networkWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            Vector<float> input =(Vector<float>) e.Argument;

            Vector<float> result = nn.SingleUnsupervisedLoop(input, 1);

            e.Result = result;
        }

        private void networkWorker_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                nextInput = ProcessInput(nextInput);
                Console.WriteLine("ERROR!");
            }
            else
            {
                Vector<float> res = (Vector<float>)e.Result;
                if (canGetResponse.Checked)
                {
                    ShowResMessage(res);
                    convHistory.AddRange(responseBuffer);
                    responseBuffer.Clear();
                }
                nextInput = ProcessInput(res);
            }

            if (running)
            {
                infoBox.Text = "Running at " + (int)(1000f / sw.ElapsedMilliseconds) + "Ups";
                sw.Restart();
                networkWorker.RunWorkerAsync(nextInput);
            }
        }

        private void loadingWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Translator.LoadDictionaryVoid();
        }
        
        private void loadingWorker_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            infoBox.Text = "Ready";
            runButton.Text = "Run";
            runButton.Enabled = true;
            loaded = true;
        }

        private Vector<float> ProcessInput(Vector<float> original)
        {
            if (inputSentenceQueue.Count > 0)
            {
                KeyValuePair<string, bool> nextItem = inputSentenceQueue[0];
                inputSentenceQueue.RemoveAt(0);
                inputBuffer.Add(nextItem);

                //empty input buffer if needed
                if (nextItem.Key == "." || nextItem.Key == "!" || nextItem.Key == "?")
                {
                    convHistory.AddRange(inputBuffer);
                    inputBuffer.Clear();
                }

                //set vector
                bool[] values = Translator.ArrayFromString(nextItem.Key);
                if (values != null)
                {
                    float[] ar = new float[40];
                    for (int i = 0; i < 20; i++)
                    {
                        ar[nextItem.Value ? i : i + 20] = (values[i] ? 1f : 0f);
                    }
                    original.SetSubVector(0, 40, Vector<float>.Build.Dense(ar));
                }
                else
                {
                    original.SetSubVector(0, 40, Vector<float>.Build.Dense(40));
                }
                
            }
            else
            {
                original.SetSubVector(0,40,Vector<float>.Build.Dense(40));
            }

            UpdateConversation();
            return original;
        }

        private void runButton_Click(object sender, EventArgs e)
        {
            if (!loaded)
            {
                runButton.Enabled = false;
                infoBox.Text = "Loading...";
                loadingWorker.RunWorkerAsync();
            }
            else
            {
                running = true;
                runButton.Enabled = false;
                cancelButton.Enabled = true;
                sw.Start();
                networkWorker.RunWorkerAsync(nextInput);
                saveButton.Enabled = false;
            }
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            running = false;
            sw.Reset();
            infoBox.Text = "Paused";
            runButton.Enabled = true;
            UpdateConversation();
            saveButton.Enabled = true;
        }

        //todo
        private void saveButton_Click(object sender, EventArgs e)
        {
            runButton.Enabled = false;
            cancelButton.Enabled = false;
            using (StreamWriter sw = new StreamWriter(@"D:\Edu\Programming\ML\NeuralLoop\NeuralLoop\matrixSave.txt", false))
            {
                Matrix<float> mat = nn.singleMatrix;
                for (int i = 0; i < mat.RowCount; i++)
                {
                    Vector<float> vec = mat.Row(i);
                    for (int j = 0; j < vec.Count; j++)
                    {
                        sw.Write(vec.At(j) + "\t");
                    }
                    sw.WriteLine();
                }
            }
            runButton.Enabled = true;
        }

        private void ShowResMessage(Vector<float> res)
        {
            Vector<float> resSub = res.SubVector(20, 20);
            bool[] comp = new bool[20];
            for (int i = 0; i < 20; i++)
            {
                comp[i] = (resSub.At(i) >= .5f);
            }
            string s = Translator.StringFromArray(comp);

            responseBuffer.Add(new KeyValuePair<string, bool>(s,false));

            if (s == "." || s == "?" || s == "!")
            {
                convHistory.AddRange(responseBuffer);
                responseBuffer.Clear();
            }

            UpdateConversation();
        }

        private void UpdateConversation()
        {
            string s = "";

            if (convHistory.Count == 0)
            {
                conversationBox.Text = "";
                return;
            }

            bool human = convHistory[0].Value;

            s = (human ? "USER: " : "PROGRAM: ");

            foreach (KeyValuePair<string, bool> entry in convHistory)
            {
                if (entry.Value != human)
                {
                    s += "\r\n"+(entry.Value ? "USER: " : "PROGRAM: ");
                    human = !human;
                }
                s += entry.Key + " ";
            }
            conversationBox.Text = s;
        }

    }
}

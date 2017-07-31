using System;
using System.Collections.Generic;
using NeuralLoop.Network;

namespace NeuralLoop
{
    class ConversationManager
    {
        static ConversationManager singleton;
        NeuralNetwork nn;

        public ConversationManager()
        {
            singleton = this;
            nn = new NeuralNetwork(new int[] {20000}, new int[] {5000});
        }

    }
}

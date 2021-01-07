using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asteroid_diameter_detection.NeuralNetwork
{
    class Layer
    {
        public Neuron[] Neurons { get; set; }

        public Layer(int neuronsAmount, int synapsesAmount = 0)
        {
            Neurons = new Neuron[neuronsAmount];
            // Tworznie neuronów z ich wejściowymi synapsami
            for (int i = 0; i < Neurons.Length; i++) Neurons[i] = new Neuron(synapsesAmount);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asteroid_diameter_detection.NeuralNetwork
{
    class Neuron
    {
        public double Value { get; set; }
        public double PreviousPartialDerivative { get; set; }
        public Synapse[] Synapses { get; set; }

        public Neuron(int inSynapses)
        {
            Synapses = new Synapse[inSynapses];
            // Tworzenie synapsów ( przypisując losowe wartości )
            for (int i = 0; i < inSynapses; i++) Synapses[i] = new Synapse();
        }
    }
}

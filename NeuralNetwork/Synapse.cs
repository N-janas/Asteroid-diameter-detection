using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asteroid_diameter_detection.NeuralNetwork
{
    class Synapse
    {
        private static Random gen = new Random();
        public double Weight { get; set; }
        public double PreviousWeight { get; set; }

        public Synapse()
        {
            Weight = gen.NextDouble();
            PreviousWeight = 0;
        }
    }
}

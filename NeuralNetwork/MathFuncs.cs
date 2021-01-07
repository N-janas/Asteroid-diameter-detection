﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asteroid_diameter_detection.NeuralNetwork
{
    class MathFuncs
    {
        public static double Sigmoid(double x)
        {
            return (1 / (1 + Math.Exp(-x)));
        }

        public static double SigmoidDerivative(double x)
        {
            return x * (1 - x);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Asteroid_diameter_detection.NeuralNetwork
{
    class Network
    {
        public int _weightsCount;
        private Layer[] _layers;
        private double _learningRate;
        private double[][] _expectedResults;
        private double _cost;

        public Network(int[] neuronsInLayers)
        {
            int layersAmount = neuronsInLayers.Length;
            // Domyśnle ustawienie length na 1
            _expectedResults = new double[1][] { new double[1] };

            // Zliczanie wag (potrzebne do zapisu)
            _weightsCount = 0;
            for (int j = 0; j < layersAmount - 1; j++)
            {
                _weightsCount += (neuronsInLayers[j] * neuronsInLayers[j + 1]);
            }

            _layers = new Layer[layersAmount];
            // Tworze kolejne warstwy z ilością podanych neuronów
            // 1 warstwa
            _layers[0] = new Layer(neuronsInLayers[0]);
            // kolejne warstwy z podawaną ilością synaps wejściowych
            for (int i = 1; i < layersAmount; i++) _layers[i] = new Layer(neuronsInLayers[i], neuronsInLayers[i - 1]);
            // Learning Rate
            _learningRate = 4; 

        }

        // Interface
        public void SetTrainingOutputs(double[][] trainingOutputs)
        {
            _expectedResults = trainingOutputs;
        }

        public void Train(double[][] trainingSet)
        {
            _cost = 0;
            for (int i = 0; i < trainingSet.Length; i++)
            {
                // Przesyłanie kolejnych wierszy ze zbioru treningowego
                PushOnInput(trainingSet[i]);
                FeedForward();
                CalculateCost(i);
                // Przesyłanie indeksu i, potrzebne do odwołania sie przy _expectedValues
                BackPropagate(i);
            }
        }

        public double[] Think(double[] problem)
        {
            // Wprowadza problemowy wektor
            PushOnInput(problem);
            // Przepuszcza go przez sieć i tworzy wynikowy wektor
            FeedForward();
            double[] result = new double[_expectedResults[0].Length];
            for (int i = 0; i < _layers[_layers.Length - 1].Neurons.Length; i++)
            {
                result[i] = _layers[_layers.Length - 1].Neurons[i].Value;
            }
            return result;

        }

        public void GetCost()
        {
            // Średnia kosztu
            Console.WriteLine("Current cost: " + (_cost / _expectedResults.Length));
        }

        // Private
        private void FeedForward()
        {
            // Neurony z drugiej warstwy ściągają wartości z wejścia i podają dalej
            // dlatego i=1
            double weightedSum = 0;
            for (int i = 1; i < _layers.Length; i++)
            {
                // Iteruje po neuronach warstwy
                for (int j = 0; j < _layers[i].Neurons.Length; j++)
                {
                    weightedSum = 0;
                    // Iteruje po ilości poprzednich podłączonych do obecnego neurona, synapsach
                    for (int m = 0; m < _layers[i].Neurons[j].Synapses.Length; m++)
                    {
                        // Weighted Sum
                        weightedSum += (_layers[i - 1].Neurons[m].Value * _layers[i].Neurons[j].Synapses[m].Weight);
                    }
                    // Nakładanie funkcji aktywacji na neuron
                    _layers[i].Neurons[j].Value = MathFuncs.Sigmoid(weightedSum);
                }
            }
        }

        private void BackPropagate(int row)
        {
            BackPropagationOutput(row);
            BackPropagationHidden();
        }

        private void BackPropagationOutput(int row)
        {
            int lastLayerNeurons = _layers[_layers.Length - 1].Neurons.Length;
            int connectionsToNeuron = _layers[_layers.Length - 1].Neurons[0].Synapses.Length;
            // Operowanie na każdym neuronie w ostaniej warstwie
            for (int i = 0; i < lastLayerNeurons; i++)
            {
                // output - ExpectedOutput
                // Pochodna po funkcji kosztu
                double difference = (_layers[_layers.Length - 1].Neurons[i].Value - _expectedResults[row][i]);

                // SigmoidDerivative to pochodna po funkcji aktywacji z neurona
                double nodeDelta = difference * MathFuncs.SigmoidDerivative(_layers[_layers.Length - 1].Neurons[i].Value);
                _layers[_layers.Length - 1].Neurons[i].PreviousPartialDerivative = nodeDelta;

                // Dla każdego neurona z ostatniej warstwy obsługiwanie jednej z jego synaps wejściowych
                for (int j = 0; j < connectionsToNeuron; j++)
                {
                    // Wartości neuronów z poprzedniej warstwy
                    double netInput = _layers[_layers.Length - 2].Neurons[j].Value;
                    double delta = -1 * nodeDelta * netInput;

                    // Aktualizacja wagi
                    // Wcześniejsza waga wpisywana na previousWeight, potrzebne do dalszej propagacji, ponieważ
                    // to ją będziemy uwzględniać ( w Weight będzie waga zaaktualizowana )
                    _layers[_layers.Length - 1].Neurons[i].Synapses[j].PreviousWeight = _layers[_layers.Length - 1].Neurons[i].Synapses[j].Weight;
                    _layers[_layers.Length - 1].Neurons[i].Synapses[j].Weight += (delta * _learningRate);
                }
            }
        }

        private void BackPropagationHidden()
        {
            // Iterowanie po warstwach wstecz, od przedostatniej warstwy
            // stop przy drugiej warstwie
            for (int i = _layers.Length - 2; i > 0; i--)
            {
                // Iterowanie po neuronach w warstwie
                for (int j = 0; j < _layers[i].Neurons.Length; j++)
                {
                    // Iterowanie po wejściowych synapsach neuronu
                    // Przygotowanie do aktualizacji ich wag
                    for (int k = 0; k < _layers[i].Neurons[j].Synapses.Length; k++)
                    {
                        // Poprzednia wartość neurona
                        double netInput = _layers[i - 1].Neurons[k].Value;
                        double sumPartial = 0;
                        // Iteracja po neuronach z warstwy następnej ( poprzedniej patrząc na propagacje
                        // jako na iteracje od końca do początku)
                        // Zbieranie wag i poprzednich pochodnych z wcześniejszych połączeń
                        for (int m = 0; m < _layers[i + 1].Neurons.Length; m++)
                        {
                            sumPartial += _layers[i + 1].Neurons[m].Synapses[j].PreviousWeight * _layers[i + 1].Neurons[m].PreviousPartialDerivative;
                        }

                        // Pochodna obecnej warstwy
                        double nodeDelta = sumPartial * MathFuncs.SigmoidDerivative(_layers[i].Neurons[j].Value);
                        _layers[i].Neurons[j].PreviousPartialDerivative = nodeDelta;

                        double delta = -1 * netInput * nodeDelta;

                        // Aktualizacja wagi
                        // Wcześniejsza waga wpisywana na previousWeight, potrzebne do dalszej propagacji, ponieważ
                        // to ją będziemy uwzględniać ( w Weight będzie waga zaaktualizowana )
                        _layers[i].Neurons[j].Synapses[k].PreviousWeight = _layers[i].Neurons[j].Synapses[k].Weight;
                        _layers[i].Neurons[j].Synapses[k].Weight += (delta * _learningRate);
                    }

                }
            }

        }

        // Dodaje do błędu całkowitego, błąd po jednym feedzie 
        private void CalculateCost(int row)
        {
            for (int i = 0; i < _expectedResults[row].Length; i++)
            {
                _cost += Math.Pow((_layers[_layers.Length - 1].Neurons[i].Value - _expectedResults[row][i]), 2);
            }

        }

        // Podaje wartości na pierwszą warstwe
        private void PushOnInput(double[] feed)
        {
            for (int i = 0; i < feed.Length; i++)
            {
                _layers[0].Neurons[i].Value = feed[i];
            }
        }

        #region Archiving

        public void SaveWeights(string path)
        {
            string[] w = new string[_weightsCount];
            int m = 0;
            // Zaczynając od drugiej warstwy
            for (int i = 1; i < _layers.Length; i++)
            {
                // Z każdego neurona
                for (int j = 0; j < _layers[i].Neurons.Length; j++)
                {
                    // Jego synapsy wejściowe
                    for (int k = 0; k < _layers[i].Neurons[j].Synapses.Length; k++)
                    {
                        w[m++] = _layers[i].Neurons[j].Synapses[k].Weight.ToString();
                    }
                }
            }
            File.WriteAllLines(path, w);
        }

        public void LoadWeights(string[] w)
        {
            int m = 0;
            // Zaczynając od drugiej warstwy
            for (int i = 1; i < _layers.Length; i++)
            {
                // Z każdego neurona
                for (int j = 0; j < _layers[i].Neurons.Length; j++)
                {
                    // Jego synapsy wejściowe
                    for (int k = 0; k < _layers[i].Neurons[j].Synapses.Length; k++)
                    {
                        _layers[i].Neurons[j].Synapses[k].Weight = Convert.ToDouble(w[m++].Replace('.', ','));
                    }
                }
            }
        }
        #endregion  

        // HELPER
        public void WypiszWagi()
        {
            for (int i = 1; i < _layers.Length; i++)
            {
                for (int j = 0; j < _layers[i].Neurons.Length; j++)
                {
                    for (int k = 0; k < _layers[i].Neurons[j].Synapses.Length; k++)
                    {
                        Console.WriteLine(_layers[i].Neurons[j].Synapses[k].Weight);
                    }
                }
            }
            Console.ReadKey();
            Console.Clear();
        }
    }
}

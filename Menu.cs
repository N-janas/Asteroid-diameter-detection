using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Asteroid_diameter_detection
{
    using NeuralNetwork;
    class Menu
    {
        static Network net;
        static double[][] data;

        public static void StartProgram()
        {
            // Wczytanie zbioru i przygotowywanie zbioru
            Console.WriteLine("Wczytywanie zbioru danych...");
            data = Dane.GetData();

            // Wprowadź podział zbioru na trening i test
            bool end = true;
            double proportionD = 0.7;
            do
            {
                Console.Clear();
                Console.WriteLine("Wczytywanie zbioru danych...");
                PrintProportionQ();
                try
                {
                    proportionD = Convert.ToDouble(Console.ReadLine().Replace('.', ','));
                    if (proportionD >= 0.1 && proportionD <= 0.9) end = true;
                    else
                    {
                        end = false;
                    }

                }
                catch (Exception)
                {
                    end = false;
                }
            } while (!end);
            // 

            

            Dane.ShuffleData(data);

            //data = Dane.TakeSample(data, 3000);

            Dane.NormalizeData(data);

            int proportion = Convert.ToInt32(data.Length * proportionD);

            // Tworzenie sieci
            Console.WriteLine("Tworzenie sieci neuronowej...");
            net = new Network(new int[] { 37, 12, 1 });

            ShowMenu(proportion);

        }

        #region Display

        private static void PrintMainMenu()
        {
            Console.Clear();
            Console.WriteLine("==== Asteroid_diameter_detection ====");
            Console.WriteLine(" Opcje : ");
            Console.WriteLine(" 1. Trenuj sieć");
            Console.WriteLine(" 2. Wyznacz skuteczność sieci");
            Console.WriteLine(" 3. Wczytaj wagi");
            Console.WriteLine(" 4. Zapisz wagi");
            Console.WriteLine(" 5. Zakończ");
            Console.Write("    -> ");
        }

        private static void ShowMenu(int p)
        {
            PrintMainMenu();
            bool notEnd = true;
            do
            {
                PrintMainMenu();
                ConsoleKeyInfo input = Console.ReadKey();
                switch (input.Key)
                {
                    case ConsoleKey.D1:
                        BeforeTrainNet(p);
                        break;

                    case ConsoleKey.D2:
                        Console.Clear();
                        TestNet(p);
                        break;

                    case ConsoleKey.D3:
                        LoadWeights();
                        break;

                    case ConsoleKey.D4:
                        SaveWeights();
                        break;

                    case ConsoleKey.D5:
                        notEnd = false;
                        break;

                    case ConsoleKey.D9: // DEBUG key
                        net.WypiszWagi();
                        break;
                    default:
                        break;
                }
            } while (notEnd);
        }

        private static void PrintTrainMenu()
        {
            Console.Clear();
            Console.WriteLine("==== TRENING ====");
            Console.WriteLine(" Ilość epok");
            Console.Write("    -> ");
        }

        private static void PrintProportionQ()
        {
            Console.WriteLine("Podaj proporcje zbioru treningowego (0.1 - 0.9)");
            Console.Write("    -> ");
        }

        private static void BeforeTrainNet(int p)
        {
            // Wprowadź liczbe epok
            PrintTrainMenu();
            int epochs = 1;
            bool end = true;
            do
            {
                try
                {
                    epochs = Convert.ToInt32(Console.ReadLine());
                    if (epochs > 0) end = true;
                    else
                    {
                        PrintTrainMenu();
                        end = false;
                    }
                }
                catch (Exception)
                {
                    end = false;
                    PrintTrainMenu();
                }
            } while (!end);

            TrainNet(epochs, p);
        }

        private static void TrainNet(int epochs, int proportion)
        {
            // Podział zbioru na trening zgodnie z proporcją
            double[][] training = new double[proportion][];
            Dane.GetTrainingSet(data, training, proportion);


            // Podział na Input i Output zbioru treningowego
            double[][] trainingOutputs = new double[training.Length][];
            double[][] trainingInputs = new double[training.Length][];
            Dane.SplitData(trainingOutputs, trainingInputs, training);

            // Ustawianie wyników dla treningu
            net.SetTrainingOutputs(trainingOutputs);

            // Trening
            for (int epoch = 0; epoch < epochs; epoch++)
            {
                net.Train(trainingInputs);
                if (epoch % 1 == 0)
                {
                    Console.Write(epoch + "  ");
                    net.GetCost();
                }
            }
            Console.WriteLine("Trening zakończony");
            Console.ReadKey(); 
        }

        private static void TestNet(int proportion)
        {
            // Podział zbioru na test zgodnie z proporcją
            double[][] test = new double[data.Length - proportion][];
            Dane.GetTestSet(data, test, proportion);

            // Podział na Input i Output zbioru testowego
            double[][] testOutputs = new double[test.Length][];
            double[][] testInputs = new double[test.Length][];
            Dane.SplitData(testOutputs, testInputs, test);


            // Test
            Console.WriteLine("==== TESTOWANIE ====");
            // Odstępstwo
            double offset = 0.17;
            int passed = 0;

            for (int q = 0; q < test.Length; q++)
            {
                double[] trial = net.Think(testInputs[q]);
                if (IsValid(trial[0], testOutputs[q][0], offset)) passed++;
                //Console.WriteLine("----------------------------");
                //Console.WriteLine("Expected : " + testOutputs[q][0]);
                //Console.WriteLine("Result : " + trial[0]);
                //Console.WriteLine("Offset : "+ Math.Abs(1 - (trial[0] / testOutputs[q][0])));
                //Console.WriteLine("----------------------------");

            }
            Console.WriteLine("Skuteczność = "+ (((double)passed / test.Length)*100) + " %");
            Console.ReadKey();
        }

        // Jeśli odstępstwo od oczekiwanego wyniku jest mniejsze niż offset to zwróć true
        private static bool IsValid(double result, double expected, double offset)
        {
            return Math.Abs(1 - (result / expected)) <= offset ? true : false;
        }

        private static void LoadWeights()
        {
            Console.Clear();
            string[] lines = File.ReadAllLines("synapticWeights.txt");
            if (lines.Length == net._weightsCount)
            {
                Console.WriteLine("Wczytano wagi");
                Console.ReadKey();
                net.LoadWeights(lines);
            }
            else
            {
                Console.WriteLine("Nie odpowiedni rozmiar sieci");
                Console.ReadKey();
            }
        }

        private static void SaveWeights()
        {
            // Zapis do pliku
            net.SaveWeights("synapticWeights.txt");
            Console.Clear();
            Console.WriteLine("Zapisano wagi");
            Console.ReadKey();
        }


        #endregion
    }
}

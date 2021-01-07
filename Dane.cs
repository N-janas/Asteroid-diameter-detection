using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Asteroid_diameter_detection
{
    class Dane
    {
        static Dictionary<string, double> spec_B = new Dictionary<string, double>();
        static Dictionary<string, double> spec_T = new Dictionary<string, double>();
        static Dictionary<string, double[]> asteroid_class = new Dictionary<string, double[]>();

        public static double[][] GetData()
        {
            string[][] data = DataToArray();

            // Drop rekordy : diameter = NaN
            data = DropDiamaterNaN(data);

            // Drop kolumny : name, extent, GM, IR, G
            data = DropColumns(data);

            // Umieszczanie kolumn z kategoriami na końcu
            data = SwapColumns(data);


            // Przygotuj unikatowe wartości (spec_B, spec_T, class)
            spec_B = Dict.ReturnDict(data, 21);
            spec_T = Dict.ReturnDict(data, 22);
            asteroid_class = Dict.ReturnBinaryDict(data, 25);


            // Konwertuj na double
            double[][] preparedData = ConvertToDataSet(data);

            // Uzupełnij numeryczne NaN (-1) średnimi
            for (int i=0; i<21; i++)
            {
                preparedData = MeanNaN(preparedData, i);
            }

            // Uzupełnienie spec_B, spec_T najpopularniejszą wartością
            preparedData = MostPopularNaN(preparedData, 21, spec_B);
            preparedData = MostPopularNaN(preparedData, 22, spec_T);

            return preparedData;
        }

        #region DataSet preparation

        private static double[][] ConvertToDataSet(string[][] data)
        {
            int count = data.Length;

            double[][] newData = new double[count][];
            for(int i=0; i< count; i++)
            {
                // Rozszerzamy tablice o 12, neo i pha (+2), class (+10)
                newData[i] = new double[data[i].Length + 12];
                
                // Konwertowanie danych numerycznych
                for (int j=0; j<21; j++)
                {
                    // Jeśli NaN => uzupełnij -1
                    if (String.IsNullOrEmpty(data[i][j]))
                    {
                        newData[i][j] = -1;
                    }
                    // Jeśli zapis typu ".123"
                    else if (data[i][j][0] == '.')
                    {
                        newData[i][j] = Convert.ToDouble(("0" + data[i][j]).Replace('.', ','));
                    }
                    else newData[i][j] = Convert.ToDouble(data[i][j].Replace('.', ','));
                }


                // Kodowanie etykiet spec_B, spec_T
                if (String.IsNullOrEmpty(data[i][21]))
                {
                    newData[i][21] = -1;
                }
                else spec_B.TryGetValue(data[i][21], out newData[i][21]);

                if (String.IsNullOrEmpty(data[i][22]))
                {
                    newData[i][22] = -1;
                }
                else spec_T.TryGetValue(data[i][22], out newData[i][22]);


                // Kodowanie neo, pha (rozbicie na neurony)
                if (data[i][23] == "N") { newData[i][23] = 0; newData[i][24] = 1; }
                else if (data[i][23] == "Y") { newData[i][23] = 1; newData[i][24] = 0; }

                if (data[i][24] == "N") { newData[i][25] = 0; newData[i][26] = 1; }
                else if (data[i][24] == "Y") { newData[i][25] = 1; newData[i][26] = 0; }


                // Kodowanie class (rozbicie na neurony)
                double[] tmp = new double[11];
                asteroid_class.TryGetValue(data[i][25], out tmp);
                for (int q = 0; q < 11; q++)
                {
                    newData[i][27 + q] = tmp[q];
                }
            }
            
            return newData;
        }

        private static string[][] SwapColumns(string[][] data)
        {
            string[][] swapped = new string[data.Length][];
            // Umieszczamy dane nienumeryczne na końcu (spec_B, spec_T, neo, pha, class)
            
            for (int i=0; i<data.Length; i++)
            {
                int k = 0;
                swapped[i] = new string[data[i].Length];
                for (int j=0; j<data[i].Length; j++)
                {
                    // Nie przepisujemy kolumn z kategoriami
                    if( j!= 12 && j!= 13 && j!=19 && j!=20 && j != 22)
                    {
                        swapped[i][k++] = data[i][j];
                    }
                }
                // Kolejność (spec_B, spec_T, neo, pha, class)
                foreach (var idx in new int[5] { 19,20, 12, 13, 22 })
                {
                    swapped[i][k++] = data[i][idx];
                }
                
            }
            return swapped;
        }

        private static double[][] MostPopularNaN(double[][] data, int column, Dictionary<string, double> dict)
        {

            List<double> vals = dict.Values.ToList();
            int[] popularity = new int[vals.Count];
            
            for (int i=0; i<data.Length; i++)
            {
                if (data[i][column] != -1)
                {
                    popularity[vals.IndexOf(data[i][column])] += 1;
                }
            }
            double mostPopular = vals[Array.IndexOf(popularity, popularity.Max())];

            for (int i = 0; i < data.Length; i++)
            {
                if (data[i][column] == -1)
                {
                    data[i][column] = mostPopular;
                }
            }

            return data;
        }

        private static double[][] MeanNaN(double[][] data, int column)
        {
            double licznik = 0.0;
            double mianownik = data.Length;
            for (int wiersz = 0; wiersz < data.Length; wiersz++)
            {

                double value = data[wiersz][column];
                if (value < 0)
                {
                    mianownik--;
                }
                else
                {
                    licznik += value;
                }
            }


            double mean = licznik / mianownik;
            for (int wiersz = 0; wiersz < data.Length; wiersz++)
            {
                double value = data[wiersz][column];
                if (value < 0)
                {
                    data[wiersz][column] = mean;
                }
                else
                {
                    continue;
                }
            }
            return data;
        }

        private static string[][] DropColumns(string[][] data)
        {
            // Indeksy
            // name=0, extent=16, GM=19, IR=22, G=25
            int cols = data[0].Length;
            string[][] trimmedData = new string[data.Length][];

            int m = 0;
            // Iteruj przez dataSet
            for(int i=0; i<data.Length; i++)
            {
                // W każdym wierszu
                trimmedData[i] = new string[cols - 5];
                for (int j=0; j<cols; j++)
                {
                    // Przepisz tylko potrzebne kolumny
                    if( j!=0 && j!=16 && j !=19 && j !=22 && j != 25)
                    {
                        trimmedData[i][m++] = data[i][j];
                    }
                }
                m = 0;
            }

            return trimmedData;
        }

        private static string[][] DropDiamaterNaN(string[][] data)
        {
            int rows = 0;
            for(int i=0; i<data.Length; i++)
            {
                // Kolumna Diameter
                if (!String.IsNullOrEmpty(data[i][15]))
                {
                    rows++;
                }
            }

            string[][] trimmedData = new string[rows][];

            int k = 0;
            // Iteracja po każdym wierszu
            for(int j=0; j<data.Length; j++)
            {
                // Jeśli coś znaleziono to wpisz
                if (!String.IsNullOrEmpty(data[j][15]))
                {
                    string[] tmp = data[j];
                    trimmedData[k++] = tmp;
                }
            }

            return trimmedData;
        }

        private static string[][] DataToArray()
        {
            string[] lines = File.ReadAllLines(@"Data\Asteroid_Updated.csv");
            lines = lines.Skip(1).ToArray();

            string[][] data = new string[lines.Length][];
            for (int i = 0; i < lines.Length; i++)
            {
                data[i] = lines[i].Split(',');
            }
            return data;
        }

        #endregion


        public static void ShuffleData(double[][] data)
        {
            Random rnd = new Random();
            int n = data.Length;
            for (int i = 0; i < (n - 1); i++)
            {
                int r = i + rnd.Next(n - i);
                double[] t = data[r];
                data[r] = data[i];
                data[i] = t;
            }
        }

        private static double NormalizeAlg(double el, double min, double max, double bot = 0.0, double top = 1.0)
        {
            el = (el - min) / (max - min) * (top - bot) + bot;
            return el;
        }

        public static void NormalizeData(double[][] data, double bot = 0.0, double top = 1.0)
        {
            // Normalizujemy numeryczne dane [0, 20] oraz ponumerowane etykiety spec_B, spec_T 21, 22
            for (int k = 0; k < 23; k++)
            {
                double max = data[0][k], min = data[0][k];

                for (int i = 0; i < data.Length; i++)
                {
                    // Znajdź min i max
                    if (data[i][k] > max) { max = data[i][k]; }
                    else if (data[i][k] < min) { min = data[i][k]; }
                }

                for (int j = 0; j < data.Length; j++)
                {
                    data[j][k] = NormalizeAlg(data[j][k], min, max);
                }
            }
        }

        // Dzieli tabele na dwie tabele: parametry wejściowe, ich wyniki
        public static void SplitData(double[][] expectedOutputVals, double[][] expectedInputVals, double[][] dataSet)
        {
            for (int i = 0; i < dataSet.Length; i++)
            {
                expectedOutputVals[i] = new double[] { dataSet[i][12] }; // w 12 kolumnie znajduje się diameter

                double[] inp = new double[dataSet[i].Length - 1];
                int k = 0;
                for (int j=0; j<dataSet[i].Length; j++)
                {
                    if( j!=12) inp[k++] = dataSet[i][j];

                }

                expectedInputVals[i] = inp;
            }
        }

        public static double[][] TakeSample(double[][] dataSet, int amount)
        {
            double[][] sample = new double[amount][];
            for (int i = 0; i < amount; i++)
            {
                sample[i] = dataSet[i];
            }
            return sample;
        }

        public static void GetTrainingSet(double[][] dataSet, double[][] trainingSet, int proportion)
        {
            for (int i = 0; i < proportion; i++)
            {
                trainingSet[i] = dataSet[i];
            }
        }

        public static void GetTestSet(double[][] dataSet, double[][] testSet, int proportion)
        {
            int k = proportion;
            for (int i = 0; i < dataSet.Length - proportion; i++)
            {
                testSet[i] = dataSet[k++];
            }
        }

    }
}

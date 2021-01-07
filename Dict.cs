using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asteroid_diameter_detection
{
    class Dict
    {
        private static string[] GetKeys(string[][] data, int column)
        {
            string[] items = new string[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                items[i] = data[i][column];
            }

            IEnumerable<string> uniqueItems = items.Distinct<string>();

            //Console.WriteLine("Unique array items: " + string.Join(",", uniqueItems));

            string[] distinctArray = new string[uniqueItems.Count()];
            for (int j = 0; j < distinctArray.Length; j++)
            {
                distinctArray[j] = uniqueItems.ElementAt(j);
            }

            return distinctArray;
        }

        private static double[] GetValues(int length)
        {
            double[] values = new double[length];
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = i + 1.0;
            }

            Random rnd = new Random();
            int n = values.Length;
            for (int i = 0; i < (n - 1); i++)
            {
                int r = i + rnd.Next(n - i);
                double t = values[r];
                values[r] = values[i];
                values[i] = t;
            }

            return values;
        }


        private static double[][] GetDoubleArrayOfValues(int length)
        {

            double[][] values = new double[length][];
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = new double[length];
            }

            //Console.WriteLine(values.Length);

            for (int wiersze = 0; wiersze < values.Length; wiersze++)
            {
                for (int kolumny = 0; kolumny < values.Length; kolumny++)
                {
                    if (wiersze == kolumny)
                    {
                        values[wiersze][kolumny] = 1.0;
                    }
                    else
                    {
                        values[wiersze][kolumny] = 0.0;
                    }
                }
            }

            Random rnd = new Random();
            int n = values.Length;
            for (int i = 0; i < (n - 1); i++)
            {
                int r = i + rnd.Next(n - i);
                double[] t = values[r];
                values[r] = values[i];
                values[i] = t;
            }

            return values;
        }
        private static Dictionary<string, double> GetDict(string[] keys, double[] values)
        {
            if (keys.Length != values.Length)
                Console.WriteLine("NIE TA SAMA DŁUGOŚĆ");

            Dictionary<string, double> dict = new Dictionary<string, double>();
            for (int i = 0; i < keys.Length; i++)
            {
                dict.Add(keys[i], values[i]);
            }

            return dict;
        }

        private static Dictionary<string, double[]> GetDict(string[] keys, double[][] values)
        {
            if (keys.Length != values.Length)
                Console.WriteLine("NIE TA SAMA DŁUGOŚĆ");

            Dictionary<string, double[]> dict = new Dictionary<string, double[]>();
            for (int i = 0; i < keys.Length; i++)
            {
                dict.Add(keys[i], values[i]);
            }

            return dict;
        }

        public static Dictionary<string, double> ReturnDict(string[][] data, int column)
        {
            string[] keys = GetKeys(data, column);
            double[] values = GetValues(keys.Length);
            return GetDict(keys, values);
        }

        public static Dictionary<string, double[]> ReturnBinaryDict(string[][] data, int column)
        {
            string[] keys = GetKeys(data, column);
            double[][] values = GetDoubleArrayOfValues(keys.Length);
            return GetDict(keys, values);
        }
    }
}

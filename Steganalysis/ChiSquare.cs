using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steganalysis
{
    internal class ChiSquare
    {

        public static double[] chiSquareAttackLeftToRight(Bitmap img)
        {
            int width = img.Width;
            int height = img.Height;
            int block = height / 10;
            int red, green, blue;
            int[] values = new int[256];
            double[] expectedValues = new double[128];
            int[] pov = new int[128];
            double[] pVal = new double[height];
            double[] chi = new double[height];

            for (int i = 0; i < values.Length; i++)
            {
                values[i] = 1;
                pVal[i] = 0;
            }

            for (int i = 0; i < height; i++)
            {

                for (int j = 0; j < width; j++)
                {
                    values[img.GetPixel(j, i).B]++;
                }
                for (int j = 0; j < values.Length/2; j++)
                {
                    pov[j] = values[2 * j];
                    expectedValues[j] = (values[2 * j] + values[2 * j + 1])/2;
                }
                double[] result = ChiSqTest(pov, expectedValues);
                chi[i] = result[0];
                pVal[i] = result[1];
            }
            return pVal;
        }

        public static double[] ChiSqTest(int[] observed,
      double[] expected)
        {
            // 1. вычислить статистику хи-квадрат
            double x = ChiSqStat(observed, expected);
            // 2. вычисление вероятности
            int df = observed.Length - 1;
            double pVal = ChiSqPval(x, df);
            // 3. возвращает оба значения
            double[] result = new double[] { x, pVal };
            return result;
        } // ChiSqTest


        public static double ChiSqStat(int[] observed,
      double[] expected)
        {
            double sum = 0.0;
            for (int i = 0; i < observed.Length; ++i)
            {
                sum += ((observed[i] - expected[i]) *
                  (observed[i] - expected[i])) / expected[i];
            }
            return sum;
        }

        public static double ChiSqPval(double x, int df)
        {
            if (x <= 0.0 || df < 1)
                throw new Exception("Bad arg in ChiSqPval()");

            double a = 0.0; 
            double y = 0.0;
            double s = 0.0;
            double z = 0.0;
            double ee = 0.0;
            double c;

            bool even;

            a = 0.5 * x;
            if (df % 2 == 0) even = true; else even = false;

            if (df > 1) y = Exp(-a);

            if (even == true) s = y;
            else s = 2.0 * Gauss(-Math.Sqrt(x));

            if (df > 2)
            {
                x = 0.5 * (df - 1.0);
                if (even == true) z = 1.0; else z = 0.5;
                if (a > 40.0)
                {
                    if (even == true) ee = 0.0;
                    else ee = 0.5723649429247000870717135; // log(sqrt(pi))
                    c = Math.Log(a); // log e
                    while (z <= x)
                    {
                        ee = Math.Log(z) + ee;
                        s = s + Exp(c * z - a - ee);
                        z = z + 1.0;
                    }
                    return s;
                } // a > 40.0
                else
                {
                    if (even == true) ee = 1.0;
                    else ee = 0.5641895835477562869480795 / Math.Sqrt(a);
                    c = 0.0;
                    while (z <= x)
                    {
                        ee = ee * (a / z);
                        c = c + ee;
                        z = z + 1.0;
                    }
                    return c * y + s;
                }
            } // df > 2
            else
            {
                return s;
            }
        } 

        private static double Exp(double x) 
        {
            if (x < -40.0)
                return 0.0;
            else
                return Math.Exp(x);
        }

        // Функция гауса
        public static double Gauss(double z)
        {
            double y;
            double p;
            double w;

            if (z == 0.0)
                p = 0.0;
            else
            {
                y = Math.Abs(z) / 2;
                if (y >= 3.0)
                {
                    p = 1.0;
                }
                else if (y < 1.0)
                {
                    w = y * y;
                    p = ((((((((0.000124818987 * w
                      - 0.001075204047) * w
                      + 0.005198775019) * w
                      - 0.019198292004) * w + 0.059054035642) * w
                      - 0.151968751364) * w + 0.319152932694) * w
                      - 0.531923007300) * w + 0.797884560593) * y * 2.0;
                }
                else
                {
                    y = y - 2.0;
                    p = (((((((((((((-0.000045255659 * y
                      + 0.000152529290) * y - 0.000019538132) * y
                      - 0.000676904986) * y + 0.001390604284) * y
                      - 0.000794620820) * y - 0.002034254874) * y
                     + 0.006549791214) * y - 0.010557625006) * y
                    + 0.011630447319) * y - 0.009279453341) * y
                   + 0.005353579108) * y - 0.002141268741) * y
                  + 0.000535310849) * y + 0.999936657524;
                }
            }

            if (z > 0.0)
                return (p + 1.0) / 2;
            else
                return (1.0 - p) / 2;
        } // Gauss()

        public static void ShowVector(int[] vector)
        {
            for (int i = 0; i < vector.Length; ++i)
                Console.Write(vector[i].ToString() + "  ");
            Console.WriteLine();
        }

        public static void ShowVector(double[] vector)
        {
            for (int i = 0; i < vector.Length; ++i)
                Console.Write(vector[i].ToString("F1") + "  ");
            Console.WriteLine();
        }


    }
}

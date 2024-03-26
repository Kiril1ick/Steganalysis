using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;

namespace Steganalysis
{
    public class ZhaoKoch
    {
        int SizeOfSegment = 8;
        string ComponentOfEmbedding;

        Point p1;
        Point p2;
        public ZhaoKoch(string size, string component = "Blue")
        {
            SizeOfSegment = DetermineSizeOfSegment(size);
            DeterminePointsOfCoefficients();
            ComponentOfEmbedding = component;
        }

        private int DetermineSizeOfSegment(string size)
        {
            int result = 0;
            switch (size)
            {
                case "2x2":
                    result = 2;
                    break;
                case "4x4":
                    result = 4;
                    break;
                case "8x8":
                    result = 8;
                    break;

            }
            return result;
        }

        public double[] chartData(Image modifImage)
        {
            Bitmap modifPicture = new Bitmap(modifImage);
            sendMessToConsol("Start analysis...");
            int x = modifPicture.Width;
            int y = modifPicture.Height;

            Byte[,] ArrayForEmbedding = new Byte[x, y];
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    if (ComponentOfEmbedding == "Blue")
                    {
                        ArrayForEmbedding[i, j] = modifPicture.GetPixel(i, j).B;
                    }
                    else if (ComponentOfEmbedding == "Green")
                    {
                        ArrayForEmbedding[i, j] = modifPicture.GetPixel(i, j).G;
                    }
                    else
                    {
                        ArrayForEmbedding[i, j] = modifPicture.GetPixel(i, j).R;
                    }
                }
            }

            int Nc = x * y / (SizeOfSegment * SizeOfSegment); //общее число сегментов
            List<byte[,]> C = new List<byte[,]>();

            sendMessToConsol("Segmentation of image components...");
            separation(ArrayForEmbedding, C, x, y, SizeOfSegment);
            sendMessToConsol("Discrete cosine transform performance...");
            List<double[,]> DKP = new List<double[,]>();
            foreach (byte[,] b in C)
            {
                DKP.Add(dkp(b));
            }

            double[] c = new double[DKP.Count];
            int iter = 0;
            foreach (double[,] b in DKP)
            {
                double result = TestFoo(b);
                c[iter] = result;
                iter++;
            }

            return c;
        }

        public double testAnalys(double[] data)
        {
            double result = 0;

            Dictionary<int, int> value = new Dictionary<int, int>();

            for(int i =0; i < data.Length; i++)
            {
                if (value.ContainsKey(((int)data[i]))) value[(int)data[i]]++;
                else if ((int)data[i] >= 24) value.Add((int)data[i],1);
            }
            var sortedValue = from entry in value orderby entry.Value descending select entry;

            var a = sortedValue.ElementAt(0);
            var b = sortedValue.ElementAt(1);
            result = (a.Value+b.Value)/(double)data.Length;

            return result;
        }
        byte[,] submatrix(byte[,] one, int a, int b, int c, int d)
        {
            byte[,] temp = new byte[b - a + 1, d - c + 1];
            for (int i = a, k = 0; i <= b; i++, k++)
                for (int j = c, l = 0; j <= d; j++, l++)
                    temp[k, l] = one[i, j];
            return temp;
        }

        double FindCoefficient(int arg)
        {
            if (arg == 0) return 1.0 / Math.Sqrt(2);
            return 1;
        }
        double[,] dkp(byte[,] one)
        {
            int n = one.GetLength(0);
            double[,] two = new double[n, n];
            double temp;
            for (int v = 0; v < n; v++)
            {
                for (int u = 0; u < n; u++)
                {
                    temp = 0;
                    for (int i = 0; i < n; i++)
                    {
                        for (int j = 0; j < n; j++)
                        {
                            temp += one[i, j] * Math.Cos(Math.PI * v * (2 * i + 1) / (2 * n)) *
                                Math.Cos(Math.PI * u * (2 * j + 1) / (2 * n));
                        }
                    }
                    two[v, u] = FindCoefficient(u) * FindCoefficient(v) * temp / (Math.Sqrt(2 * n));
                }
            }
            return two;
        }

        private void separation(byte[,] B, List<byte[,]> C, int sizeX, int sizeY, int sizeSegment)
        {
            int Nx = sizeX / sizeSegment;
            int Ny = sizeY / sizeSegment;
            for (int i = 0; i < Nx; i++)
            {
                int startX = i * sizeSegment;
                int endX = startX + sizeSegment - 1;
                for (int j = 0; j < Ny; j++)
                {
                    int startY = j * sizeSegment;
                    int endY = startY + sizeSegment - 1;
                    C.Add(submatrix(B, startX, endX, startY, endY));
                }
            }
        }

        private void sendMessToConsol(String mess)
        {
            Console.WriteLine(mess);
        }


        double TestFoo(double[,] bloc)
        {
            double result = Math.Abs(Math.Abs(bloc[p1.X, p1.Y]) - Math.Abs(bloc[p2.X, p2.Y]));

            return result;
        }

        private void DeterminePointsOfCoefficients()
        {
            if (SizeOfSegment == 2)
            {
                p1 = new Point(1, 0);
                p2 = new Point(1, 1);
            }
            else if (SizeOfSegment == 4)
            {
                p1 = new Point(3, 2);
                p2 = new Point(2, 3);
            }
            else
            {
                p1 = new Point(6, 3);
                p2 = new Point(3, 6);
            }
        }

    }

}

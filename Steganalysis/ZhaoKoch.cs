using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Steganalysis
{
    public class ZhaoKoch
    {
        int SizeOfSegment = 8;
        string ComponentOfEmbedding;

        Point p1;
        Point p2;
        public ZhaoKoch(int size = 8, string component = "Blue")
        {
            SizeOfSegment = size;
            ComponentOfEmbedding = component;
        }

        public string doAnalysis(Image modifImage)
        {
            Bitmap modifPicture = new Bitmap(modifImage);
            sendMessToConsol("Start analysis...");
            string result = null;
            try
            {
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

                int a = 0;
            }
            catch (Exception e)
            {
                return "Failed to analysis.";
            }
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

    }

}

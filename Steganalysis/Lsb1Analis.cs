using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Steganalysis
{
    public class Lsb1Analis
    {

        public static Bitmap drawPic(Bitmap img, string color = "all")
        {
            Bitmap newImg = new Bitmap(img);
            for (int i = 0; i < img.Width; i++)
            {
                for (int j = 0; j < img.Height; j++)
                {
                    if (color == "green")
                    {
                        Color pixel = img.GetPixel(i, j);
                        if (pixel.G % 2 == 0)
                        {
                            newImg.SetPixel(i, j, Color.Black);
                        }
                        else
                        {
                            newImg.SetPixel(i, j, Color.White);
                        }
                    }
                    else if (color == "red")
                    {
                        Color pixel = img.GetPixel(i, j);
                        if (pixel.R % 2 == 0)
                        {
                            newImg.SetPixel(i, j, Color.Black);
                        }
                        else
                        {
                            newImg.SetPixel(i, j, Color.White);
                        }
                    }
                    else if (color == "blue")
                    {
                        Color pixel = img.GetPixel(i, j);
                        if (pixel.B % 2 == 0)
                        {
                            newImg.SetPixel(i, j, Color.Black);
                        }
                        else
                        {
                            newImg.SetPixel(i, j, Color.White);
                        }
                    }
                    else
                    {
                        Color pixel = img.GetPixel(i, j);
                        byte r = 0;
                        byte g = 0;
                        byte b = 0;
                        if (pixel.R % 2 == 0) r = 255;
                        if (pixel.G % 2 == 0) g = 255;
                        if (pixel.B % 2 == 0) b = 255;
                        newImg.SetPixel(i, j, Color.FromArgb(r, g, b));
                    }
                }
            }
            return newImg;
        }

        public static Bitmap analisys(Bitmap img)
        {
            double[] chiArr = new double[img.Height];
            for(int i = 0; i < img.Height; i++) 
            {
                int[] arr = Enumerable.Repeat(1, img.Width).ToArray();
                for (int k = 0; k < arr.Length; k++)
                {
                    int c = img.GetPixel(k, i).B;
                    for (int j = 0; j < img.Height; j++)
                    {
                        if (c == img.GetPixel(j, i).B) arr[k] += 1;
                    }
                }
                int[] fact = new int[img.Width/2];
                double[] teor = new double[img.Width/2];
                for (int j = 0; j < img.Width/2; j++)
                {
                    fact[j] = arr[2*j];
                    teor[j] = (arr[2*j] + arr[2*j+1])/2;
                }

                double chiSqr = 0;

                for (int j = 0; j < fact.Length; j++)
                {
                    chiSqr += Math.Pow(fact[j] - teor[j], 2) / teor[j];
                }

                chiArr[i] = chiSqr;

                if (chiSqr > 35)
                {
                    for (int b = 0; b < arr.Length; b++)
                    {
                        img.SetPixel(b, i, Color.Black);
                    }
                }
                int o = 0;
            }
            return img;
        }

        public static double[] analisysB(Bitmap img)
        {
            double[] chiArr = new double[img.Height];

            for (int i = 0; i < img.Height; i++)
            {

                int[] h = Enumerable.Repeat(0, 256).ToArray();

                for (int l = 0; l < h.Length; l++)
                {
                    for (int j = 0; j < img.Width; j++)
                    {
                        if (l == img.GetPixel(j, i).B) h[l]++;
                    }
                }

                double[] expected = new double[h.Length];
                int[] observed = new int[h.Length];

                for (int j = 0; j < h.Length/2;j++)
                {
                    expected[j] = (h[2*j] + h[2*j + 1]) / 2;
                    observed[j] = h[2 * j];
                }

                double chiSqrt = 0;

                for (int j = 0; j < 256; j++)
                {
                    chiSqrt += Math.Pow((observed[j] - expected[j]),2)/2;
                }

                chiArr[i] = chiSqrt;
            }
            return chiArr;
        }

    }

}

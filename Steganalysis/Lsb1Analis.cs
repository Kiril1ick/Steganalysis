using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using static System.Net.Mime.MediaTypeNames;

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

        public static double[] analisys(Bitmap img)
        {
            double[] result = ChiSquare.chiSquareAttackLeftToRight(img);
            return result;
        }

    }

}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steganalysis
{
    internal class RSAnalysis
    {
        //Переменные

        // Анализ красного канала
        public static int ANALYSIS_COLOUR_RED = 0;

        // Анализ зеленого канала
        public static int ANALYSIS_COLOUR_GREEN = 1;

        // Анализ синего канала
        public static int ANALYSIS_COLOUR_BLUE = 2;

        // Использованная маска для групп
        private int[][] mMask;

        // Длина маски по оси x.
        private int mM;

        // Длина маски по оси y.
        private int mN;
        //CONSTRUCTORS

        /**
         * Создает новый RS-анализ с заданным размером маски m x n.
         *
         * Каждый переменный бит имеет значение 1. Например, для маски размером 2x2 результирующая маска будет равна {1,0;0,1}. 
         * Используются две маски, одна из которых является обратной по отношению к другой.
         */
        public RSAnalysis(int m, int n)
        {

            //две маски
            mMask = new int[2][];
            for (int i = 0; i < mMask.Length; i++)
            {
                mMask[i] = new int[m*n];
            }

            //прохождение по маскам и установка чередующихся бит
            int k = 0;
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    if (((j % 2) == 0 && (i % 2) == 0)
                            || ((j % 2) == 1 && (i % 2) == 1))
                    {
                        mMask[0][k] = 1;
                        mMask[1][k] = 0;
                    }
                    else
                    {
                        mMask[0][k] = 0;
                        mMask[1][k] = 1;
                    }
                    k++;
                }
            }

            //установка размера маски.
            mM = m;
            mN = n;
        }



        // Выполняет RS-анализ изображения.
        public double[] doAnalysis(Bitmap image, int colour, bool overlap)
        {

            //получает размер изображения
            int imgx = image.Width, imgy = image.Height;

            int startx = 0, starty = 0;
            Color[] block = new Color[mM * mN];
            double numregular = 0, numsingular = 0;
            double numnegreg = 0, numnegsing = 0;
            double numunusable = 0, numnegunusable = 0;
            double variationB, variationP, variationN;

            while (startx < imgx && starty < imgy)
            {
                for (int m = 0; m < 2; m++)
                {
                    //получение блока данных
                    int k = 0;
                    for (int i = 0; i < mN; i++)
                    {
                        for (int j = 0; j < mM; j++)
                        {
                            block[k] = image.GetPixel(startx + j, starty + i);
                            k++;
                        }
                    }

                    //получение вариации блока
                    variationB = getVariation(block, colour);

                    // замена блока
                    block = flipBlock(block, mMask[m]);
                    variationP = getVariation(block, colour);
                    //обратная замена
                    block = flipBlock(block, mMask[m]);

                    //negative mask
                    mMask[m] = invertMask(mMask[m]);
                    variationN = getNegativeVariation(block, colour, mMask[m]);
                    mMask[m] = invertMask(mMask[m]);

                    //Определение группы блока

                    //обычяная маска
                    if (variationP > variationB)
                        numregular++;
                    if (variationP < variationB)
                        numsingular++;
                    if (variationP == variationB)
                        numunusable++;

                    //обратная маска
                    if (variationN > variationB)
                        numnegreg++;
                    if (variationN < variationB)
                        numnegsing++;
                    if (variationN == variationB)
                        numnegunusable++;

                }
                //переход к следующему
                if (overlap)
                    startx += 1;
                else
                    startx += mM;

                if (startx >= (imgx - 1))
                {
                    startx = 0;
                    if (overlap)
                        starty += 1;
                    else
                        starty += mN;
                }
                if (starty >= (imgy - 1))
                    break;
            }

            // расчет x
            double totalgroups = numregular + numsingular + numunusable;
            double[] allpixels = getAllPixelFlips(image, colour, overlap);
            double x = getX(numregular, numnegreg, allpixels[0], allpixels[2],
                    numsingular, numnegsing, allpixels[1], allpixels[3]);

            //рассчет предполагаемого процента перевернутых пикселей и длины сообщения
            double epf, ml;
            if (2 * (x - 1) == 0)
                epf = 0;
            else
                epf = Math.Abs(x / (2 * (x - 1)));

            if (x - 0.5 == 0)
                ml = 0;
            else
                ml = Math.Abs(x / (x - 0.5));

            //количество групп
            double[] results = new double[28];

            //сохранение результата
            results[0] = numregular;
            results[1] = numsingular;
            results[2] = numnegreg;
            results[3] = numnegsing;
            results[4] = Math.Abs(numregular - numnegreg);
            results[5] = Math.Abs(numsingular - numnegsing);
            results[6] = (numregular / totalgroups) * 100;
            results[7] = (numsingular / totalgroups) * 100;
            results[8] = (numnegreg / totalgroups) * 100;
            results[9] = (numnegsing / totalgroups) * 100;
            results[10] = (results[4] / totalgroups) * 100;
            results[11] = (results[5] / totalgroups) * 100;

            //результат всех пикселей
            results[12] = allpixels[0];
            results[13] = allpixels[1];
            results[14] = allpixels[2];
            results[15] = allpixels[3];
            results[16] = Math.Abs(allpixels[0] - allpixels[1]);
            results[17] = Math.Abs(allpixels[2] - allpixels[3]);
            results[18] = (allpixels[0] / totalgroups) * 100;
            results[19] = (allpixels[1] / totalgroups) * 100;
            results[20] = (allpixels[2] / totalgroups) * 100;
            results[21] = (allpixels[3] / totalgroups) * 100;
            results[22] = (results[16] / totalgroups) * 100;
            results[23] = (results[17] / totalgroups) * 100;

            //общие результаты
            results[24] = totalgroups;
            results[25] = epf;
            results[26] = ml;
            results[27] = ((imgx * imgy * 3) * ml) / 8;

            return results;
        }

        /**
         * Возвращает значение x для уравнения p=x(x/2) RS.
         *
         * @param r The value of Rm(p/2).
         * @param rm The value of R-m(p/2).
         * @param r1 The value of Rm(1-p/2).
         * @param rm1 The value of R-m(1-p/2).
         * @param s The value of Sm(p/2).
         * @param sm The value of S-m(p/2).
         * @param s1 The value of Sm(1-p/2).
         * @param sm1 The value of S-m(1-p/2).
         * @return The value of x.
         */
        private double getX(double r, double rm, double r1, double rm1,
                double s, double sm, double s1, double sm1)
        {

            double x = 0; //точка пересечения

            double dzero = r - s; // d0 = Rm(p/2) - Sm(p/2)
            double dminuszero = rm - sm; // d-0 = R-m(p/2) - S-m(p/2)
            double done = r1 - s1; // d1 = Rm(1-p/2) - Sm(1-p/2)
            double dminusone = rm1 - sm1; // d-1 = R-m(1-p/2) - S-m(1-p/2)

            //получаем x как корень уравнения
            //2(d1 + d0)x^2 + (d-0 - d-1 - d1 - 3d0)x + d0 - d-0 = 0
            //x = (-b +or- sqrt(b^2-4ac))/2a
            //где ax ^ 2 + bx + c = 0, и это форма уравнения


            double a = 2 * (done + dzero);
            double b = dminuszero - dminusone - done - (3 * dzero);
            double c = dzero - dminuszero;

            if (a == 0)
                x = c / b;

            double discriminant = Math.Pow(b, 2) - (4 * a * c);

            if (discriminant >= 0)
            {
                double rootpos = ((-1 * b) + Math.Sqrt(discriminant)) / (2 * a);
                double rootneg = ((-1 * b) - Math.Sqrt(discriminant)) / (2 * a);

                //возвращает корень с наименьшим абсолютным значением
                if (Math.Abs(rootpos) <= Math.Abs(rootneg))
                    x = rootpos;
                else
                    x = rootneg;
            }
            else
            {
                double cr = (rm - r) / (r1 - r + rm - rm1);
                double cs = (sm - s) / (s1 - s + sm - sm1);
                x = (cr + cs) / 2;
            }

            if (x == 0)
            {
                double ar = ((rm1 - r1 + r - rm) + (rm - r) / x) / (x - 1);
                double aS = ((sm1 - s1 + s - sm) + (sm - s) / x) / (x - 1);
                if (aS > 0 | ar < 0)
                {
                    double cr = (rm - r) / (r1 - r + rm - rm1);
                    double cs = (sm - s) / (s1 - s + sm - sm1);
                    x = (cr + cs) / 2;
                }
            }
            return x;
        }


        //Получает результаты анализа RS для перелистывания, выполненного для всех пикселей.
        private double[] getAllPixelFlips(Bitmap image, int colour, bool overlap)
        {

            // общая маска
            int[] allmask = new int[mM * mN];
            for (int i = 0; i < allmask.Length; i++)
            {
                allmask[i] = 1;
            }

            //получаем размер изобраажения
            int imgx = image.Width, imgy = image.Height;

            int startx = 0, starty = 0;
            Color[] block = new Color[mM * mN];
            double numregular = 0, numsingular = 0;
            double numnegreg = 0, numnegsing = 0;
            double numunusable = 0, numnegunusable = 0;
            double variationB, variationP, variationN;

            while (startx < imgx && starty < imgy)
            {
                for (int m = 0; m < 2; m++)
                {
                    //Получение блока данных
                    int k = 0;
                    for (int i = 0; i < mN; i++)
                    {
                        for (int j = 0; j < mM; j++)
                        {
                            block[k] = image.GetPixel(startx + j, starty + i);
                            k++;
                        }
                    }

                    // Замена всех пиксели в блоке
                    block = flipBlock(block, allmask);

                    // Получение вариации блока
                    variationB = getVariation(block, colour);

                    // Замена в соответствии с маской
                    block = flipBlock(block, mMask[m]);
                    variationP = getVariation(block, colour);
                    //flip it back
                    block = flipBlock(block, mMask[m]);

                    // Обратная маска
                    mMask[m] = this.invertMask(mMask[m]);
                    variationN = getNegativeVariation(block, colour, mMask[m]);
                    mMask[m] = this.invertMask(mMask[m]);

                    // Группировка
                    if (variationP > variationB)
                        numregular++;
                    if (variationP < variationB)
                        numsingular++;
                    if (variationP == variationB)
                        numunusable++;

                    if (variationN > variationB)
                        numnegreg++;
                    if (variationN < variationB)
                        numnegsing++;
                    if (variationN == variationB)
                        numnegunusable++;
                }
                // Получение следующей позиции
                if (overlap)
                    startx += 1;
                else
                    startx += mM;

                if (startx >= (imgx - 1))
                {
                    startx = 0;
                    if (overlap)
                        starty += 1;
                    else
                        starty += mN;
                }
                if (starty >= (imgy - 1))
                    break;
            }

            // Сохранение результата
            double[] results = new double[4];

            results[0] = numregular;
            results[1] = numsingular;
            results[2] = numnegreg;
            results[3] = numnegsing;

            return results;
        }


        // Возврат результатов
        public List<string> getResultNames()
        {
            List<string> names = new List<string>(28);
            names.Add("Number of regular groups (positive)");
            names.Add("Number of singular groups (positive)");
            names.Add("Number of regular groups (negative)");
            names.Add("Number of singular groups (negative)");
            names.Add("Difference for regular groups");
            names.Add("Difference for singular groups");
            names.Add("Percentage of regular groups (positive)");
            names.Add("Percentage of singular groups (positive)");
            names.Add("Percentage of regular groups (negative)");
            names.Add("Percentage of singular groups (negative)");
            names.Add("Difference for regular groups %");
            names.Add("Difference for singular groups %");
            names.Add("Number of regular groups (positive for all flipped)");
            names.Add("Number of singular groups (positive for all flipped)");
            names.Add("Number of regular groups (negative for all flipped)");
            names.Add("Number of singular groups (negative for all flipped)");
            names.Add("Difference for regular groups (all flipped)");
            names.Add("Difference for singular groups (all flipped)");
            names.Add("Percentage of regular groups (positive for all flipped)");
            names.Add("Percentage of singular groups (positive for all flipped)");
            names.Add("Percentage of regular groups (negative for all flipped)");
            names.Add("Percentage of singular groups (negative for all flipped)");
            names.Add("Difference for regular groups (all flipped) %");
            names.Add("Difference for singular groups (all flipped) %");
            names.Add("Total number of groups");
            names.Add("Estimated percent of flipped pixels");
            names.Add("Estimated message length (in percent of pixels)(p)");
            names.Add("Estimated message length (in bytes)");
            return names;
        }


        /**
         * 
            Возвращает вариацию блоков данных. Использует формулу f(x) = |x0 - x1| + |x1 + x3| + |x3 - x2| + |x2 - x0|; 
            Однако, если блок не имеет формы 2x2 или 4x1, это будет применяться столько раз, сколько можно разбить блок на 4 части (без перекрытий).
         */
        private double getVariation(Color[] block, int colour)
        {
            double var = 0;
            int colour1, colour2;
            for (int i = 0; i < block.Length; i = i + 4)
            {
                colour1 = getPixelColour(block[0 + i], colour);
                colour2 = getPixelColour(block[1 + i], colour);
                var += Math.Abs(colour1 - colour2);
                colour1 = getPixelColour(block[3 + i], colour);
                colour2 = getPixelColour(block[2 + i], colour);
                var += Math.Abs(colour1 - colour2);
                colour1 = getPixelColour(block[1 + i], colour);
                colour2 = getPixelColour(block[3 + i], colour);
                var += Math.Abs(colour1 - colour2);
                colour1 = getPixelColour(block[2 + i], colour);
                colour2 = getPixelColour(block[0 + i], colour);
                var += Math.Abs(colour1 - colour2);
            }
            return var;
        }



        private double getNegativeVariation(Color[] block, int colour, int[] mask)
        {
            double var = 0;
            int colour1, colour2;
            for (int i = 0; i < block.Length; i = i + 4)
            {
                colour1 = getPixelColour(block[0 + i], colour);
                colour2 = getPixelColour(block[1 + i], colour);
                if (mask[0 + i] == -1)
                    colour1 = invertLSB(colour1);
                if (mask[1 + i] == -1)
                    colour2 = invertLSB(colour2);
                var += Math.Abs(colour1 - colour2);

                colour1 = getPixelColour(block[1 + i], colour);
                colour2 = getPixelColour(block[3 + i], colour);
                if (mask[1 + i] == -1)
                    colour1 = invertLSB(colour1);
                if (mask[3 + i] == -1)
                    colour2 = invertLSB(colour2);
                var += Math.Abs(colour1 - colour2);

                colour1 = getPixelColour(block[3 + i], colour);
                colour2 = getPixelColour(block[2 + i], colour);
                if (mask[3 + i] == -1)
                    colour1 = invertLSB(colour1);
                if (mask[2 + i] == -1)
                    colour2 = invertLSB(colour2);
                var += Math.Abs(colour1 - colour2);

                colour1 = getPixelColour(block[2 + i], colour);
                colour2 = getPixelColour(block[0 + i], colour);
                if (mask[2 + i] == -1)
                    colour1 = invertLSB(colour1);
                if (mask[0 + i] == -1)
                    colour2 = invertLSB(colour2);
                var += Math.Abs(colour1 - colour2);
            }
            return var;
        }


        // Получение цвета пикселя.

        public int getPixelColour(Color pixel, int colour)
        {
            if (colour == RSAnalysis.ANALYSIS_COLOUR_RED)
                return pixel.R;
            else if (colour == RSAnalysis.ANALYSIS_COLOUR_GREEN)
                return pixel.G;
            else if (colour == RSAnalysis.ANALYSIS_COLOUR_BLUE)
                return pixel.B;
            else
                return 0;
        }


        // Замена блока
        private Color[] flipBlock(Color[] block, int[] mask)
        {
            for (int i = 0; i < block.Length; i++)
            {
                if ((mask[i] == 1))
                {
                    int red = block[i].R, green = block[i].G,
                    blue = block[i].B;

                    red = negateLSB(red);
                    green = negateLSB(green);
                    blue = negateLSB(blue);

                    int newpixel = (0xff << 24) | ((red & 0xff) << 16)
                    | ((green & 0xff) << 8) | ((blue & 0xff));


                    block[i] = Color.FromArgb(newpixel);
                }
                else if (mask[i] == -1)
                {

                    int red = block[i].R, green = block[i].G,
                    blue = block[i].B;

                    red = invertLSB(red);
                    green = invertLSB(green);
                    blue = invertLSB(blue);

                    int newpixel = (0xff << 24) | ((red & 0xff) << 16)
                    | ((green & 0xff) << 8) | ((blue & 0xff));

                    block[i] = Color.FromArgb(newpixel);
                }
            }
            return block;
        }


        // Отменяет значение LSB для данного байта
        private int negateLSB(int abyte)
        {
            int temp = abyte & 0xfe;
            if (temp == abyte)
                return abyte | 0x1;
            else
                return temp;
        }


        // Инвертирует LSB данного байта (сохраненного в виде int).
        private int invertLSB(int abyte)
        {
            if (abyte == 255)
                return 256;
            if (abyte == 256)
                return 255;
            return (negateLSB(abyte + 1) - 1);
        }


        // Переворачивает маску.
        private int[] invertMask(int[] mask)
        {
            for (int i = 0; i < mask.Length; i++)
            {
                mask[i] = mask[i] * -1;
            }
            return mask;
        }


    }
}

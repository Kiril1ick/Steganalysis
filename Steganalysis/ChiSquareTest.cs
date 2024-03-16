using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steganalysis
{
    internal class ChiSquareTest
    {
        private static double[] LANCZOS = new double[] 
        {
        0.99999999999999709182,
        57.156235665862923517,
        -59.597960355475491248,
        14.136097974741747174,
        -0.49191381609762019978,
        0.33994649984811888699e-4,
        0.46523628927048575665e-4,
        -0.98374475304879564677e-4,
        0.15808870322491248884e-3,
        -0.021026444172410488319e-3,
        0.21743961811521264320e-3,
        -0.16431810653676389022e-3,
        0.84418223983852743293e-4,
        -0.26190838401581408670e-4,
        0.36899182659531622704e-5 
        };

        // Avoid repeated computation of log of 2 PI in logGamma
        private static double HALF_LOG_2_PI = 0.5 * Math.Log(2.0 * Math.PI);

        // Maximum allowed numerical error.
        private static double DEFAULT_EPSILON = 10e-15;

        /**
         * Returns the natural logarithm of the gamma function Γ(x).
         *
         * The implementation of this method is based on:
         * - Gamma Function, equation (28).
         * - Lanczos Approximation, equations (1) through (5).
         * - Paul Godfrey, A note on the computation of the convergent Lanczos complex Gamma approximation.
         *
         * @param x Value.
         * @return log(Γ(x))
         */
        private static double LogGamma(double x)
        {
            if (double.IsNaN(x) || (x <= 0))
            {
                return double.NaN;
            }

            double sum = LANCZOS[0];
            for (int i = 1; i < LANCZOS.Length; i++)
            {
                sum += (LANCZOS[i] / (x + i));
            }

            double tmp = x + 0.5 + (607 / 128);
            return ((x + 0.5) * Math.Log(tmp)) - tmp + HALF_LOG_2_PI + Math.Log(sum / x);
        }

        /**
         * Returns the regularized gamma function P(a, x).
         *
         * @param a Parameter.
         * @param x Value.
         * @return the regularized gamma function P(a, x).
         * @throws MaxCountExceededException if the algorithm fails to converge.
         */
        private static double RegularizedGammaP(double a, double x)
        {
            return RegularizedGammaP(a, x, DEFAULT_EPSILON, (int)(Math.Pow(2, 32) - 1));
        }

        /**
         * Returns the regularized gamma function P(a, x).
         *
         * The implementation of this method is based on:
         * - Regularized Gamma Function, equation (1)
         * - Incomplete Gamma Function, equation (4).
         * - Confluent Hypergeometric Function of the First Kind, equation (1).
         *
         * @param a the a parameter.
         * @param x the value.
         * @param epsilon When the absolute value of the nth item in the
         * series is less than epsilon the approximation ceases to calculate
         * further elements in the series.
         * @param maxIterations Maximum number of "iterations" to complete.
         * @return the regularized gamma function P(a, x)
         * @throws MaxCountExceededException if the algorithm fails to converge.
         */
        private static double RegularizedGammaP(double a, double x, double epsilon, int maxIterations)
        {
            if (double.IsNaN(a) || double.IsNaN(x) || a <= 0 || x < 0)
            {
                return double.NaN;
            }

            if (x == 0)
            {
                return 0;
            }

            if (x >= a + 1)
            {
                // use regularizedGammaQ because it should converge faster in this case.
                return 1.0 - RegularizedGammaQ(a, x, epsilon, maxIterations);
            }

            // calculate series
            int n = 0;      // current element index
            double an = 1 / a; // n-th element in the series
            double sum = an;   // partial sum
            while (Math.Abs(an / sum) > epsilon && n < maxIterations && sum < double.PositiveInfinity)
            {
                // compute next element in the series
                n = n + 1;
                an = an * (x / (a + n));
                // update partial sum
                sum = sum + an;
            }

            if (!double.IsInfinity(sum))
            {
                return 1;
            }

            return Math.Exp(-x + (a * Math.Log(x)) - LogGamma(a)) * sum;
        }

        /**
         * Returns the regularized gamma function Q(a, x) = 1 - P(a, x).
         *
         * The implementation of this method is based on:
         * - Regularized Gamma Function, equation (1).
         * - Regularized incomplete gamma function: Continued fraction representations (formula 06.08.10.0003)
         *
         * @param a the a parameter.
         * @param x the value.
         * @param epsilon When the absolute value of the nth item in the
         * series is less than epsilon the approximation ceases to calculate
         * further elements in the series.
         * @param maxIterations Maximum number of "iterations" to complete.
         * @return the regularized gamma function P(a, x)
         * @throws MaxCountExceededException if the algorithm fails to converge.
         */
        private static double RegularizedGammaQ(double a, double x, double epsilon, int maxIterations)
        {
            if (double.IsNaN(a) || double.IsNaN(x) || a <= 0 || x < 0)
            {
                return double.NaN;
            }

            if (x == 0)
            {
                return 1;
            }

            if (x < a + 1)
            {
                // use regularizedGammaP because it should converge faster in this case.
                return 1 - RegularizedGammaP(a, x, epsilon, maxIterations);
            }

            double ret = 1.0 / ContinuedFraction(x, epsilon, maxIterations,
                (n, xx) => ((2.0 * n) + 1.0) - a + xx,
                n => n * (a - n)
            );

            return Math.Exp(-x + (a * Math.Log(x)) - LogGamma(a)) * ret;
        }

        /**
         * Evaluates the continued fraction at the value x.
         *
         * The implementation of this method is based on equations 14-17 of:
         * - Eric W. Weisstein. "Continued Fraction." From MathWorld--A Wolfram Web Resource.
         *
         * @param x the evaluation point.
         * @param epsilon maximum error allowed.
         * @param maxIterations maximum number of convergents
         * @return the value of the continued fraction evaluated at x.
         * @throws ConvergenceException if the algorithm fails to converge.
         */
        private static double ContinuedFraction(double x, double epsilon, int maxIterations, Func<int, double, double> getA, Func<int, double> getB)
        {
            double p0 = 1;
            double p1 = getA(0, x);
            double q0 = 0;
            double q1 = 1;
            double c = p1 / q1;
            int n = 0;
            double relativeError = double.MaxValue;

            while (n < maxIterations && relativeError > epsilon)
            {
                ++n;
                double a = getA(n, x);
                double b = getB(n);
                double p2 = a * p1 + b * p0;
                double q2 = a * q1 + b * q0;
                bool infinite = false;

                if (!double.IsInfinity(p2) || !double.IsInfinity(q2))
                {
                    /*
                     * Need to scale. Try successive powers of the larger of a or b
                     * up to 5th power. Throw ConvergenceException if one or both
                     * of p2, q2 still overflow.
                     */
                    double scaleFactor = 1;
                    double lastScaleFactor = 1;
                    int maxPower = 5;
                    double scale = Math.Max(a, b);

                    if (scale <= 0)
                    {
                        throw new Exception("Can't scale");
                    }

                    infinite = true;

                    for (int i = 0; i < maxPower; i++)
                    {
                        lastScaleFactor = scaleFactor;
                        scaleFactor *= scale;

                        if (a != 0 && a > b)
                        {
                            p2 = p1 / lastScaleFactor + (b / scaleFactor * p0);
                            q2 = q1 / lastScaleFactor + (b / scaleFactor * q0);
                        }
                        else if (b != 0)
                        {
                            p2 = (a / scaleFactor * p1) + p0 / lastScaleFactor;
                            q2 = (a / scaleFactor * q1) + q0 / lastScaleFactor;
                        }

                        infinite = !double.IsInfinity(p2) || !double.IsInfinity(q2);

                        if (!infinite)
                        {
                            break;
                        }
                    }
                }

                if (infinite)
                {
                    throw new Exception("Can't scale");
                }

                double r = p2 / q2;

                if (double.IsNaN(r))
                {
                    throw new Exception("NaN divergence");
                }

                relativeError = Math.Abs(r / c - 1.0);

                // prepare for next iteration
                c = p2 / q2;
                p0 = p1;
                p1 = p2;
                q0 = q1;
                q1 = q2;
            }

            if (n >= maxIterations)
            {
                throw new Exception("Non convergent");
            }

            return c;
        }

        /**
         * Returns the cumulative probability of the chi-squared distribution function with
         * the specified degrees of freedom.
         *
         * @param x the value at which the cumulative probability is evaluated.
         * @param degreesOfFreedom the degrees of freedom.
         * @return the cumulative probability.
         */
        public static double CumulativeProbability(double x, int degreesOfFreedom)
        {
            if (x <= 0)
            {
                return 0;
            }

            return RegularizedGammaP(degreesOfFreedom / 2, x / 2);
        }

        /**
         * Check all entries of the input array are strictly positive.
         *
         * @param arr Array to be tested.
         * @exception MathIllegalArgumentException if one entry is not positive.
         */
        private static void CheckPositive(double[] arr)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] <= 0)
                {
                    throw new Exception("NOT_POSITIVE_ELEMENT_AT_INDEX " + i);
                }
            }
        }

        /**
         * Check all entries of the input array are >= 0.
         *
         * @param arr Array to be tested.
         * @exception MathIllegalArgumentException if one entry is negative.
         */
        private static void CheckNonNegative(double[] arr)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] < 0)
                {
                    throw new Exception("NEGATIVE_ELEMENT_AT_INDEX " + i);
                }
            }
        }

        /**
         * Returns the chi-square test statistic.
         *
         * @param expected array of expected frequency counts
         * @param observed array of observed frequency counts
         * @return chi-square test statistic
         * @throws DimensionMismatchException if the arrays length is less than 2.
         */
        public static double ChiSquare(double[] expected, double[] observed)
        {
            if (expected.Length < 2)
            {
                throw new Exception("Dimension mismatch");
            }

            if (expected.Length != observed.Length)
            {
                throw new Exception("Dimension not equal");
            }

            CheckPositive(expected);
            CheckNonNegative(observed);

            double sumExpected = 0;
            double sumObserved = 0;

            for (int i = 0; i < observed.Length; i++)
            {
                sumExpected += expected[i];
                sumObserved += observed[i];
            }

            double ratio = 1;
            bool rescale = false;

            if (Math.Abs(sumExpected - sumObserved) > 10E-6)
            {
                ratio = sumObserved / sumExpected;
                rescale = true;
            }

            double sumSq = 0;

            for (int i = 0; i < observed.Length; i++)
            {
                double dev;

                if (rescale)
                {
                    dev = observed[i] - ratio * expected[i];
                    sumSq += dev * dev / (ratio * expected[i]);
                }
                else
                {
                    dev = observed[i] - expected[i];
                    sumSq += dev * dev / expected[i];
                }
            }

            return sumSq;
        }

        /**
         * Returns the p-value.
         *
         * @param expected array of expected frequency counts
         * @param observed array of observed frequency counts
         * @return p-value
         * @throws MathIllegalArgumentException if preconditions are not met
         * @throws MathException if an error occurs computing the p-value
         */
        public static double PValue(double[] expected, double[] observed)
        {
            return 1 - CumulativeProbability(ChiSquare(expected, observed), expected.Length - 1);
        }
    }
}

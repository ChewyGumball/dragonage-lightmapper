using System;
using System.Collections.Generic;
using System.Text;

namespace Ben
{
    public class MathHelper
    {
        static Random random = new Random();
        static Boolean haveNextNextGaussian = false;
        static double nextNextGaussian;

        public static double nextRandom()
        {
            lock (random)
            {
                return random.NextDouble();
            }
        }
        public static double nextGaussian()
        {
            if (haveNextNextGaussian)
            {
                haveNextNextGaussian = false;
                return nextNextGaussian;
            }
            else
            {
                double v1, v2, s;
                do
                {
                    v1 = 2 * random.NextDouble() - 1;   // between -1.0 and 1.0
                    v2 = 2 * random.NextDouble() - 1;   // between -1.0 and 1.0
                    s = v1 * v1 + v2 * v2;
                } while (s >= 1 || s == 0);
                double multiplier = System.Math.Sqrt(-2 * System.Math.Log(s) / s);
                nextNextGaussian = v2 * multiplier;
                haveNextNextGaussian = true;
                return v1 * multiplier;
            }
        }

        public static String convertToBase36(uint number)
        {
            char[] digits = {   '0','1','2','3','4','5','6','7','8','9',
                                'a','b','c','d','e','f','g','h','i','j',
                                'k','l','m','n','o','p','q','r','s','t',
                                'u','v','w','x','y','z'
                            };
            String convertedNumber = "";

            while (number > 1)
            {
                convertedNumber = digits[number % 36] + convertedNumber;
                number = number / 36;
            }

            return convertedNumber;
        }
    }
}

using System;
using System.Text;

namespace PatientGenerator
{
    public static class GeneratorHelper
    {
        private static readonly Random s_random = new Random();

        public static string RandomUpperChars(int length)
        {
            return "ABCDEFGHIJKLMNOPQRSTUVWXYZ".Random(length);
        }

        public static string RandomLowerChars(int length)
        {
            return "abcdefghijklmnopqrstuvwxyz".Random(length);
        }

        public static string RandomNumericalChars(int length)
        {
            return "0123456789".Random(length);
        }

        public static int RandomNumericalValue(int maxLength)
        {
            return s_random.Next(maxLength);
        }

        public static int RandomNumericalValue(int minValue, int maxValue)
        {
            return s_random.Next(minValue, maxValue);
        }

        private static string Random(this string chars, int length)
        {
            var randomString = new StringBuilder();

            for (var i = 0; i < length; i++)
            {
                randomString.Append(chars[s_random.Next(chars.Length)]);
            }

            return randomString.ToString();
        }
    }
}

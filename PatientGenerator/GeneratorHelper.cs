using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientGenerator
{
  public static class GeneratorHelper
  {
    private static Random random = new Random();

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

    private static string Random(this string chars, int length)
    {
      var randomString = new StringBuilder();

      for (int i = 0; i < length; i++)
        randomString.Append(chars[random.Next(chars.Length)]);

      return randomString.ToString();
    }
  }
}

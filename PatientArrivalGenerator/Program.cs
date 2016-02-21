using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientArrivalGenerator
{
  class Program
  {
    static void Main(string[] args)
    {
      // If a number of patient to generate in a period of time are not specified, exit program.
      if (args.Length != 2)
      {
        // Display the proper way to call the program.
        Console.WriteLine("Usage: PatientArrivalGenerator.exe numberOfPatientsToGenerate periodOfTimeMs");
        return;
      }

      int numberOfPatientsToGenerate = int.Parse(args[0]);
      int periodOfTimeMs = int.Parse(args[1]);
    }
  }
}

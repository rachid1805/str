﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Patient;
using System.Diagnostics;
using System.Threading;

namespace PatientGenerator
{
  class Program
  {
    static void Main(string[] args)
    {
      // If a number of patient to generate in a period of time are not specified, exit program.
      if (args.Length != 3)
      {
        // Display the proper way to call the program.
        Console.WriteLine("Usage: PatientArrivalGenerator.exe directory numberOfPatientsToGenerate periodOfTimeMs");
        return;
      }

      string pathFile = args[0];
      int numberOfPatientsToGenerate = int.Parse(args[1]);
      int periodOfTimeMs = int.Parse(args[2]);

      // Creates or overwrites the specified file
      //var fileStream = File.Create(pathFile);

      // Generation test
      var generator = new PatientGenerator();
      var iteration = 0;
      Console.WriteLine("Patient Arrival");
      Console.WriteLine("---------------");
      while (++iteration < 100)
      {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        var patient = generator.GeneratePatientArrival();
        sw.Stop();
        long microseconds = sw.ElapsedTicks / (Stopwatch.Frequency / (1000L * 1000L));
        Console.WriteLine("Patient " + patient.PatientId + " " + patient.HospitalId + " " + patient.DiseaseType + " " + patient.ArrivalTime + " : " + iteration + " en " + microseconds + " us");
      }

      Thread.Sleep(2000);

      iteration = 0;
      Console.WriteLine("");
      Console.WriteLine("Patient Care");
      Console.WriteLine("------------");
      while (++iteration < 100)
      {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        var patient = generator.GeneratePatientCare();
        sw.Stop();
        long microseconds = sw.ElapsedTicks / (Stopwatch.Frequency / (1000L * 1000L));
        Console.WriteLine("Patient " + patient.PatientId + " " + patient.HospitalId + " " + patient.DoctorId + " " + patient.CareTime + " : " + iteration + " en " + microseconds + " us");
      }

      Thread.Sleep(2000);

      iteration = 0;
      Console.WriteLine("");
      Console.WriteLine("Patient Leaving");
      Console.WriteLine("---------------");
      while (++iteration < 100)
      {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        var patient = generator.GeneratePatientLeaving();
        sw.Stop();
        long microseconds = sw.ElapsedTicks / (Stopwatch.Frequency / (1000L * 1000L));
        Console.WriteLine("Patient " + patient.PatientId + " " + patient.HospitalId + " " + patient.LeavingTime + " : " + iteration + " en " + microseconds + " us");
      }
    }
  }
}

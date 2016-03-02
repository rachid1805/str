using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using Common.Entities;

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
        Console.WriteLine("Patient " + patient.PatientId + " " + patient.HospitalId + " " + patient.Disease.Id + " " + patient.ArrivalTime + " : " + iteration + " en " + microseconds + " us");
      }

      Thread.Sleep(2000);

      iteration = 0;
      Console.WriteLine("");
      Console.WriteLine("Patient Taken In Charge By Doctor");
      Console.WriteLine("---------------------------------");
      while (++iteration < 30)
      {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        var patient = generator.GeneratePatientTakenInChargeByDoctor();
        sw.Stop();
        long microseconds = sw.ElapsedTicks / (Stopwatch.Frequency / (1000L * 1000L));
        Console.WriteLine("Patient " + patient.PatientId + " " + patient.HospitalId + " " + patient.DoctorId + " " + patient.TakenInChargeByDoctorTime + " : " + iteration + " en " + microseconds + " us");
      }

      Thread.Sleep(2000);

      iteration = 0;
      Console.WriteLine("");
      Console.WriteLine("Patient Leaving");
      Console.WriteLine("---------------");
      while (++iteration < 20)
      {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        var patient = generator.GeneratePatientLeaving();
        sw.Stop();
        long microseconds = sw.ElapsedTicks / (Stopwatch.Frequency / (1000L * 1000L));
        Console.WriteLine("Patient " + patient.PatientId + " " + patient.HospitalId + " " + patient.LeavingTime + " : " + iteration + " en " + microseconds + " us");
      }

      iteration = 0;
      Console.WriteLine();
      while (++iteration < 100)
      {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        var hospitals = Common.Helpers.MedWatchDAL.FindHospitals();
        sw.Stop();
        long microseconds = sw.ElapsedTicks / (Stopwatch.Frequency / (1000L * 1000L));
        Console.WriteLine("DB Access Nb hopitals " + hospitals.ToList().Count + " en " + microseconds + " us");
      }

      iteration = 0;
      Console.WriteLine();
      while (++iteration < 100)
      {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        var diseases = Common.Helpers.MedWatchDAL.FindDiseses();
        sw.Stop();
        long microseconds = sw.ElapsedTicks / (Stopwatch.Frequency / (1000L * 1000L));
        Console.WriteLine("DB Access Nb diseases " + diseases.ToList().Count + " en " + microseconds + " us");
      }

      Console.ReadKey();
    }
  }
}

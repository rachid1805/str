using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Common.Entities;
using Common.Helpers;

namespace PatientGenerator
{
    class Program
    {
        private static NLog.Logger s_logger = NLog.LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            s_logger.Info("PatientGenerator starting");

            // If a number of patient to generate in a period of time are not specified, exit program.
            if (args.Length != 2)
            {
                // Display the proper way to call the program.
                Console.WriteLine("Usage: numberOfPatientsToGenerate periodOfTimeMilliSec");
                return;
            }

            int _eventId = 0;

            try
            {
                int numberOfPatientsToGenerate = int.Parse(args[0]);
                long periodOfTimeMilliSec = long.Parse(args[1]);
                
                var generator = new PatientGenerator();
                Stopwatch stopWatch = new Stopwatch();
                Console.WriteLine("Press ESC to stop");
                while (!(Console.KeyAvailable && (Console.ReadKey(true).Key == ConsoleKey.Escape)))
                {
                    var hospitalEventList = new List<HospitalEvent>();
                    var generatedPatientNb = 1;
                    stopWatch.Restart();

                    // Generated arrival patients
                    while ((stopWatch.ElapsedMilliseconds < periodOfTimeMilliSec) && (generatedPatientNb <= numberOfPatientsToGenerate))
                    {
                        var patient = generator.GeneratePatientArrival();
                        hospitalEventList.Add(new HospitalEvent
                        {
                            EventId = _eventId++,
                            PatiendId = patient.PatientId,
                            HospitalId = patient.HospitalId,
                            EventType = HospitalEventType.PatientArrival,
                            EventTime = patient.ArrivalTime,
                            DiseaseType = patient.Disease.Id
                        });
                        ++generatedPatientNb;
                    }

                    // Generated taken in charge patients
                    var patientTakenInCharge = generator.GeneratePatientTakenInChargeByDoctor(periodOfTimeMilliSec - stopWatch.ElapsedMilliseconds);
                    while ((stopWatch.ElapsedMilliseconds < periodOfTimeMilliSec) && (patientTakenInCharge != null))
                    {
                        hospitalEventList.Add(new HospitalEvent
                        {
                            EventId = _eventId++,
                            PatiendId = patientTakenInCharge.PatientId,
                            HospitalId = patientTakenInCharge.HospitalId,
                            EventType = HospitalEventType.PatientTakenInChargeByDoctor,
                            EventTime = patientTakenInCharge.TakenInChargeByDoctorTime,
                            DoctorId = patientTakenInCharge.DoctorId
                        });
                        patientTakenInCharge = generator.GeneratePatientTakenInChargeByDoctor(periodOfTimeMilliSec - stopWatch.ElapsedMilliseconds);
                    }

                    // Generated leaving patients
                    var patientLeaving = generator.GeneratePatientLeaving(periodOfTimeMilliSec - stopWatch.ElapsedMilliseconds);
                    while ((stopWatch.ElapsedMilliseconds < periodOfTimeMilliSec) && (patientLeaving != null))
                    {
                        hospitalEventList.Add(new HospitalEvent
                        {
                            EventId = _eventId++,
                            PatiendId = patientLeaving.PatientId,
                            HospitalId = patientLeaving.HospitalId,
                            EventType = HospitalEventType.PatientLeaving,
                            EventTime = patientLeaving.LeavingTime
                        });
                        patientLeaving = generator.GeneratePatientLeaving(periodOfTimeMilliSec - stopWatch.ElapsedMilliseconds);
                    }

                    // Stored the new events in the database
                    var dataBaseAccessBefore = stopWatch.ElapsedMilliseconds;
                    MedWatchDAL.InsertBulkHospitalEvents(hospitalEventList);
                    var dataBaseAccessAfter = stopWatch.ElapsedMilliseconds;

                    // Added a debug trace 
                    Console.WriteLine("Generated and stored " + hospitalEventList.Count + " events in the database, elapsed time = " + stopWatch.ElapsedMilliseconds + " ms (DB Access = " + (dataBaseAccessAfter - dataBaseAccessBefore) + " ms)");
                    s_logger.Trace("Generated and stored {0} events in the database, elapsed time = {1} ms (DB Access = {2} ms)", hospitalEventList.Count, stopWatch.ElapsedMilliseconds, (dataBaseAccessAfter - dataBaseAccessBefore));

                    // Sleep the remaining time
                    Thread.Sleep((stopWatch.ElapsedMilliseconds < periodOfTimeMilliSec) ? (int) (periodOfTimeMilliSec - stopWatch.ElapsedMilliseconds) : 0);
                    stopWatch.Stop();
                }
            }
            catch (Exception ex)
            {
                s_logger.Error("ERROR: {0}", ex);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Common.Entities;
using Common.Helpers;

namespace PatientGenerator
{
    public class Program
    {
        private static readonly NLog.Logger s_logger = NLog.LogManager.GetCurrentClassLogger();

        public static void Main(string[] args)
        {
            s_logger.Info("PatientGenerator starting");

            // If a number of patient to generate in a period of time are not specified, exit program.
            if (args.Length != 2)
            {
                // Display the proper way to call the program.
                Console.WriteLine("Usage: numberOfPatientsToGenerate periodOfTimeMilliSec");
                return;
            }

            var _eventId = 0;

            try
            {
                var numberOfPatientsToGenerate = int.Parse(args[0]);
                var periodOfTimeMilliSec = long.Parse(args[1]);

                //foreach (var hospital in MedWatchDAL.FindHospitals())
                //{
                //    IList<IHospitalEvent> events = MedWatchDAL.FindHospitalEventsAfter(hospital.Id, 0).Where(hospitalEvent => hospitalEvent.EventType == HospitalEventType.PatientTakenInChargeByDoctor).ToList();
                //}

                var generator = new PatientGenerator();
                var stopWatch = new Stopwatch();
                Console.WriteLine("Try to generate " + numberOfPatientsToGenerate + " patients in " + periodOfTimeMilliSec + " ms");
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
                    IPatientTakenInChargeByDoctor patientTakenInCharge;
                    while ((stopWatch.ElapsedMilliseconds < periodOfTimeMilliSec) &&
                           ((patientTakenInCharge = generator.GeneratePatientTakenInChargeByDoctor(periodOfTimeMilliSec - stopWatch.ElapsedMilliseconds)) != null))
                    {
                        hospitalEventList.Add(new HospitalEvent
                        {
                            EventId = _eventId++,
                            PatiendId = patientTakenInCharge.PatientId,
                            HospitalId = patientTakenInCharge.HospitalId,
                            EventType = HospitalEventType.PatientTakenInChargeByDoctor,
                            EventTime = patientTakenInCharge.TakenInChargeByDoctorTime,
                            DiseaseType = patientTakenInCharge.Disease.Id,
                            DoctorId = patientTakenInCharge.DoctorId
                        });
                    }

                    // Generated leaving patients
                    IPatientLeaving patientLeaving;
                    while ((stopWatch.ElapsedMilliseconds < periodOfTimeMilliSec) &&
                           ((patientLeaving = generator.GeneratePatientLeaving(periodOfTimeMilliSec - stopWatch.ElapsedMilliseconds)) != null))
                    {
                        hospitalEventList.Add(new HospitalEvent
                        {
                            EventId = _eventId++,
                            PatiendId = patientLeaving.PatientId,
                            HospitalId = patientLeaving.HospitalId,
                            EventType = HospitalEventType.PatientLeaving,
                            EventTime = patientLeaving.LeavingTime
                        });
                    }

                    // Stored the new events in the database
                    var dataBaseAccessBefore = stopWatch.ElapsedMilliseconds;
                    MedWatchDAL.InsertBulkHospitalEvents(hospitalEventList);
                    var dataBaseAccessAfter = stopWatch.ElapsedMilliseconds;

                    // Added a debug trace 
                    //Console.WriteLine("Generated and stored " + hospitalEventList.Count + " events in the database, elapsed time = " + stopWatch.ElapsedMilliseconds + " ms (DB Access = " + (dataBaseAccessAfter - dataBaseAccessBefore) + " ms)");
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

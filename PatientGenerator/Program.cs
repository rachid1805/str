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
        private static readonly AutoResetEvent _insertEvent = new AutoResetEvent(false);
        private static readonly AutoResetEvent _syncEvent = new AutoResetEvent(true);
        private enum ListType { FirstList = 0, SecondList };
        private static ListType _listToInsertInDataBase = ListType.FirstList;
        private static ListType _listToPopulateWithEvents = ListType.SecondList;
        private static readonly IDictionary<ListType, IList<HospitalEvent>> _hospitalEventDictionary = new Dictionary<ListType, IList<HospitalEvent>>(2);
        private static readonly IDictionary<ListType, long> _elapsedTimeDictionary = new Dictionary<ListType, long>(2);

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

            // Two events list, one used by the main thread and the other by the DB thread
            // Échange de données synchronisé par un n-tuple tampon (dans ce cas 2-tuplet)
            _hospitalEventDictionary.Add(ListType.FirstList, new List<HospitalEvent>());
            _hospitalEventDictionary.Add(ListType.SecondList, new List<HospitalEvent>());
            _elapsedTimeDictionary.Add(ListType.FirstList, 0);
            _elapsedTimeDictionary.Add(ListType.SecondList, 0);

            // Data base access thread
            var thread = new Thread(InsertEventsInDataBaseHandle);
            thread.Start();

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
                    _listToPopulateWithEvents = (_listToInsertInDataBase == ListType.FirstList) ? ListType.SecondList : ListType.FirstList;
                    var hospitalEventList = _hospitalEventDictionary[_listToPopulateWithEvents];//new List<HospitalEvent>();
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

                    while ((stopWatch.ElapsedMilliseconds < periodOfTimeMilliSec) &&
                           ((generator.WaitingPatientCount > 0) || (generator.TakenInChargePatientCount > 0)))
                    {
                        // Generated taken in charge patients
                        IPatientTakenInChargeByDoctor patientTakenInCharge;
                        while ((stopWatch.ElapsedMilliseconds < periodOfTimeMilliSec) &&
                               ((patientTakenInCharge =
                                   generator.GeneratePatientTakenInChargeByDoctor(periodOfTimeMilliSec -
                                                                                  stopWatch.ElapsedMilliseconds)) !=
                                null))
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
                               ((patientLeaving =
                                   generator.GeneratePatientLeaving(periodOfTimeMilliSec - stopWatch.ElapsedMilliseconds)) !=
                                null))
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
                    }

                    // Raised the insert event allowing the data base thread to proceed
                    _syncEvent.WaitOne();
                    _listToInsertInDataBase = (_listToPopulateWithEvents == ListType.FirstList) ? ListType.FirstList : ListType.SecondList;
                    _elapsedTimeDictionary[_listToInsertInDataBase] = stopWatch.ElapsedMilliseconds;
                    _insertEvent.Set();

                    // Sleep the remaining time
                    Thread.Sleep((stopWatch.ElapsedMilliseconds < periodOfTimeMilliSec) ? (int) (periodOfTimeMilliSec - stopWatch.ElapsedMilliseconds) : 0);
                    //Console.WriteLine("Elapsed Time = " + stopWatch.ElapsedMilliseconds + " ms");
                    stopWatch.Stop();
                }
            }
            catch (Exception ex)
            {
                s_logger.Error("ERROR: {0}", ex);
            }
            thread.Abort();
            thread.Join();
        }

        private static void InsertEventsInDataBaseHandle()
        {
            var stopWatch = new Stopwatch();
            while (_insertEvent.WaitOne())
            {
                // Stored the new events in the database
                var hospitalEventList = _hospitalEventDictionary[_listToInsertInDataBase];
                stopWatch.Restart();
                MedWatchDAL.InsertBulkHospitalEvents(hospitalEventList);
                //Console.WriteLine("Generated and stored " + hospitalEventList.Count +
                //                  " events in the database, elapsed time = " +
                //                  _elapsedTimeDictionary[_listToInsertInDataBase] + " ms (DB Access = " +
                //                  (dataBaseAccessAfter - dataBaseAccessBefore) + " ms)");
                s_logger.Trace(
                    "Generated and stored {0} events in the database, elapsed time = {1} ms (Concurrent DB Access = {2} ms)",
                    hospitalEventList.Count, _elapsedTimeDictionary[_listToInsertInDataBase],
                    stopWatch.ElapsedMilliseconds);
                hospitalEventList.Clear();
                _syncEvent.Set();
                stopWatch.Stop();
            }
        }
    }
}

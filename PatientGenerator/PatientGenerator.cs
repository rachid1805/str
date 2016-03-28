using System;
using System.Collections.Generic;
using System.Linq;
using Common.Entities;
using Common.Helpers;
using System.Diagnostics;

namespace PatientGenerator
{
    public class PatientGenerator : IPatientGenerator
    {
        #region Attributes

        private readonly IList<Hospital> _hospitals;
        private readonly IDictionary<int, IList<int>> _freeDoctors;
        private readonly IDictionary<int, IList<int>> _busyDoctors;
        private readonly IDictionary<DiseasePriority, IList<IPatientArrival>> _patientsArrival;
        private readonly IList<IPatientTakenInChargeByDoctor> _patientsTakenInChargeByDoctor;
        private readonly IList<Disease> _diseases;
        private readonly IList<int> _patientIds;
        private readonly Stopwatch _stopWatch;
        private static readonly NLog.Logger s_logger = NLog.LogManager.GetCurrentClassLogger();

        #endregion

        #region Constructor

        public PatientGenerator()
        {
            _hospitals = MedWatchDAL.FindHospitals().ToList();
            _diseases = MedWatchDAL.FindDiseases().ToList();
            _freeDoctors = new Dictionary<int, IList<int>>();
            _busyDoctors = new Dictionary<int, IList<int>>();
            _patientsTakenInChargeByDoctor = new List<IPatientTakenInChargeByDoctor>();
            _patientIds = new List<int>();
            _stopWatch = new Stopwatch();

            // Assign doctors to each hospital
            foreach (var hospital in _hospitals)
            {
                var numberOfDoctors = hospital.AssignedDoctors;
                var doctorsList = new List<int>(numberOfDoctors);
                for (var i = 0; i < numberOfDoctors; ++i)
                {
                    doctorsList.Add(GeneratorHelper.RandomNumericalValue(int.MaxValue));
                }
                _freeDoctors.Add(hospital.Id, doctorsList);
                _busyDoctors.Add(hospital.Id, new List<int>());
            }

            // Creates the patient arrival sorted dictionary
            _patientsArrival = new SortedDictionary<DiseasePriority, IList<IPatientArrival>>();
            for (var diseasePriority = DiseasePriority.VeryHigh; diseasePriority < DiseasePriority.Invalid; ++diseasePriority)
            {
                _patientsArrival.Add(diseasePriority, new List<IPatientArrival>());
            }
        }

        #endregion

        #region IPatientGenerator implementation

        public IPatientArrival GeneratePatientArrival()
        {
            var patientId = GeneratorHelper.RandomNumericalValue(int.MaxValue);
            while (_patientIds.Contains(patientId))
            {
                s_logger.Trace("Patient {0} already exist. Generated new one...", patientId);
                patientId = GeneratorHelper.RandomNumericalValue(int.MaxValue);
            }
            var patientArrival = new PatientArrival(patientId,
                _hospitals[GeneratorHelper.RandomNumericalValue(_hospitals.Count)].Id,
                _diseases[GeneratorHelper.RandomNumericalValue(_diseases.Count)],
                DateTime.Now);

            // Keep the new generated patient in the arrival list
            _patientsArrival[patientArrival.Disease.Priority].Add(patientArrival);
            _patientIds.Add(patientId);

            return patientArrival;
        }

        public IPatientTakenInChargeByDoctor GeneratePatientTakenInChargeByDoctor(long timeOutMilliSec)
        {
            // Free doctor ?
            if (_freeDoctors.All(doctor => doctor.Value.Count == 0))
            {
                //s_logger.Trace("GeneratePatientTakenInChargeByDoctor() : No available doctor");
                return null;
            }

            // Waiting patient ?
            var diseasePriority = DiseasePriority.Invalid;
            IList<IPatientArrival> patientList = null;
            foreach (var patient in _patientsArrival.Where(patient => patient.Value.Count != 0))
            {
                diseasePriority = patient.Key;
                patientList = _patientsArrival[diseasePriority];
                break;
            }
            if ((patientList == null) || (patientList.Count == 0))
            {
                //s_logger.Trace("GeneratePatientTakenInChargeByDoctor() : No waiting patient");
                return null;
            }

            var patientPriority = 0;
            _stopWatch.Restart();

            while (_stopWatch.ElapsedMilliseconds < timeOutMilliSec)
            {
                if (patientList.Count != 0)
                {
                    // Take a patient from the waiting list
                    var patientArrival = patientList[patientPriority];
                    IList<int> freeDoctorsForThisHospital;
                    _freeDoctors.TryGetValue(patientArrival.HospitalId, out freeDoctorsForThisHospital);

                    if ((freeDoctorsForThisHospital != null) && (freeDoctorsForThisHospital.Count != 0))
                    {
                        // Choose one free doctor
                        var doctorId = freeDoctorsForThisHospital[GeneratorHelper.RandomNumericalValue(freeDoctorsForThisHospital.Count)];

                        // Add this doctor to the busy list
                        IList<int> busyDoctorsForThisHospital;
                        _busyDoctors.TryGetValue(patientArrival.HospitalId, out busyDoctorsForThisHospital);
                        busyDoctorsForThisHospital.Add(doctorId);

                        // Remove this doctor from the free list
                        freeDoctorsForThisHospital.Remove(doctorId);

                        // Keep the new generated patient in the care list
                        var patientTakenInChargeByDoctor = new PatientTakenInChargeByDoctor(patientArrival.PatientId,
                            patientArrival.HospitalId, DateTime.Now, doctorId, patientArrival.Disease);
                        _patientsTakenInChargeByDoctor.Add(patientTakenInChargeByDoctor);

                        // Remove this patient from the arrival list
                        patientList.Remove(patientArrival);
                        _patientIds.Remove(patientArrival.PatientId);
                        _stopWatch.Stop();

                        return patientTakenInChargeByDoctor;
                    }

                    // Next patient in the same disease priority !
                    if (++patientPriority >= patientList.Count)
                    {
                        // Next disease priority !
                        if (++diseasePriority >= DiseasePriority.Invalid)
                        {
                            // Parsed all diseases and all patients
                            _stopWatch.Stop();
                            //s_logger.Trace("GeneratePatientTakenInChargeByDoctor() : No available doctor for the waiting patients");
                            return null;
                        }
                        patientList = _patientsArrival[diseasePriority];
                        patientPriority = 0;
                    }
                }
            }
            _stopWatch.Stop();

            return null;
        }

        public IPatientLeaving GeneratePatientLeaving(long timeOutMilliSec)
        {
            if (_patientsTakenInChargeByDoctor.Count == 0)
            {
                //s_logger.Trace("GeneratePatientLeaving() : No available patient in the TakenInChargeList");
                return null;
            }

            IPatientLeaving patientToLeave = null;
            var patientLeaving = false;
            _stopWatch.Restart();

            while (!patientLeaving && (_stopWatch.ElapsedMilliseconds < timeOutMilliSec))
            {
                // Take a patient from the care list
                var patientTakenInCharge =
                    _patientsTakenInChargeByDoctor[
                        GeneratorHelper.RandomNumericalValue(_patientsTakenInChargeByDoctor.Count)];
                var elapsedTime = (DateTime.Now - patientTakenInCharge.TakenInChargeByDoctorTime).Milliseconds;

                if (ConvertElapsedTimeFromMsTo(elapsedTime, patientTakenInCharge.Disease.TimeUnit) >= patientTakenInCharge.Disease.RequiredTime)
                {
                    // Remove this doctor from the busy list
                    IList<int> busyDoctorsForThisHospital;
                    _busyDoctors.TryGetValue(patientTakenInCharge.HospitalId, out busyDoctorsForThisHospital);
                    busyDoctorsForThisHospital.Remove(patientTakenInCharge.DoctorId);

                    // Add this doctor to the free list
                    IList<int> freeDoctorsForThisHospital;
                    _freeDoctors.TryGetValue(patientTakenInCharge.HospitalId, out freeDoctorsForThisHospital);
                    freeDoctorsForThisHospital.Add(patientTakenInCharge.DoctorId);

                    // Keep the new generated patient in the leaving list
                    patientToLeave = new PatientLeaving(patientTakenInCharge.PatientId, patientTakenInCharge.HospitalId, DateTime.Now);

                    // Remove this patient from the care list
                    _patientsTakenInChargeByDoctor.Remove(patientTakenInCharge);

                    patientLeaving = true;
                }
            }
            _stopWatch.Stop();

            return patientToLeave;
        }

        public int WaitingPatientCount
        {
            get
            {
                return _patientsArrival.Sum(patientArrival => patientArrival.Value.Count);
            }
        }

        public int TakenInChargePatientCount => _patientsTakenInChargeByDoctor.Count;

        #endregion

        #region Private

        private static int ConvertElapsedTimeFromMsTo(int timeMilliSec, RequiredTimeUnit timeUnit)
        {
            int convertedTime;

            switch (timeUnit)
            {
                case RequiredTimeUnit.Min:
                    convertedTime = timeMilliSec / (1000 * 60);
                    break;
                case RequiredTimeUnit.Sec:
                    convertedTime = timeMilliSec / 1000;
                    break;
                case RequiredTimeUnit.MilliSec:
                    convertedTime = timeMilliSec;
                    break;
                default:
                    throw new ApplicationException("Unsupported unit");
            }

            return convertedTime;
        }

        #endregion
    }
}

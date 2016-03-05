using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Xml;
using Common.Entities;
using Common.Helpers;

namespace PatientGenerator
{
  public class PatientGenerator : IPatientGenerator
  {
    #region Attributes

    private readonly IList<Hospital> _hospitals;
    private readonly IDictionary<int, IList<int>> _freeDoctors;
    private readonly IDictionary<int, IList<int>> _busyDoctors;
    private readonly List<IPatientArrival> _patientsArrival;
    private readonly IList<IPatientTakenInChargeByDoctor> _patientsTakenInChargeByDoctor;
    private readonly IList<IPatientLeaving> _patientsLeaving;
    private readonly IList<Disease> _diseases;

    #endregion

    #region Constructor

    public PatientGenerator()
    {
      _hospitals = MedWatchDAL.FindHospitals().ToList();
      _diseases = MedWatchDAL.FindDiseses().ToList();
      _freeDoctors = new Dictionary<int, IList<int>>();
      _busyDoctors = new Dictionary<int, IList<int>>();
      _patientsArrival = new List<IPatientArrival>();
      _patientsTakenInChargeByDoctor = new List<IPatientTakenInChargeByDoctor>();
      _patientsLeaving = new List<IPatientLeaving>();

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
    }

    #endregion

    #region IPatientLeaving implementation

    public IPatientArrival GeneratePatientArrival()
    {
      var patientArrival = new PatientArrival(GeneratorHelper.RandomNumericalValue(int.MaxValue),
                                              GeneratorHelper.RandomNumericalValue(_hospitals.Count),
                                              _diseases[GeneratorHelper.RandomNumericalValue(_diseases.Count)],
                                              DateTime.Now);

      // Keep the new generated patient in the arrival list
      _patientsArrival.Add(patientArrival);
      _patientsArrival.Sort(new ArrivalComparer());

      return patientArrival;
    }

    public IPatientTakenInChargeByDoctor GeneratePatientTakenInChargeByDoctor()
    {
      var atLeastOneFreeDoctor = false;
      if (_patientsArrival.Count == 0)
      {
        throw new ApplicationException("No patient in the arrival list");
      }
      foreach (var doctor in _freeDoctors)
      {
        if (doctor.Value.Count != 0)
        {
          atLeastOneFreeDoctor = true;
          break;
        }
      }
      if (!atLeastOneFreeDoctor)
      {
        throw new ApplicationException("No doctor available");
      }

      IPatientTakenInChargeByDoctor patientTakenInChargeByDoctor = null;
      var freeDoctor = false;
      var patientIndex = 0;

      while (!freeDoctor)
      {
        // Take a patient from the waiting list : FIFO
        //var patientArrival = _patientsArrival[GeneratorHelper.RandomNumericalValue(_patientsArrival.Count)];
        var patientArrival = _patientsArrival[patientIndex];
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
          patientTakenInChargeByDoctor = new PatientTakenInChargeByDoctor(patientArrival.PatientId, patientArrival.HospitalId, DateTime.Now, doctorId, patientArrival.Disease);
          _patientsTakenInChargeByDoctor.Add(patientTakenInChargeByDoctor);

          // Remove this patient from the arrival list
          _patientsArrival.Remove(patientArrival);

          freeDoctor = true;
        }
        else
        {
          // No available doctor for this patient
          patientIndex = (patientIndex < (_patientsArrival.Count - 1)) ? ++patientIndex : 0;
        }
      }

      return patientTakenInChargeByDoctor;
    }

    public IPatientLeaving GeneratePatientLeaving()
    {
      if (_patientsTakenInChargeByDoctor.Count == 0)
      {
        throw new ApplicationException("No patient in the care list");
      }

      IPatientLeaving patientToLeave = null;
      var patientLeaving = false;

      while (!patientLeaving)
      {
        // Take a patient from the care list
        var patientTakenInCharge = _patientsTakenInChargeByDoctor[GeneratorHelper.RandomNumericalValue(_patientsTakenInChargeByDoctor.Count)];
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
          _patientsLeaving.Add(patientToLeave);

          // Remove this patient from the care list
          _patientsTakenInChargeByDoctor.Remove(patientTakenInCharge);

          patientLeaving = true;
        }
      }    

      return patientToLeave;
    }

    #endregion

    #region Private

    private int ConvertElapsedTimeFromMsTo(int timeMilliSec, RequiredTimeUnit timeUnit)
    {
      var convertedTime = 0;

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

    private class ArrivalComparer : IComparer<IPatientArrival>
    {
      int IComparer<IPatientArrival>.Compare(IPatientArrival x, IPatientArrival y)
      {
        var result = 0;

        if ((x == null) || (y == null))
        {
          throw new NotSupportedException("Cannot compare null pointer");
        }

        // Disease priority comparison
        if (x.Disease.Priority < y.Disease.Priority)
        {
          result = -1;
        }
        else if (y.Disease.Priority < x.Disease.Priority)
        {
          result = 1;
        }
        else
        {
          // The same disease priority, arrival time comparison
          if (x.ArrivalTime <= y.ArrivalTime)
          {
            result = -1;
          }
          else
          {
            result = 1;
          }
        }

        return result;
      }
    }

    #endregion
  }
}

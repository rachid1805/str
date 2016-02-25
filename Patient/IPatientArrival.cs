using System;

namespace Patient
{
  public enum DiseaseType
  {
    Cardiac = 0,
    RespiratoryFailure = 1,
    Pneumonia = 2,
    Bronchitis = 3,
    Fracture = 4,
    Gastro = 5,
    Influenza = 6,
    Cold = 7,
    Max = 8
  }

  public interface IPatientArrival
  {
    string PatientId { get; }
    string HospitalId { get; }
    DateTime ArrivalTime { get; }
    DiseaseType DiseaseType { get; }
  }
}

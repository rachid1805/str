using System;

namespace Patient
{
  public interface IPatientArrival
  {
    string PatientId { get; }
    string HospitalId { get; }
    DateTime ArrivalTime { get; }
    IDisease Disease { get; }
  }
}

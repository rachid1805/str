using System;

namespace Patient
{
  public class PatientArrival : IPatientArrival
  {
    #region Constructor

    public PatientArrival(string patientId, string hospitalId, IDisease disease, DateTime arrivalTime)
    {
      PatientId = patientId;
      HospitalId = hospitalId;
      Disease = disease;
      ArrivalTime = arrivalTime;
    }

    #endregion

    #region IPatientArrival implementation

    public string PatientId { get; private set; }
    public string HospitalId { get; private set; }
    public DateTime ArrivalTime { get; private set; }
    public IDisease Disease { get; private set; }

    #endregion
  }
}

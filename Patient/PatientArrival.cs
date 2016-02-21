using System;

namespace Patient
{
  public class PatientArrival : IPatientArrival
  {
    #region Constructor

    public PatientArrival(string patientId, string hospitalId, DateTime arrivalTime, DiseaseType diseaseType)
    {
      PatientId = patientId;
      HospitalId = hospitalId;
      ArrivalTime = arrivalTime;
      DiseaseType = diseaseType;
    }

    #endregion

    #region IPatientArrival implementation

    public string PatientId { get; private set; }
    public string HospitalId { get; private set; }
    public DateTime ArrivalTime { get; private set; }
    public DiseaseType DiseaseType { get; private set; }

    #endregion
  }
}

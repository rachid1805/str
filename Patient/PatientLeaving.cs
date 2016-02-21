using System;

namespace Patient
{
  public class PatientLeaving : IPatientLeaving
  {
    #region Constructor

    public PatientLeaving(string patientId, string hospitalId, DateTime leavingTime)
    {
      PatientId = patientId;
      HospitalId = hospitalId;
      LeavingTime = leavingTime;
    }

    #endregion

    #region IPatientLeaving implementation

    public string PatientId { get; private set; }
    public string HospitalId { get; private set; }
    public DateTime LeavingTime { get; private set; }

    #endregion
  }
}

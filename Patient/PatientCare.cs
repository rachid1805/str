using System;

namespace Patient
{
  public class PatientCare : IPatientCare
  {
    #region Constructor

    public PatientCare(string patientId, string hospitalId, DateTime careTime, string doctorId)
    {
      PatientId = patientId;
      HospitalId = hospitalId;
      CareTime = careTime;
      DoctorId = doctorId;
    }

    #endregion

    #region IPatientCare implementation

    public string PatientId { get; private set; }
    public string HospitalId { get; private set; }
    public DateTime CareTime { get; private set; }
    public string DoctorId { get; private set; }

    #endregion
  }
}

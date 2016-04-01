using System;

namespace Common.Entities
{
    public interface IHospitalEvent
    {
        long EventId { get; }
        int HospitalId { get; }
        int PatiendId { get; }
        HospitalEventType EventType { get; }
        DateTime EventTime { get; }
        DiseaseType? DiseaseType { get; }                
        int? DoctorId { get; }                          
    }

    public enum HospitalEventType
    {
        PatientArrival = 0,
        PatientTakenInChargeByDoctor,
        PatientLeaving
    }

    public interface IPatientArrival
    {
        int PatientId { get; }
        int HospitalId { get; }
        DateTime ArrivalTime { get; }
        Disease Disease { get; }
    }

    public interface IPatientTakenInChargeByDoctor
    {
        int PatientId { get; }
        int HospitalId { get; }
        DateTime TakenInChargeByDoctorTime { get; }
        int DoctorId { get; }
        Disease Disease { get; }
  }

    public interface IPatientLeaving
    {
        int PatientId { get; }
        int HospitalId { get; }
        DateTime LeavingTime { get; }
    }

    public class HospitalEvent : IHospitalEvent
    {
        public long EventId { get; set; }
        public int HospitalId { get; set; }
        public int PatiendId { get; set; }
        public HospitalEventType EventType { get; set; }
        public DateTime EventTime { get; set; }
        public DiseaseType? DiseaseType { get; set; }
        public int? DoctorId { get; set; }
    }

    public class PatientArrival : IPatientArrival 
    {
        #region Constructor

        public PatientArrival(int patientId, int hospitalId, Disease disease, DateTime arrivalTime)
        {
            PatientId = patientId;
            HospitalId = hospitalId;
            Disease = disease;
            ArrivalTime = arrivalTime;
        }

        #endregion

        #region IPatientArrival implementation

        public int PatientId { get; }
        public int HospitalId { get;  }
        public DateTime ArrivalTime { get; }
        public Disease Disease { get; }

        #endregion
    }

    public class PatientTakenInChargeByDoctor : IPatientTakenInChargeByDoctor
    {
        #region Constructor

        public PatientTakenInChargeByDoctor( int patientId, int hospitalId, DateTime takenInChargeByDoctorTime, int doctorId, Disease disease )
        {
            PatientId = patientId;
            HospitalId = hospitalId;
            TakenInChargeByDoctorTime = takenInChargeByDoctorTime;
            DoctorId = doctorId;
            Disease = disease;
        }

        #endregion

        #region IPatientTakenInChargeByDoctor implementation

        public int PatientId { get; }
        public int HospitalId { get; }
        public DateTime TakenInChargeByDoctorTime { get; }
        public int DoctorId { get; }
        public Disease Disease { get; }

        #endregion
    }

    public class PatientLeaving : IPatientLeaving 
    {
        #region Constructor

        public PatientLeaving( int patientId, int hospitalId, DateTime leavingTime )
        {
            PatientId = patientId;
            HospitalId = hospitalId;
            LeavingTime = leavingTime;
        }

        #endregion

        #region IPatientLeaving implementation

        public int PatientId { get; }
        public int HospitalId { get;  }
        public DateTime LeavingTime { get; }

        #endregion
    }
}

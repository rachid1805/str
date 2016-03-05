using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Entities
{
    public interface IHospitalEvent
    {
        int EventId { get; }
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
        public int EventId { get; set; }
        public int HospitalId { get; set; }
        public int PatiendId { get; set; }
        public HospitalEventType EventType { get; set; }
        public DateTime EventTime { get; set; }
        public DiseaseType? DiseaseType { get; set; }
        public int? DoctorId { get; set; }
    }

    public class PatientArrival : IPatientArrival //TODO IHospitalEvent
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

        public int PatientId { get; private set; }
        public int HospitalId { get; private set; }
        public DateTime ArrivalTime { get; private set; }
        public Disease Disease { get; private set; }

        #endregion
    }

    public class PatientTakenInChargeByDoctor : IPatientTakenInChargeByDoctor // TODO IHospitalEvent
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

        public int PatientId { get; private set; }
        public int HospitalId { get; private set; }
        public DateTime TakenInChargeByDoctorTime { get; private set; }
        public int DoctorId { get; private set; }
        public Disease Disease { get; private set; }

        #endregion
    }

    public class PatientLeaving : IPatientLeaving  // TODO IHospitalEvent
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

        public int PatientId { get; private set; }
        public int HospitalId { get; private set; }
        public DateTime LeavingTime { get; private set; }

        #endregion
    }
}

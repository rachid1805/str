using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Entities
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
        string PatientId { get; }               // TODO : change data type to int
        string HospitalId { get; }              // TODO : change data type to int
        DateTime ArrivalTime { get; }
        DiseaseType DiseaseType { get; }
    }

    public interface IPatientCare
    {
        string PatientId { get; }           // TODO : change data type to int
        string HospitalId { get; }          // TODO : change data type to int
        DateTime CareTime { get; }
        string DoctorId { get; }
    }

    public interface IPatientLeaving
    {
        string PatientId { get; }               // TODO : change data type to int
        string HospitalId { get; }              // TODO : change data type to int
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

        public PatientArrival( string patientId, string hospitalId, DateTime arrivalTime, DiseaseType diseaseType )
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

    public class PatientCare : IPatientCare // TODO IHospitalEvent
    {
        #region Constructor

        public PatientCare( string patientId, string hospitalId, DateTime careTime, string doctorId )
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

    public class PatientLeaving : IPatientLeaving  // TODO IHospitalEvent
    {
        #region Constructor

        public PatientLeaving( string patientId, string hospitalId, DateTime leavingTime )
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

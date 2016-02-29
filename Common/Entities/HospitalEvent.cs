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

    public enum DiseasePriority
    {
        VeryHigh = 0,
        High = 1,
        Medium = 2,
        Low = 3,
        VeryLow = 4,
        Max = 5
    }

    public enum RequiredTimeUnit
    {
        Hr = 0,
        Min = 1,
        Sec = 2,
        MilliSec = 3,
        Max = 4
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

    public interface IDisease
    {
        DiseaseType Type { get; }
        DiseasePriority Priority { get; }
        uint RequiredTimeValue { get; }
        RequiredTimeUnit RequiredTimeUnit { get; }
    }

    public interface IPatientArrival
    {
        string PatientId { get; }               // TODO : change data type to int
        string HospitalId { get; }              // TODO : change data type to int
        DateTime ArrivalTime { get; }
        IDisease Disease { get; }
    }

    public interface IPatientTakenInChargeByDoctor
    {
        string PatientId { get; }           // TODO : change data type to int
        string HospitalId { get; }          // TODO : change data type to int
        DateTime TakenInChargeByDoctorTime { get; }
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

    public class Disease : IDisease
    {
        #region Constructor

        public Disease(DiseaseType type, DiseasePriority priority, uint requiredTimeValue, RequiredTimeUnit requiredTimeUnit)
        {
            Type = type;
            Priority = priority;
            RequiredTimeValue = requiredTimeValue;
            RequiredTimeUnit = requiredTimeUnit;
        }

        #endregion

        #region IDisease implementation

        public DiseaseType Type { get; private set; }
        public DiseasePriority Priority { get; private set; }
        public uint RequiredTimeValue { get; private set; }
        public RequiredTimeUnit RequiredTimeUnit { get; private set; }

        #endregion
    }

    public class PatientArrival : IPatientArrival //TODO IHospitalEvent
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

    public class PatientTakenInChargeByDoctor : IPatientTakenInChargeByDoctor // TODO IHospitalEvent
    {
        #region Constructor

        public PatientTakenInChargeByDoctor( string patientId, string hospitalId, DateTime takenInChargeByDoctorTime, string doctorId )
        {
            PatientId = patientId;
            HospitalId = hospitalId;
            TakenInChargeByDoctorTime = takenInChargeByDoctorTime;
            DoctorId = doctorId;
        }

        #endregion

        #region IPatientTakenInChargeByDoctor implementation

        public string PatientId { get; private set; }
        public string HospitalId { get; private set; }
        public DateTime TakenInChargeByDoctorTime { get; private set; }
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

using Common.Entities;

namespace PatientGenerator
{
    public interface IPatientGenerator
    {
        IPatientArrival GeneratePatientArrival();
        IPatientTakenInChargeByDoctor GeneratePatientTakenInChargeByDoctor(long timeOutMilliSec);
        IPatientLeaving GeneratePatientLeaving(long timeOutMilliSec);
        int WaitingPatientCount { get; }
        int TakenInChargePatientCount { get; }
    }
}

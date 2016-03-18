using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

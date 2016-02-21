using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Patient;

namespace PatientGenerator
{
  public interface IPatientGenerator
  {
    IPatientArrival GeneratePatientArrival();
    IPatientCare GeneratePatientCare();
    IPatientLeaving GeneratePatientLeaving();
  }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Patient
{
  public interface IPatientLeaving
  {
    string PatientId { get; }
    string HospitalId { get; }
    DateTime LeavingTime { get; }
  }
}

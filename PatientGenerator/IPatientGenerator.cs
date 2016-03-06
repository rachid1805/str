﻿using System;
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
    IPatientTakenInChargeByDoctor GeneratePatientTakenInChargeByDoctor();
    IPatientLeaving GeneratePatientLeaving(long timeOutMilliSec);
  }
}

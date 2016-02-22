using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Patient;

namespace PatientGenerator
{
  public class PatientGenerator : IPatientGenerator
  {
    #region Constructor

    public PatientGenerator()
    {
    }

    #endregion

    #region IPatientLeaving implementation

    public IPatientArrival GeneratePatientArrival()
    {
      return new PatientArrival(GeneratorHelper.RandomUpperChars(4) + GeneratorHelper.RandomNumericalChars(8),
                                GeneratorHelper.RandomLowerChars(6),
                                DateTime.Now,
                                DiseaseType.Influenza);
    }

    public IPatientCare GeneratePatientCare()
    {
      return new PatientCare("", "", DateTime.Now, "");
    }

    public IPatientLeaving GeneratePatientLeaving()
    {
      return new PatientLeaving("", "", DateTime.Now);
    }

    #endregion
  }
}

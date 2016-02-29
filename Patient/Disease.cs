using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Patient
{
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
}

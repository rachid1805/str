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

    public class Disease
    {
        public DiseaseType Id { get; set; }
        public string Name { get; set; }
        public DiseasePriority Priority { get; set; }
        public int RequiredTime { get; set; }
        public RequiredTimeUnit TimeUnit { get; set; }
    }
}


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
        Cold = 7
    }

    public enum DiseasePriority
    {
        VeryHigh = 0,
        High = 1,
        Medium = 2,
        Low = 3,
        VeryLow = 4,
        Invalid = 5
    }

    public enum RequiredTimeUnit
    {
        Min = 0,
        Sec = 1,
        MilliSec = 2
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

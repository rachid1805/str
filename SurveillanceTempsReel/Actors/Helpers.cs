using System;

namespace SurveillanceTempsReel.Actors
{
    public static class PerformanceCounterHelper
    {
        public static readonly string MainCategory = "Surveillance médicale";

        public static readonly string CounterAvgTimeToSeeADoctor = "Temps moyen pour voir un médecin";

        public static readonly string CounterAvgAppointmentDuration = "Temps moyen d'un rendez-vous";

        public static readonly string CounterPerDisease = "Pourcentage de chaque maladie";

        public static readonly string CounterEstimatedTimeToSeeADoctor = "Temps moyen estimé pour voir un médecin";

        public static string GetPerformanceCounterName( StatisticType statType, int hospitalId )
        {
            string name;

            switch ( statType )
            {
                case StatisticType.AvgTimeToSeeADoctor:
                    name = $"(H{hospitalId}) {CounterAvgTimeToSeeADoctor}";
                    break;

                case StatisticType.AvgAppointmentDuration:
                    name = $"(H{hospitalId}) {CounterAvgAppointmentDuration}";
                    break;

                case StatisticType.Illness:
                    name = $"(H{hospitalId}) {CounterPerDisease}";
                    break;

                case StatisticType.EstimatedTimeToSeeADoctor:
                    name = $"(H{hospitalId}) {CounterEstimatedTimeToSeeADoctor}";
                    break;

                default:
                    throw new ArgumentOutOfRangeException( "statType" );
            }

            return name;
        }

        public static string GetPerformanceBaseCounterName(StatisticType statType, int hospitalId)
        {
            string name;

            switch (statType)
            {
                case StatisticType.AvgTimeToSeeADoctor:
                    name = $"(H{hospitalId}) {CounterAvgTimeToSeeADoctor} (BASE)";
                    break;

                case StatisticType.AvgAppointmentDuration:
                    name = $"(H{hospitalId}) {CounterAvgAppointmentDuration} (BASE)";
                    break;

                case StatisticType.Illness:
                    name = $"(H{hospitalId}) {CounterPerDisease} (BASE)";
                    break;

                case StatisticType.EstimatedTimeToSeeADoctor:
                    name = $"(H{hospitalId}) {CounterEstimatedTimeToSeeADoctor} (BASE)";
                    break;

                default:
                    throw new ArgumentOutOfRangeException("statType");
            }

            return name;
        }
    }
}

using System;
using System.Collections.Generic;

namespace SurveillanceTempsReel.Actors
{
    public static class PerformanceCounterHelper
    {
        public static readonly string MainCategory = "Surveillance médicale";

        public static readonly string CounterAvgTimeToSeeADoctor = "Temps moyen pour voir un médecin";

        public static readonly string CounterAvgAppointmentDuration = "Temps moyen d'un rendez-vous";

        public static readonly string CounterPerDisease = "Pourcentage de chaque maladie";

        // TODO ajouter autres compteurs

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

                // TODO ajouter autres compteurs

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

                // TODO ajouter autres compteurs

                default:
                    throw new ArgumentOutOfRangeException("statType");
            }

            return name;
        }
    }
}

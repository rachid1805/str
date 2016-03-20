using System;
using System.Collections.Generic;

namespace SurveillanceTempsReel.Actors
{
    public static class PerformanceCounterHelper
    {
        public static readonly string MainCategory = "Surveillance médicale";

        public static readonly string CounterAvgTimeToSeeADoctor = "Temps moyen pour voir un médecin";

        public static readonly string CounterAvgAppointmentDuration = "Temps moyen d'un rendez-vous";

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

                // TODO ajouter autres compteurs

                default:
                    throw new ArgumentOutOfRangeException( "statType" );
            }

            return name;
        }
    }
}

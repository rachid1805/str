using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Akka.Actor;

namespace SurveillanceTempsReel.Actors
{

    /// <summary>
    /// Les statistiques prises en charge par le système de surveillance.
    /// </summary>
    public enum StatisticType
    {
        AvgTimeToSeeADoctor,
        AvgAppointmentDuration,
        CommonColdCount,
        AllergyCount,
        // TODO.. trouver autres maladies
    }

    public class Watch
    {
        public StatisticType Statistic { get; private set; }

        public Watch( StatisticType stat )
        {
            Statistic = stat;
        }
    }

    public class Unwatch
    {
        public StatisticType Statistic { get; private set; }

        public Unwatch( StatisticType stat )
        {
            Statistic = stat;
        }
    }

    public class SubscribeStatistic
    {
        public StatisticType Statistic { get; private set; }

        public IActorRef Subscriber { get; private set; }

        public SubscribeStatistic( StatisticType stat, IActorRef subscriber )
        {
            Subscriber = subscriber;
            Statistic = stat;
        }
    }

    public class UnsubscribeStatistic
    {
        public StatisticType Statistic { get; private set; }

        public IActorRef Subscriber { get; private set; }

        public UnsubscribeStatistic( StatisticType stat, IActorRef subscriber )
        {
            Subscriber = subscriber;
            Statistic = stat;
        }
    }
}

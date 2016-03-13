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
        IllnessCount,
    }

    #region Message types

    public class GatherStats { }

    public class Stat
    {
        public string Series { get; private set; }

        public float CounterValue { get; private set; }

        public Stat( string series, float counterValue )
        {
            CounterValue = counterValue;
            Series = series;
        }
    }

    public class FetchHostpitalEvents { }
    
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

    public class SubscribeEventFetcher
    {
        public IActorRef Subscriber { get; private set; }

        public SubscribeEventFetcher( IActorRef subscriber )
        {
            Subscriber = subscriber;
        }
    }

    public class UnsubscribeEventFetcher
    {
        public IActorRef Subscriber { get; private set; }

        public UnsubscribeEventFetcher( IActorRef subscriber )
        {
            Subscriber = subscriber;
        }
    }

    #endregion
}

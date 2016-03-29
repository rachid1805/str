using System;
using Akka.Actor;
using Common.Entities;

namespace SurveillanceTempsReel.Actors
{

    /// <summary>
    /// Les statistiques prises en charge par le système de surveillance.
    /// </summary>
    public enum StatisticType
    {
        AvgTimeToSeeADoctor,
        AvgAppointmentDuration,
        Illness,
        EstimatedTimeToSeeADoctor
    }

    #region Message types

    public class GatherStats { }

    public class Stat
    {
        public int HospitalId { get; private set; }
        public StatisticType Statistic { get; private set; }
        public double CounterValue { get; private set; }

        public Stat( int hospitalId, StatisticType stat, double counterValue )
        {
            HospitalId = hospitalId;
            CounterValue = counterValue;
            Statistic = stat;
        }
    }

    public class SeriesStat
    {
        public string Series { get; private set; }
        public double CounterValue { get; private set; }

        public SeriesStat( string series, double counterValue )
        {
            CounterValue = counterValue;
            Series = series;
        }
    }

    public class FetchHostpitalEvents { }

    public class TogglePauseFetchingHospitalEvents { }

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

    public class HospitalEvent { }

    /// <summary>
    /// Arrivée d'un patient à l'hôpital.
    /// </summary>
    public class RegisterPatient : HospitalEvent
    {
        public int HospitalId { get; private set; }
        public int PatientId { get; private set; }
        public DateTime ArrivalTime { get; private set; }
        public Disease Disease { get; private set; }

        public RegisterPatient( int hospitalId, int patientId, DateTime arrivalTime, Disease disease )
        {
            HospitalId = hospitalId;
            PatientId = patientId;
            ArrivalTime = arrivalTime;
            Disease = disease;
        }
    }

    /// <summary>
    /// Départ d'un patient de l'hôpital
    /// </summary>
    public class UnregisterPatient : HospitalEvent
    {
        public int HospitalId { get; private set; }
        public int PatientId { get; private set; }
        public DateTime LeavingTime { get; private set; }

        public UnregisterPatient(int hospitalid, int patientId, DateTime leavingTime)
        {
            HospitalId = hospitalid;
            PatientId = patientId;
            LeavingTime = leavingTime;
        }
    }

    public class BeginAppointmentWithDoctor : HospitalEvent
    {
        public int HospitalId { get; private set; }
        public int PatientId { get; private set; }
        public int DoctorId { get; private set; }
        public DateTime StartTime { get; private set; }
        public Disease Disease { get; private set; }

        public BeginAppointmentWithDoctor( int hospitalId, int patientId, int doctorId, DateTime startTime, Disease disease)
        {
            HospitalId = hospitalId;
            PatientId = patientId;
            DoctorId = doctorId;
            StartTime = startTime;
            Disease = disease;
        }
    }

    #endregion
}

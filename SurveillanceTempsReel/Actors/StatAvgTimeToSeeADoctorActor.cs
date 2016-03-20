using System;
using System.Collections.Generic;
using System.Diagnostics;
using Akka.Actor;
using Common.Entities;

namespace SurveillanceTempsReel.Actors
{
    /// <summary>
    /// Acteur responsable de calculer une statistique pour un hôpital en particulier.
    /// </summary>
    public class StatAvgTimeToSeeADoctorActor : ReceiveActor
    {
        #region Fields and constants

        private readonly Hospital _hospital;

        private readonly IActorRef _hospitalCoordinator;

        private readonly HashSet<IActorRef> _subscriptions;

        private readonly ICancelable _cancelPublishing;

        private PerformanceCounter _counter;

        private Dictionary<int, DateTime> _patients;

        private double _avgMinutesToSeeADoctor;

        private int _statCount;

        #endregion

        public StatAvgTimeToSeeADoctorActor( Hospital hospital, IActorRef hospitalCoordinator )
        {
            _hospital = hospital;
            _hospitalCoordinator = hospitalCoordinator;
            _subscriptions = new HashSet<IActorRef>();
            _cancelPublishing = new Cancelable( Context.System.Scheduler );
            
            Processing();
        }

        #region Actor lifecycle methods

        protected override void PreStart()
        {
            _counter = new PerformanceCounter( PerformanceCounterHelper.MainCategory, PerformanceCounterHelper.GetPerformanceCounterName( StatisticType.AvgTimeToSeeADoctor, _hospital.Id ), string.Empty );
            _patients = new Dictionary<int, DateTime>();
            _avgMinutesToSeeADoctor = 0.0d;
            _statCount = 0;

            // cédule une tâche pour nous envoyer régulièrement un message
            // pour rafraîchir le "dashboard".
            Context.System.Scheduler.ScheduleTellRepeatedly(
                TimeSpan.FromMilliseconds( 250 ),           // TODO tweak numbers
                TimeSpan.FromMilliseconds( 250 ),
                Self,
                new GatherStats(),
                Self,
                _cancelPublishing );
        }

        protected override void PostStop()
        {
            try
            {
                _cancelPublishing.Cancel( false );
                _counter.Dispose();
            }
            catch
            {
                // oh well... on ne se préoccupe pas des autres exceptions
            }
            finally
            {
                base.PostStop();
            }
        }

        #endregion

        private void Processing()
        {
            Receive<GatherStats>( bof =>
            {
                //var stat = new Stat( StatisticType.AvgTimeToSeeADoctor.ToString(), _counter.NextValue() );
                var stat = new Stat(StatisticType.AvgTimeToSeeADoctor.ToString(), _avgMinutesToSeeADoctor);

                foreach ( var sub in _subscriptions )
                    sub.Tell( stat );
            } );

            Receive<SubscribeStatistic>( sc =>
            {
                _subscriptions.Add( sc.Subscriber );
            } );

            Receive<UnsubscribeStatistic>( uc =>
            {
                _subscriptions.Remove( uc.Subscriber );
            } );

            Receive<RegisterPatient>( rp =>   
            {
                _patients.Add( rp.PatientId, rp.ArrivalTime );
            } );

            Receive<BeginAppointmentWithDoctor>(bawd =>
            {
                DateTime arrivalTime;
                if ( _patients.TryGetValue( bawd.PatientId, out arrivalTime ) )
                {
                    var waitingMinutes = ( bawd.StartTime - arrivalTime ).TotalMinutes;
                    _avgMinutesToSeeADoctor = (_avgMinutesToSeeADoctor + waitingMinutes) / ++_statCount;

                    _patients.Remove( bawd.PatientId );
                }
            } );
        }
    }
}

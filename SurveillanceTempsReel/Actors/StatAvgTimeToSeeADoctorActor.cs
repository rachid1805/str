using System;
using System.Collections.Generic;
using System.Diagnostics;
using Akka.Actor;
using Akka.Event;
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
        
        private readonly HashSet<IActorRef> _subscriptions;

        private ICancelable _cancelPublishing;

        private PerformanceCounter _counter;
        //private PerformanceCounter _baseCounter;
        private PerformanceCounter _msgPerSecondCounter;

        private Dictionary<int, DateTime> _patients;
        private double _avgDuration;
        private long _statCount;

        private readonly ILoggingAdapter _log = Context.GetLogger();

        #endregion

        #region Constructors

        public StatAvgTimeToSeeADoctorActor( Hospital hospital )
        {
            _hospital = hospital;
            _subscriptions = new HashSet<IActorRef>();
            
            Processing();
        }

        #endregion

        #region Actor lifecycle methods

        protected override void PreStart()
        {
            _counter = new PerformanceCounter( PerformanceCounterHelper.MainCategory, PerformanceCounterHelper.GetPerformanceCounterName( StatisticType.AvgTimeToSeeADoctor, _hospital.Id ), false );
            //_baseCounter = new PerformanceCounter( PerformanceCounterHelper.MainCategory, PerformanceCounterHelper.GetPerformanceBaseCounterName( StatisticType.AvgTimeToSeeADoctor, _hospital.Id ), false );
            _counter.RawValue = 0;
            //_baseCounter.RawValue = 0;
            _msgPerSecondCounter = new PerformanceCounter( PerformanceCounterHelper.MainCategory, $"(H{_hospital.Id}) Messages par seconde pour {PerformanceCounterHelper.CounterAvgTimeToSeeADoctor}", false );
            
            _patients = new Dictionary<int, DateTime>();
            _avgDuration = 0.0d;
            _statCount = 0;

            _cancelPublishing = ScheduleGatherStatsTask();
        }

        protected override void PostStop()
        {
            try
            {
                _cancelPublishing.Cancel( false );
                _counter.RawValue = 0;
                _counter.Dispose();
                //_baseCounter.Dispose();
                _msgPerSecondCounter.Dispose();
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

        #region Private methods

        private void Processing()
        {
            Receive<GatherStats>( gs =>
            {
                var stat = new Stat( _hospital.Id, StatisticType.AvgTimeToSeeADoctor, _avgDuration );
                //var stat = new Stat(StatisticType.AvgTimeToSeeADoctor, _counter.NextValue());

                foreach ( var sub in _subscriptions )
                    sub.Tell( stat );

                _msgPerSecondCounter.RawValue++;
            } );

            Receive<SubscribeStatistic>( sc =>
            {
                _subscriptions.Add( sc.Subscriber );

                _msgPerSecondCounter.RawValue++;
            } );

            Receive<UnsubscribeStatistic>( uc =>
            {
                _subscriptions.Remove( uc.Subscriber );

                _msgPerSecondCounter.RawValue++;
            } );

            Receive<RegisterPatient>( rp =>   
            {
                var sw = Stopwatch.StartNew();
                _patients.Add( rp.PatientId, rp.ArrivalTime );
                _log.Info( $"(H{_hospital.Id}) Registering patient with ID={rp.PatientId} took {sw.ElapsedTicks} ticks" );

                _msgPerSecondCounter.RawValue++;
            } );

            Receive<BeginAppointmentWithDoctor>(bawd =>
            {
                var sw = Stopwatch.StartNew();

                DateTime arrivalTime;
                if ( _patients.TryGetValue( bawd.PatientId, out arrivalTime ) )
                {
                    var duration = (long)(bawd.StartTime - arrivalTime).TotalMilliseconds;
                    _avgDuration = ((_avgDuration * _statCount) + duration) / (++_statCount);

             
                    _counter.RawValue = (long)_avgDuration;
                    
                    //_baseCounter.Increment();

                    _patients.Remove( bawd.PatientId );

                    _log.Info( $"(H{_hospital.Id}) BeginAppointmentWithDoctor for patient ID={bawd.PatientId} took {sw.ElapsedTicks} ticks" );
                }

                _msgPerSecondCounter.RawValue++;
            } );
        }

        private void Publish( Stat stat )
        {
            foreach ( var s in _subscriptions )
                s.Tell( stat );
        }

        /// <summary>
        /// Cédule une tâche pour nous envoyer régulièrement un message
        /// pour publier la statistique à jour de l'acteur
        /// </summary>
        /// <returns></returns>
        private ICancelable ScheduleGatherStatsTask()
        {
            var cancellation = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(
                TimeSpan.FromMilliseconds( 2000 ),           // TODO : tweak these numbers
                TimeSpan.FromMilliseconds( 1000 ),
                Self,
                new GatherStats(),
                Self );

            return cancellation;
        }

        #endregion
    }
}

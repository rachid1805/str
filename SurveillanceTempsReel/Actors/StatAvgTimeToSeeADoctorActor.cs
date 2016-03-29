﻿using System;
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
        
        private readonly HashSet<IActorRef> _subscriptions;

        private ICancelable _cancelPublishing;

        private PerformanceCounter _counter;
        //private PerformanceCounter _baseCounter;

        private Dictionary<int, DateTime> _patients;
        private double _avgDuration;
        private long _statCount;

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
                    var duration = (long)(bawd.StartTime - arrivalTime).TotalMilliseconds;
                    _avgDuration = ((_avgDuration * _statCount) + duration) / (++_statCount);
                    _counter.RawValue = (long)_avgDuration;
                    //_baseCounter.Increment();

                    _patients.Remove( bawd.PatientId );
                }
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

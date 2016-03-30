using System;
using System.Collections.Generic;
using System.Diagnostics;
using Akka.Actor;
using Common.Entities;

namespace SurveillanceTempsReel.Actors
{
    /// <summary>
    /// Acteur responsable de calculer le nombre de chaque type de maladie pour un hôpital en particulier.
    /// </summary>
    public class StatDiseaseActor : ReceiveActor
    {
        #region Fields and constants

        private readonly Hospital _hospital;
        
        private readonly HashSet<IActorRef> _subscriptions;

        private ICancelable _cancelPublishing;

        private PerformanceCounter _counter;
        //private PerformanceCounter _baseCounter;

        private double _totalCount;
        private double _influenzaCount;

        #endregion

        #region Constructors

        public StatDiseaseActor( Hospital hospital )
        {
            _hospital = hospital;
            _subscriptions = new HashSet<IActorRef>();
            
            Processing();
        }

        #endregion

        #region Actor lifecycle methods

        protected override void PreStart()
        {
            _counter = new PerformanceCounter( PerformanceCounterHelper.MainCategory, PerformanceCounterHelper.GetPerformanceCounterName( StatisticType.Illness, _hospital.Id ), false );
            //_baseCounter = new PerformanceCounter( PerformanceCounterHelper.MainCategory, PerformanceCounterHelper.GetPerformanceBaseCounterName( StatisticType.Illness, _hospital.Id ), false );
            _counter.RawValue = 0;
            //_baseCounter.RawValue = 0;

            _cancelPublishing = ScheduleGatherStatsTask();
        }

        protected override void PostStop()
        {
            try
            {
                _cancelPublishing.Cancel( false ); 
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
                var stat = new Stat( _hospital.Id, StatisticType.Illness, (_influenzaCount * 100) / _totalCount);

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
                // Surveiller par exemple l'éclosion de l'influenza
                if (rp.Disease.Id == DiseaseType.Influenza)
                {
                    _counter.RawValue = (long)(++_influenzaCount);
                }
                ++_totalCount;
            } );
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

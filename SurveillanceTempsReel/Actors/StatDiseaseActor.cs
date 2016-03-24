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

        private readonly IActorRef _hospitalCoordinator;

        private readonly HashSet<IActorRef> _subscriptions;

        private readonly ICancelable _cancelPublishing;

        private PerformanceCounter _counter;
        private PerformanceCounter _baseCounter;

        #endregion

        public StatDiseaseActor( Hospital hospital, IActorRef hospitalCoordinator )
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
            _counter = new PerformanceCounter( PerformanceCounterHelper.MainCategory, PerformanceCounterHelper.GetPerformanceCounterName( StatisticType.Illness, _hospital.Id ), false );
            _baseCounter = new PerformanceCounter( PerformanceCounterHelper.MainCategory, PerformanceCounterHelper.GetPerformanceBaseCounterName( StatisticType.Illness, _hospital.Id ), false );
            _counter.RawValue = 0;
            _baseCounter.RawValue = 0;
        }

        protected override void PostStop()
        {
            try
            {
                _cancelPublishing.Cancel( false ); 
                _counter.Dispose();
                _baseCounter.Dispose();
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
                var stat = new Stat( StatisticType.Illness.ToString(), _counter.NextValue() );

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
                    _counter.Increment();
                }
                _baseCounter.Increment();
            } );
        }
    }
}

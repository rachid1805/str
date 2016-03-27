using System;
using System.Collections.Generic;
using System.Diagnostics;
using Akka.Actor;
using Common.Entities;

namespace SurveillanceTempsReel.Actors
{
    public class StatAvgAppointmentDurationActor : ReceiveActor
    {
        #region Fields and constants

        private readonly Hospital _hospital;
        
        private readonly HashSet<IActorRef> _subscriptions;

        private readonly ICancelable _cancelPublishing;

        private PerformanceCounter _counter;

        #endregion

        public StatAvgAppointmentDurationActor( Hospital hospital )
        {
            _hospital = hospital;
            _subscriptions = new HashSet<IActorRef>();
            _cancelPublishing = new Cancelable( Context.System.Scheduler );
        }

        #region Actor lifecycle methods

        protected override void PreStart()
        {
            _counter = new PerformanceCounter( PerformanceCounterHelper.MainCategory, PerformanceCounterHelper.GetPerformanceCounterName( StatisticType.AvgAppointmentDuration, _hospital.Id ), string.Empty );

            // cédule une tâche pour nous envoyer régulièrement un message
            // pour rafraîchir le "dashboard".
            //Context.System.Scheduler.ScheduleTellRepeatedly(
            //    TimeSpan.FromMilliseconds( 250 ),           // TODO tweak numbers
            //    TimeSpan.FromMilliseconds( 250 ),
            //    Self,
            //    new GatherStats(),
            //    Self,
            //    _cancelPublishing );
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
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Akka.Actor;

namespace SurveillanceTempsReel.Actors
{
    /// <summary>
    /// Acteur responsable de calculer une statistique pour un hôpital en particulier.
    /// </summary>
    public class StatAvgTimeToSeeADoctorActor : ReceiveActor
    {
        private readonly uint _hospitalId;      // TODO : necessary?
        private readonly string _hospitalName;

        private readonly HashSet<IActorRef> _subscriptions;
        private readonly ICancelable _cancelPublishing;

        public StatAvgTimeToSeeADoctorActor( uint hospitalId, string hospitalName )
        {
            _hospitalId = hospitalId;
            _hospitalName = hospitalName;
            _subscriptions = new HashSet<IActorRef>();
            _cancelPublishing = new Cancelable( Context.System.Scheduler );

            Processing();
        }

        #region Actor lifecycle methods

        protected override void PreStart()
        {
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
                // TODO  : send updated data to dashboard
                
            } );

            Receive<SubscribeStatistic>( sc =>
            {
                _subscriptions.Add( sc.Subscriber );
            } );

            Receive<UnsubscribeStatistic>( uc =>
            {
                _subscriptions.Remove( uc.Subscriber );
            } );

            // TODO : create and handle message that contains new data to process
        }
    }
}

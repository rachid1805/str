using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Akka.Actor;
using Common.Helpers;

namespace SurveillanceTempsReel.Actors
{
    /// <summary>
    /// Acteur responsable de lire les événements de la base de données 
    /// et de les envoyer aux récipients.
    /// </summary>
    public class HospitalEventFetcherActor : ReceiveActor
    {
        private readonly uint _hospitalId;
        private readonly string _connectionString;

        private readonly HashSet<IActorRef> _subscriptions;
        private readonly ICancelable _cancelFetching;

        private int _lastEventId;

        public HospitalEventFetcherActor( uint hospitalId, string connectionString )
        {
            _hospitalId = hospitalId;
            _connectionString = connectionString;
            _subscriptions = new HashSet<IActorRef>();
            _cancelFetching = new Cancelable( Context.System.Scheduler );
            _lastEventId = 0;

            Fetching();
        }

        #region Actor lifecycle methods

        protected override void PreStart()
        {
            // TODO : init sql connection

            // cédule une tâche pour nous envoyer régulièrement un message
            // pour obtenir les derniers événements de l'hôpital
            Context.System.Scheduler.ScheduleTellRepeatedly(
                TimeSpan.FromMilliseconds( 250 ),           // TODO : tweak these numbers
                TimeSpan.FromMilliseconds( 250 ),
                Self,
                new FetchHostpitalEvents(),
                Self,
                _cancelFetching );
        }

        protected override void PostStop()
        {
            try
            {
                _cancelFetching.Cancel( false );
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

        private void Fetching()
        {
            Receive<FetchHostpitalEvents>( bof =>
            {
                // TODO : maybe change hospitalId type to int... enough for project
                var hospitalEvents = MedWatchDAL.FindHospitalEventsAfter( (int) _hospitalId, afterEventId: _lastEventId );

                // TODO convert to actor messages and propagate into system
            } );

            Receive<SubscribeEventFetcher>( sc =>
            {
                _subscriptions.Add( sc.Subscriber );
            } );

            Receive<UnsubscribeEventFetcher>( uc =>
            {
                _subscriptions.Remove( uc.Subscriber );
            } );
        }
    }
}

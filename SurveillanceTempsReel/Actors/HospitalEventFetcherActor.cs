using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Akka.Actor;
using Akka.Event;
using Common.Helpers;

namespace SurveillanceTempsReel.Actors
{
    /// <summary>
    /// Acteur responsable de lire les événements de la base de données 
    /// et de les envoyer aux récipients.
    /// </summary>
    public class HospitalEventFetcherActor : ReceiveActor
    {
        private readonly int _hospitalId;
        private readonly string _connectionString;

        private HashSet<IActorRef> _subscriptions;
        private ICancelable _cancelFetching;

        private int _lastEventId;

        private readonly ILoggingAdapter _log = Logging.GetLogger( Context );
        
        public HospitalEventFetcherActor( int hospitalId, string connectionString )
        {
            _hospitalId = hospitalId;
            _connectionString = connectionString;
            
            Fetching();
        }

        #region Actor lifecycle methods

        protected override void PreStart()
        {
            _log.Debug( "PreStart" );

            _subscriptions = new HashSet<IActorRef>();
            _cancelFetching = new Cancelable( Context.System.Scheduler );
            _lastEventId = 0;           // TODO restore state

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

        // TODO : this is a good place to (see p.76):
        // - stash messages?
        // - save _lastEventId ?
        //protected override void PreRestart( Exception reason, object message )
        //{
        //    base.PreRestart( reason, message );
        //}

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
                _log.Debug( $"Fetching hospital events after event id {_lastEventId}" );
                var hospitalEvents = MedWatchDAL.FindHospitalEventsAfter( _hospitalId, afterEventId: _lastEventId );

                // TODO JS send messages

                // TODO JS update _lastEventId

                _log.Debug( $"Number of events fetched: {hospitalEvents.Count()}" );
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

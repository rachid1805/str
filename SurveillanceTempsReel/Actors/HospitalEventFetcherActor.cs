using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Akka.Actor;
using Akka.Event;
using Common.Entities;
using Common.Helpers;

namespace SurveillanceTempsReel.Actors
{
    /// <summary>
    /// Acteur responsable de lire les événements de la base de données 
    /// et de les envoyer aux récipients.
    /// </summary>
    public class HospitalEventFetcherActor : ReceiveActor, IWithUnboundedStash
    {
        #region Fields and constants

        private static readonly int MaxCountPerFetch = 25000;

        public IStash Stash { get; set; }

        private readonly Hospital _hospital;

        private Dictionary<DiseaseType, Disease> _diseases;

        private readonly HashSet<IActorRef> _subscriptions;
        private ICancelable _cancelFetching;
        
        private long _lastEventId;
        
        private readonly ILoggingAdapter _log = Context.GetLogger();

        #endregion

        public HospitalEventFetcherActor( Hospital hospital )
        {
            _hospital = hospital;
            _subscriptions = new HashSet<IActorRef>();
            _diseases = new Dictionary<DiseaseType, Disease>();

            Paused();
        }

        #region Actor lifecycle methods

        protected override void PreStart()
        {
            _lastEventId = 0;           

            _diseases = InitDiseases();
        }

        protected override void PostStop()
        {
            try
            {
                _cancelFetching?.Cancel( false );
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

        private void Fetching()
        {
            Receive<SubscribeEventFetcher>( sc =>
            {
                _subscriptions.Add( sc.Subscriber );
            } );

            Receive<UnsubscribeEventFetcher>( uc =>
            {
                _subscriptions.Remove( uc.Subscriber );
            } );

            Receive<FetchHostpitalEvents>( fetch =>
            {
                _log.Debug( $"(H{_hospital.Id}) Fetching hospital events after event id {_lastEventId}" );

                var sw = Stopwatch.StartNew();

                var dbEvents = MedWatchDAL.FindHospitalEventsAfter( _hospital.Id, _lastEventId, MaxCountPerFetch );
                
                foreach ( var dbe in dbEvents )
                {
                    var actorEvent = ConvertToActorEvent( dbe );
                    Publish( actorEvent );

                    _lastEventId = dbe.EventId;
                }

                //var beforeLog = sw.Elapsed;
                _log.Info( $"(H{_hospital.Id}) Fetching and publishing {dbEvents.Count()} events took {sw.ElapsedMilliseconds} ms" );
                //var afterLog = sw.Elapsed;

                //_log.Info( $"(H{_hospital.Id}) Logging took {(afterLog - beforeLog).TotalMilliseconds} ms" );
            } );

            Receive<TogglePauseFetchingHospitalEvents>( togglePauseFetching =>
            {
                _cancelFetching.Cancel( false );
                UnbecomeStacked();
            } );
        }
        
        private void Paused()
        {
            Receive<SubscribeEventFetcher>( sc => Stash.Stash() );
            Receive<UnsubscribeEventFetcher>( uc => Stash.Stash() );
            Receive<FetchHostpitalEvents>( fetch => Stash.Stash() );
            Receive<TogglePauseFetchingHospitalEvents>( togglePauseFetching =>
            {
                BecomeStacked( Fetching );
                Stash.UnstashAll();
                _cancelFetching = ScheduleFetchingTask();
            } );
        }

        private void Publish(HospitalEvent he)
        {
            foreach (var s in _subscriptions)
                s.Tell(he);
        }

        private HospitalEvent ConvertToActorEvent(Common.Entities.IHospitalEvent dbEvent)
        {
            HospitalEvent hospitalEvent = null;

            switch (dbEvent.EventType)
            {
                case HospitalEventType.PatientArrival:
                    hospitalEvent = new RegisterPatient(dbEvent.HospitalId, dbEvent.PatiendId, dbEvent.EventTime, _diseases[dbEvent.DiseaseType.Value]);
                    break;

                case HospitalEventType.PatientLeaving:
                    hospitalEvent = new UnregisterPatient(dbEvent.HospitalId, dbEvent.PatiendId, dbEvent.EventTime);
                    break;

                case HospitalEventType.PatientTakenInChargeByDoctor:
                    hospitalEvent = new BeginAppointmentWithDoctor(dbEvent.HospitalId, dbEvent.PatiendId, dbEvent.DoctorId.Value, dbEvent.EventTime, _diseases[dbEvent.DiseaseType.Value]);
                    break;

                default:
                    throw new ArgumentException("Unexpected hospital event type");
            }

            return hospitalEvent;
        }

        private static Dictionary<DiseaseType, Disease> InitDiseases()
        {
            var diseases = MedWatchDAL.FindDiseases();
            
            return diseases.ToDictionary(d => d.Id);
        }

        /// <summary>
        /// Cédule une tâche pour nous envoyer régulièrement un message
        /// pour obtenir les derniers événements de l'hôpital
        /// </summary>
        /// <returns></returns>
        private ICancelable ScheduleFetchingTask()
        {
            var cancellation = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(
                TimeSpan.FromMilliseconds( 1000 ),           
                TimeSpan.FromMilliseconds( 1000 ),
                Self,
                new FetchHostpitalEvents(),
                Self );

            return cancellation;
        }

        #endregion
    }
}

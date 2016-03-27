using System;
using System.Collections.Generic;
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
    public class HospitalEventFetcherActor : ReceiveActor
    {
        #region Fields and constants

        private static readonly int MaxCountPerFetch = 1000;

        private readonly Hospital _hospital;
        private readonly string _connectionString;

        private Dictionary<DiseaseType, Disease> _diseases;

        private HashSet<IActorRef> _subscriptions;
        private ICancelable _cancelFetching;
        
        private long _lastEventId;

        private readonly ILoggingAdapter _log = Logging.GetLogger( Context );

        #endregion

        public HospitalEventFetcherActor( Hospital hospital , string connectionString )
        {
            _hospital = hospital;
            _connectionString = connectionString;
            _subscriptions = new HashSet<IActorRef>();
            _cancelFetching = new Cancelable(Context.System.Scheduler);
            _diseases = new Dictionary<DiseaseType, Disease>();

            Fetching();
        }

        #region Actor lifecycle methods

        protected override void PreStart()
        {
            _log.Debug( "PreStart" );
            
            _lastEventId = 0;           // TODO restore state

            _diseases = InitDiseases();

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

        #region Private methods

        private void Fetching()
        {
            Receive<FetchHostpitalEvents>( bof =>
            {
                _log.Debug( $"Fetching hospital events after event id {_lastEventId}" );
                var dbEvents = MedWatchDAL.FindHospitalEventsAfter( _hospital.Id, _lastEventId, MaxCountPerFetch);

                foreach (var dbe in dbEvents)
                {
                    var actorEvent = ConvertToActorEvent(dbe);
                    Publish(actorEvent);

                    _lastEventId = dbe.EventId;
                }
                
                _log.Debug( $"Number of events fetched: {dbEvents.Count()}" );
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
            var diseasesByType = new Dictionary<DiseaseType, Disease>();
            
            var diseases = MedWatchDAL.FindDiseases();

            foreach (var d in diseases)
            {
                diseasesByType.Add(d.Id, d);
            }

            return diseasesByType;
        }

        #endregion
    }
}

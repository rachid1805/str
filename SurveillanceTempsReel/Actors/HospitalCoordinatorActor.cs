using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms.DataVisualization.Charting;
using Akka.Actor;
using Akka.Routing;
using Common.Entities;
using Common.Helpers;

namespace SurveillanceTempsReel.Actors
{
    /// <summary>
    /// Acteur responsable de la coordination des acteurs de statistiques pour un hôpital donné.
    /// </summary>
    public class HospitalCoordinatorActor : ReceiveActor
    {
        #region Fields and constants

        private readonly Hospital _hospital;

        private readonly Dictionary<StatisticType, IActorRef> _hospitalStatActors;
        
        private readonly IActorRef _dashboardActor;

        private IActorRef _coordinatorActor;
        private IActorRef _eventFetcherActor;

        #endregion

        #region Constructors

        public HospitalCoordinatorActor( Hospital hospital, IActorRef dashboardActor )
        {
            _hospital = hospital;
            _dashboardActor = dashboardActor;
            _hospitalStatActors = new Dictionary<StatisticType, IActorRef>();

            Processing();
        }

        #endregion 

        #region Actor lifecycle methods

        protected override void PreStart()
        {
            // crée un acteur enfant pour chacune des statistiques à surveiller

            // stat 1
            {
                var actorStat1 = Context.ActorOf(Props.Create(() => new StatAvgTimeToSeeADoctorActor(_hospital)), ActorPaths.StatAvgTimeToSeeADoctorActorName);
                _hospitalStatActors[StatisticType.AvgTimeToSeeADoctor] = actorStat1;
                
                // l'acteur de statistique doit publier ses données vers l'acteur du "dashboard"
                _hospitalStatActors[StatisticType.AvgTimeToSeeADoctor].Tell(new SubscribeStatistic(StatisticType.AvgTimeToSeeADoctor, _dashboardActor));
            }

            // stat 2
            {
                var actorStat2 = Context.ActorOf(Props.Create(() => new StatAvgAppointmentDurationActor(_hospital)), ActorPaths.StatAvgAppointmentDurationActorName);
                _hospitalStatActors[StatisticType.AvgAppointmentDuration] = actorStat2;

                _hospitalStatActors[StatisticType.AvgAppointmentDuration].Tell(new SubscribeStatistic(StatisticType.AvgAppointmentDuration, _dashboardActor));
            }

            // stat 3
            {
                var actorStat3 = Context.ActorOf(Props.Create(() => new StatDiseaseActor(_hospital)), ActorPaths.StatDiseaseActorName);
                _hospitalStatActors[StatisticType.Illness] = actorStat3;
                
                _hospitalStatActors[StatisticType.Illness].Tell(new SubscribeStatistic(StatisticType.Illness, _dashboardActor));
            }

            // stat 4
            {
                var actorStat4 = Context.ActorOf(Props.Create(() => new StatEstimatedTimeToSeeADoctorActor(_hospital)), ActorPaths.StatEstimatedTimeToSeeADoctorActorName);
                _hospitalStatActors[StatisticType.EstimatedTimeToSeeADoctor] = actorStat4;
                
                _hospitalStatActors[StatisticType.EstimatedTimeToSeeADoctor].Tell(new SubscribeStatistic(StatisticType.EstimatedTimeToSeeADoctor, _dashboardActor));
            }

            // crée un routeur pour broadcaster les messages vers les acteurs de statistiques
            _coordinatorActor = Context.ActorOf(Props.Empty.WithRouter(new BroadcastGroup(
                ActorPaths.GetActorPath(ActorType.StatAvgTimeToSeeADoctorActor, _hospital.Id),
                ActorPaths.GetActorPath(ActorType.StatAvgAppointmentDurationActor, _hospital.Id),
                ActorPaths.GetActorPath(ActorType.StatDiseaseActor, _hospital.Id),
                ActorPaths.GetActorPath(ActorType.StatEstimatedTimeToSeeADoctorActor, _hospital.Id))), "router");
           
            // crée un acteur pour obtenir les événements de la BD et les propager dans le système d'acteurs.
            _eventFetcherActor = Context.ActorOf( Props.Create( () => new HospitalEventFetcherActor( _hospital, MedWatchDAL.ConnectionString ) ), ActorPaths.HospitalEventFetcherActorName );
            _eventFetcherActor.Tell( new SubscribeEventFetcher( _coordinatorActor ));

            base.PreStart();
        }

        protected override void PreRestart( Exception reason, object message )
        {
            _coordinatorActor.Tell( PoisonPill.Instance );
            base.PreRestart( reason, message );
        }

        #endregion

        #region Private methods

        private void Processing()
        {
            Receive<TogglePauseFetchingHospitalEvents>( togglePauseFetching =>
            {
                _eventFetcherActor.Tell( togglePauseFetching );
            } );
        }

        #endregion
    }
}

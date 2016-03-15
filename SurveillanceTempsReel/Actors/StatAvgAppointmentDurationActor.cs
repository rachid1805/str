using System;
using System.Collections.Generic;
using Akka.Actor;
using Common.Entities;

namespace SurveillanceTempsReel.Actors
{
    public class StatAvgAppointmentDurationActor : ReceiveActor
    {
        private readonly Hospital _hospital;
        
        private readonly IActorRef _hospitalCoordinator;

        private readonly HashSet<IActorRef> _subscriptions;

        private readonly ICancelable _cancelPublishing;

        public StatAvgAppointmentDurationActor( Hospital hospital, IActorRef hospitalCoordinator )
        {
            _hospital = hospital;
            _hospitalCoordinator = hospitalCoordinator;
            _subscriptions = new HashSet<IActorRef>();
            _cancelPublishing = new Cancelable( Context.System.Scheduler );
        }
    }
}

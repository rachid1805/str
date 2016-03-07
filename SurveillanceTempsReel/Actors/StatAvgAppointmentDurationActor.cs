using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Akka.Actor;

namespace SurveillanceTempsReel.Actors
{
    public class StatAvgAppointmentDurationActor : ReceiveActor
    {
        private readonly int _hospitalId;      // TODO : necessary?
        private readonly string _hospitalName;

        private readonly HashSet<IActorRef> _subscriptions;

        public StatAvgAppointmentDurationActor( int hospitalId, string hospitalName )
        {
            _hospitalId = hospitalId;
            _hospitalName = hospitalName;


        }
    }
}

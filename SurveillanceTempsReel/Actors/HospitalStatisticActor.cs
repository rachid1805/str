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
    public class HospitalStatisticActor : ReceiveActor
    {
        private readonly uint _hospitalId;      // TODO : necessary?
        private readonly string _hospitalName;

        private readonly HashSet<IActorRef> _subscriptions;

        public HospitalStatisticActor( uint hospitalId, string hospitalName )
        {
            _hospitalId = hospitalId;
            _hospitalName = hospitalName;


        }

       
    }
}

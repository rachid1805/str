using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;

namespace SurveillanceTempsReel.Actors
{
    /// <summary>
    /// Acteur responsable de la coordination des acteurs de statistiques pour un hôpital donné.
    /// </summary>
    public class HospitalCoordinatorActor : ReceiveActor
    {
        private readonly uint _hospitalId;
        private readonly string _hospitalName;

        private readonly Dictionary<StatisticType, IActorRef> _hospitalStatActors;

        private readonly IActorRef _dashboardActor;

        public HospitalCoordinatorActor( uint hospitalId, string hospitalName, IActorRef dashboardActor )
            : this( hospitalId, hospitalName, dashboardActor, new Dictionary<StatisticType, IActorRef>() )
        { }

        public HospitalCoordinatorActor( uint hospitalId, string hospitalName, IActorRef dashboardActor, Dictionary<StatisticType, IActorRef> hospitalStatActors )
        {
            _hospitalId = hospitalId;
            _hospitalName = hospitalName;
            _dashboardActor = dashboardActor;
            _hospitalStatActors = hospitalStatActors;
            
            Receive<Watch>( watch =>
            {
                if ( !_hospitalStatActors.ContainsKey( watch.Statistic ) )
                {
                    // crée un acteur enfant pour surveiller cette statistique
                    var statActor = Context.ActorOf( Props.Create( () => new HospitalStatisticActor( hospitalId, hospitalName ) ) );

                    _hospitalStatActors[ watch.Statistic ] = statActor;
                }
            } );
        }
    }
}

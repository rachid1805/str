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
                    // pas très évolutif comme code, mais fait l'affaire dans le contexte du projet.
                    IActorRef statActor = null;

                    if ( watch.Statistic == StatisticType.AvgTimeToSeeADoctor )
                        statActor = Context.ActorOf( Props.Create( () => new StatAvgTimeToSeeADoctorActor( hospitalId, hospitalName ) ) );
                    else if ( watch.Statistic == StatisticType.AvgAppointmentDuration )
                        statActor = Context.ActorOf( Props.Create( () => new StatAvgAppointmentDurationActor( hospitalId, hospitalName ) ) );
                    else
                        throw new Exception( $"missing handler for stat type: {watch.Statistic}" );
                    
                    _hospitalStatActors[ watch.Statistic ] = statActor;
                }

                // TODO : send message to dashboard actor to register series
                //_dashboardActor.Tell( )

                // l'acteur de statistique doit publier ses données vers l'acteur du "dashboard"
                _hospitalStatActors[ watch.Statistic ].Tell( new SubscribeStatistic( watch.Statistic, _dashboardActor ) );
            } );

            Receive<Unwatch>( unwatch =>
            {
                if ( !_hospitalStatActors.ContainsKey( unwatch.Statistic ) )
                    return;

                // désabonnement auprès de l'acteur du "dashboard"
                _hospitalStatActors[ unwatch.Statistic ].Tell( new UnsubscribeStatistic( unwatch.Statistic, _dashboardActor ) );

                // TODO : remove series
                //_dashboardActor.Tell( )
            } );
        }
    }
}

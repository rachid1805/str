using System.Collections.Generic;
using Common.Entities;
using Akka.Actor;

namespace SurveillanceTempsReel.Actors
{
    public class MediWatchCommanderActor : ReceiveActor
    {
        #region Fields and constants

        private readonly IActorRef _dashboardActor;
        
        private readonly Dictionary<int, IActorRef> _hospitalCoordinatorActors;   // maps hospital id -> actor ref

        #endregion

        #region Constructors

        public MediWatchCommanderActor( IEnumerable<Hospital> hospitals, IActorRef dashboardActor )
        {
            _dashboardActor = dashboardActor;
            _hospitalCoordinatorActors = InitializeHospitalCoordinatorActors( hospitals );

            Processing();
        }

        #endregion

        #region Private methods
        
        private void Processing()
        {
            Receive<TogglePauseFetchingHospitalEvents>( togglePauseFetching =>
            {
                foreach ( var coordinator in _hospitalCoordinatorActors.Values )
                {
                    coordinator.Tell( togglePauseFetching );
                }
            } );
        }

        private Dictionary<int, IActorRef> InitializeHospitalCoordinatorActors( IEnumerable<Hospital> hospitals )
        {
            var coordinators = new Dictionary<int, IActorRef>();

            foreach (var h in hospitals)
            {
                if ( h == null ) continue;

                var actor = Context.ActorOf( Props.Create( () => new HospitalCoordinatorActor( h, _dashboardActor ) ), ActorPaths.GetActorCoordinatorName( h.Id ) );
                coordinators[ h.Id ] = actor;
            }

            return coordinators;
        }

        #endregion

    }
}

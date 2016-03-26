using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SurveillanceTempsReel.Actors
{
    public enum ActorType
    {
        Dashboard,
        Commander,
        Coordinator,
        StatAvgTimeToSeeADoctorActor,
        StatAvgAppointmentDurationActor,
        StatDiseaseActor,
        StatEstimatedTimeToSeeADoctorActor
    }

    public static class ActorPaths
    {
        public static readonly string DashboardActorName = "dashboard";
        public static readonly string MediWatchCommanderActorName = "commander";
        public static readonly string HospitalCoordinatorActorName = "coordinator";
        public static readonly string StatAvgTimeToSeeADoctorActorName = "stat1";
        public static readonly string StatAvgAppointmentDurationActorName = "stat2";
        public static readonly string StatDiseaseActorName = "stat3";
        public static readonly string StatEstimatedTimeToSeeADoctorActorName = "stat4";

        public static readonly string MediWatchActorSystemName = "MediWatchActors";

        public static readonly string PathPrefix = "akka://" + MediWatchActorSystemName;

        public static string GetActorPath( ActorType actorType, int hospitalId = 0 )
        {
            string path;

            switch ( actorType )
            {
                case ActorType.Dashboard:
                    path = $"{PathPrefix}/user/{DashboardActorName}";
                    break;

                case ActorType.Commander:
                    path = $"{PathPrefix}/user/{MediWatchCommanderActorName}";
                    break;

                case ActorType.Coordinator:
                    if ( hospitalId <= 0 ) throw new ArgumentOutOfRangeException( "hospitalId" );
                    path = $"{PathPrefix}/user/{MediWatchCommanderActorName}/{GetActorCoordinatorName( hospitalId )}";
                    break;

                case ActorType.StatAvgTimeToSeeADoctorActor:
                    if ( hospitalId <= 0 ) throw new ArgumentOutOfRangeException( "hospitalId" );
                    path = $"{PathPrefix}/user/{MediWatchCommanderActorName}/{GetActorCoordinatorName( hospitalId )}/{StatAvgTimeToSeeADoctorActorName}";
                    break;

                case ActorType.StatAvgAppointmentDurationActor:
                    if ( hospitalId <= 0 ) throw new ArgumentOutOfRangeException( "hospitalId" );
                    path = $"{PathPrefix}/user/{MediWatchCommanderActorName}/{GetActorCoordinatorName( hospitalId )}/{StatAvgAppointmentDurationActorName}";
                    break;

                case ActorType.StatDiseaseActor:
                    if (hospitalId <= 0) throw new ArgumentOutOfRangeException("hospitalId");
                    path = $"{PathPrefix}/user/{MediWatchCommanderActorName}/{GetActorCoordinatorName(hospitalId)}/{StatDiseaseActorName}";
                    break;

                case ActorType.StatEstimatedTimeToSeeADoctorActor:
                    if (hospitalId <= 0) throw new ArgumentOutOfRangeException("hospitalId");
                    path = $"{PathPrefix}/user/{MediWatchCommanderActorName}/{GetActorCoordinatorName(hospitalId)}/{StatEstimatedTimeToSeeADoctorActorName}";
                    break;

                default:
                    throw new ArgumentOutOfRangeException( "actorType" );
            }

            return path;
        }

        public static string GetActorCoordinatorName( int hospitalId )
        {
            return $"{HospitalCoordinatorActorName}_H{hospitalId}";
        }
    }
    
    public class ActorMetaData
    {
        public string Name { get; private set; }
        public string Path { get; private set; }

        public ActorMetaData( string name, string path )
        {
            Name = name;
            Path = path;
        }
    }
}

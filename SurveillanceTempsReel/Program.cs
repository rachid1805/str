using System;
using System.Windows.Forms;
using Akka.Actor;

namespace SurveillanceTempsReel
{
    static class Program
    {
        /// <summary>
        /// Le système d'acteurs
        /// </summary>
        public static ActorSystem MediWatchActors;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            MediWatchActors = ActorSystem.Create( Actors.ActorPaths.MediWatchActorSystemName );
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault( false );
            Application.Run( new Dashboard() );
        }
    }
}

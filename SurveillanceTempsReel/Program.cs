using System;
using System.Windows.Forms;
using Akka.Actor;

namespace SurveillanceTempsReel
{
    static class Program
    {
        /// <summary>
        /// Notre système d'acteurs.
        /// </summary>
        public static ActorSystem MedWatchActors;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            MedWatchActors = ActorSystem.Create("MedWatchActors");
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault( false );
            Application.Run( new Dashboard() );
        }
    }
}

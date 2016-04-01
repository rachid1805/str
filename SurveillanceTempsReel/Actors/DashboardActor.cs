using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Akka.Actor;


namespace SurveillanceTempsReel.Actors 
{
    public class DashboardActor : ReceiveActor, IWithUnboundedStash
    {
        #region Message types
        
        public class TogglePause { }

        #endregion

        #region Fields and constants

        public const int MaxPoints = 250;

        public IStash Stash { get; set; }

        private int _statChartXPosCounter = 0;

        private readonly DataTable _hospitalStatsDataTable;

        private readonly Button _pauseButton;
        
        #endregion

        #region Constructors

        public DashboardActor( DataTable hospitalStatsDataTable, Button pauseButton )
        {
            _hospitalStatsDataTable = hospitalStatsDataTable;
            _pauseButton = pauseButton;
            
            Paused();
        }

        #endregion

        #region Private methods

        private void Running()
        {
            Receive<Stat>( stat =>
            {
                HandleStat( stat );
            } );

            Receive<TogglePause>( pause =>
            {
                SetPauseButtonText( _pauseButton, paused: true );
                UnbecomeStacked();

                // Stop fetching hospital events
                var commander = Context.ActorSelection( ActorPaths.GetActorPath( ActorType.Commander ) );
                commander.Tell( new TogglePauseFetchingHospitalEvents() );
            } );
        }

        private void Paused()
        {
            Receive<Stat>( stat =>
            {
                HandleStat( stat );
            } );

            Receive<TogglePause>( pause =>
            {
                SetPauseButtonText( _pauseButton, paused: false );
                BecomeStacked( Running );
                
                Stash.UnstashAll();

                // Resume fetching hospital events
                var commander = Context.ActorSelection( ActorPaths.GetActorPath( ActorType.Commander ) );
                commander.Tell( new TogglePauseFetchingHospitalEvents() );
            } );
        }

        private static void SetPauseButtonText( Button button, bool paused )
        {
            button.Text = string.Format( "{0}", !paused ? "ARRÊTER" : "DÉMARRER" );
        }
        
        #endregion

        #region Message handlers

        private void HandleStat( Stat stat )
        {
            string filter = $"{Dashboard.HospitalIDColumnName}={stat.HospitalId}";

            DataRow[] hospitalRows = _hospitalStatsDataTable.Select( filter );
            // using unique id, so should only be 1 row
            System.Diagnostics.Debug.Assert( hospitalRows.Length == 1 );

            string columnNameToUpdate = string.Empty;
            string numericFormat = "F2";

            switch ( stat.Statistic )
            {
                case StatisticType.EstimatedTimeToSeeADoctor:
                    columnNameToUpdate = Dashboard.EstimatedTimeToSeeADoctorColumnName;
                    break;

                case StatisticType.AvgTimeToSeeADoctor:
                    columnNameToUpdate = Dashboard.AvgTimeToSeeADoctorColumnName;
                    break;

                case StatisticType.AvgAppointmentDuration:
                    columnNameToUpdate = Dashboard.AvgAppointmentDurationColumnName;
                    break;

                case StatisticType.Illness:
                    columnNameToUpdate = Dashboard.InfluenzaStatColumnName;
                    break;

                default:
                    throw new ArgumentException( $"Unexpected StatisticType: {stat.Statistic}" );
            }

            hospitalRows[ 0 ][ columnNameToUpdate ] = stat.CounterValue.ToString( numericFormat );
         }
        
        #endregion
    }
}

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

        public class InitializeStatChart
        {
            public Dictionary<string, Series> InitialSeries { get; private set; }

            public InitializeStatChart( Dictionary<string, Series> initialSeries )
            {
                InitialSeries = initialSeries;
            }
        }

        public class AddSeriesToStatChart
        {
            public Series Series { get; private set; }

            public AddSeriesToStatChart( Series series )
            {
                Series = series;
            }
        }

        public class RemoveSeriesFromStatChart
        {
            public string SeriesName { get; private set; }

            public RemoveSeriesFromStatChart( string seriesName )
            {
                SeriesName = seriesName;
            }
        }
        
        public class TogglePause { }

        #endregion

        #region Fields and constants

        public const int MaxPoints = 250;

        public IStash Stash { get; set; }

        private int _statChartXPosCounter = 0;

        private readonly DataTable _hospitalStatsDataTable;

        private readonly Button _pauseButton;

        // TODO deprecated stuff
        private readonly Chart _statChart;
        private Dictionary<string, Series> _statChartSeries;

        #endregion

        #region Constructors

        public DashboardActor( DataTable hospitalStatsDataTable, Button pauseButton )
        {
            _hospitalStatsDataTable = hospitalStatsDataTable;
            _pauseButton = pauseButton;

            // TODO deprecated stuff
            _statChart = new Chart();       
            _statChartSeries = new Dictionary<string, Series>();
            ///////////
           
            Paused();
        }

        #endregion

        #region Private methods

        private void Running()
        {
            /* TODO DEPRECATED
            Receive<InitializeStatChart>( isc => HandleInitializeStatChart( isc ) );
            Receive<AddSeriesToStatChart>( addSeries => HandleAddSeriesToStatChart( addSeries ) );
            Receive<RemoveSeriesFromStatChart>( removeSeries => HandleRemoveSeriesFromStatChart( removeSeries ) );
            Receive<SeriesStat>( stat => HandleSeriesStats( stat ) );
            */

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
            /* TODO DEPRECATED
            Receive<AddSeriesToStatChart>( addSeries => Stash.Stash() );
            Receive<RemoveSeriesFromStatChart>( removeSeries => Stash.Stash() );
            Receive<SeriesStat>( stat => HandleStatPaused( stat ) );
            */

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

        /// <summary>
        /// <DEPRECATED
        /// </summary>
        private static void SetChartBoundaries( Chart chart, Dictionary<string, Series> series, int _statChartXPosCounter )
        {
            double maxAxisX, maxAxisY, minAxisX, minAxisY = 0.0d;
            var allPoints = series.Values.SelectMany( s => s.Points ).ToList();
            var yValues = allPoints.SelectMany( point => point.YValues ).ToList();
            maxAxisX = _statChartXPosCounter;
            minAxisX = _statChartXPosCounter - MaxPoints;
            maxAxisY = yValues.Count > 0 ? Math.Ceiling( yValues.Max() ) : 1.0d;
            minAxisY = yValues.Count > 0 ? Math.Floor( yValues.Min() ) : 0.0d;
            if ( allPoints.Count > 2 )
            {
                var area = chart.ChartAreas[ 0 ];
                area.AxisX.Minimum = minAxisX;
                area.AxisX.Maximum = maxAxisX;
                area.AxisY.Minimum = minAxisY;
                area.AxisY.Maximum = maxAxisY;
            }
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

        private void HandleInitializeStatChart( InitializeStatChart isc )
        {
            if ( isc.InitialSeries != null )
            {
                _statChartSeries = isc.InitialSeries;
            }
            
            _statChart.Series.Clear();

            var area = _statChart.ChartAreas[ 0 ];
            area.AxisX.IntervalType = DateTimeIntervalType.Number;
            area.AxisY.IntervalType = DateTimeIntervalType.Number;

            SetChartBoundaries( _statChart, _statChartSeries, _statChartXPosCounter );
            
            if ( _statChartSeries.Any() )
            {
                foreach ( var series in _statChartSeries )
                {
                    series.Value.Name = series.Key;
                    _statChart.Series.Add( series.Value );
                }
            }

            SetChartBoundaries( _statChart, _statChartSeries, _statChartXPosCounter );
        }

        private void HandleAddSeriesToStatChart( AddSeriesToStatChart series )
        {
            if ( !string.IsNullOrEmpty( series.Series.Name ) &&
                !_statChartSeries.ContainsKey( series.Series.Name ) )
            {
                _statChartSeries.Add( series.Series.Name, series.Series );
                _statChart.Series.Add( series.Series );
                SetChartBoundaries( _statChart, _statChartSeries, _statChartXPosCounter );
            }
        }

        private void HandleRemoveSeriesFromStatChart( RemoveSeriesFromStatChart series )
        {
            if ( !string.IsNullOrEmpty( series.SeriesName ) && _statChartSeries.ContainsKey( series.SeriesName ) )
            {
                var seriesToRemove = _statChartSeries[ series.SeriesName ];
                _statChartSeries.Remove( series.SeriesName );
                _statChart.Series.Remove( seriesToRemove );
                SetChartBoundaries( _statChart, _statChartSeries, _statChartXPosCounter );
            }
        }

        private void HandleSeriesStats( SeriesStat stat )
        {
            if ( !string.IsNullOrEmpty( stat.Series ) && _statChartSeries.ContainsKey( stat.Series ) )
            {
                var series = _statChartSeries[ stat.Series ];
                series.Points.AddXY( _statChartXPosCounter++, stat.CounterValue );
                while ( series.Points.Count > MaxPoints ) series.Points.RemoveAt( 0 );
                SetChartBoundaries(_statChart, _statChartSeries, _statChartXPosCounter );
            }
        }

        private void HandleStatPaused( SeriesStat stat )
        {
            if ( !string.IsNullOrEmpty( stat.Series ) && _statChartSeries.ContainsKey( stat.Series ) )
            {
                var series = _statChartSeries[ stat.Series ];
                series.Points.AddXY( _statChartXPosCounter++, 0.0d ); 
                while ( series.Points.Count > MaxPoints ) series.Points.RemoveAt( 0 );
                SetChartBoundaries( _statChart, _statChartSeries, _statChartXPosCounter );
            }
        }
        
        #endregion
    }
}

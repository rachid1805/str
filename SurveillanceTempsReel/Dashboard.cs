﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Windows.Forms;
using Akka.Actor;
using Common.Entities;
using Common.Helpers;
using SurveillanceTempsReel.Actors;

namespace SurveillanceTempsReel
{
    public partial class Dashboard : Form
    {
        #region Fields and constants
        
        public static readonly string AppName = "Système de surveillance médicale";

        public static readonly string HospitalIDColumnName = "ID";
        public static readonly string HospitalNameColumnName = "Name";
        public static readonly string EstimatedTimeToSeeADoctorColumnName = "EstimatedTimeToSeeADoctor";
        public static readonly string AvgTimeToSeeADoctorColumnName = "AvgTimeToSeeADoctor";
        public static readonly string AvgAppointmentDurationColumnName = "AvgAppointmentDuration";
        public static readonly string InfluenzaStatColumnName = "Influenza";

        private IActorRef _dashboardActor;

        private IActorRef _commanderActor;

        #endregion

        public Dashboard()
        {
            InitializeComponent();
        }

        #region Handlers

        private void Dashboard_Load( object sender, EventArgs e )
        {
            try
            {
                btnPauseResume.Enabled = false;
                this.Text = AppName;
                
                var hospitals = MedWatchDAL.FindHospitals();
               
                var hospitalStatsDataTable = CreateDataTableForHospitalStats( hospitals );
                dataGridView.DataSource = hospitalStatsDataTable;

                InitPerformanceCounters( hospitals );
                
                // initialization de l'acteur pour le tableau de bord
                _dashboardActor = Program.MediWatchActors.ActorOf( Props.Create( () => new DashboardActor( hospitalStatsDataTable, btnPauseResume ) ), ActorPaths.DashboardActorName );
               
                // initialization du commander
                _commanderActor = Program.MediWatchActors.ActorOf( Props.Create( () => new MediWatchCommanderActor( hospitals, _dashboardActor ) ), ActorPaths.MediWatchCommanderActorName );

                btnPauseResume.Enabled = true;
            }
            catch ( Exception ex )
            {
                MessageBox.Show( $"While loading up dashboard: {ex.ToString()}", AppName, MessageBoxButtons.OK, MessageBoxIcon.Error );
            }
        }

        private void Dashboard_FormClosing( object sender, FormClosingEventArgs e )
        {
            // arrêt du système d'acteurs
            _dashboardActor.Tell( PoisonPill.Instance );
            Program.MediWatchActors.Terminate();
        }

        private void btnPauseResume_Click( object sender, EventArgs e )
        {
            _dashboardActor.Tell( new DashboardActor.TogglePause() );
        }
        
        #endregion

        #region Private methods
        
        private void InitPerformanceCounters( IEnumerable<Hospital> hospitals )
        {
            if ( PerformanceCounterCategory.Exists( PerformanceCounterHelper.MainCategory ) )
            {
                PerformanceCounterCategory.Delete( PerformanceCounterHelper.MainCategory );
            }

            var dc = new CounterCreationDataCollection();

            foreach ( var h in hospitals )
            {
                dc.Add( new CounterCreationData( PerformanceCounterHelper.GetPerformanceCounterName( StatisticType.AvgTimeToSeeADoctor, h.Id ),
                    PerformanceCounterHelper.CounterAvgTimeToSeeADoctor, PerformanceCounterType.NumberOfItems64) );

                dc.Add(new CounterCreationData(PerformanceCounterHelper.GetPerformanceCounterName(StatisticType.Illness, h.Id),
                    PerformanceCounterHelper.CounterPerDisease, PerformanceCounterType.NumberOfItems64));
                
                dc.Add( new CounterCreationData( PerformanceCounterHelper.GetPerformanceCounterName( StatisticType.AvgAppointmentDuration, h.Id ),
                    PerformanceCounterHelper.CounterAvgAppointmentDuration, PerformanceCounterType.NumberOfItems64 ) );

                dc.Add(new CounterCreationData(PerformanceCounterHelper.GetPerformanceCounterName(StatisticType.EstimatedTimeToSeeADoctor, h.Id),
                    PerformanceCounterHelper.CounterEstimatedTimeToSeeADoctor, PerformanceCounterType.NumberOfItems64));

                dc.Add( new CounterCreationData( $"(H{h.Id}) Messages par seconde pour {PerformanceCounterHelper.CounterAvgTimeToSeeADoctor}", 
                    "Actor messages per second", PerformanceCounterType.RateOfCountsPerSecond64 ) );
            }
            
            PerformanceCounterCategory.Create( PerformanceCounterHelper.MainCategory, "Catégorie pour Système de surveillance médicale", PerformanceCounterCategoryType.SingleInstance, dc );
        }

        private DataTable CreateDataTableForHospitalStats( IEnumerable<Hospital> hospitals )
        {
            var dataTable = new DataTable();

            var hospitalIDColumn = new DataColumn( HospitalIDColumnName, typeof( int ) );
            hospitalIDColumn.Caption = "No. hôpital";
            var hospitalNameColumn = new DataColumn( HospitalNameColumnName, typeof( string ) );
            hospitalNameColumn.Caption = "Nom";
            var estimatedTimeToSeeADoctorColumn = new DataColumn( EstimatedTimeToSeeADoctorColumnName, typeof( string ) );
            estimatedTimeToSeeADoctorColumn.Caption = "Temps estimé pour voir un médecin";
            var avgTimeToSeeADoctorColumn = new DataColumn( AvgTimeToSeeADoctorColumnName, typeof( string ) );
            avgTimeToSeeADoctorColumn.Caption = "Temps moyen pour voir un médecin";
            var avgAppointmentDurationColumn = new DataColumn( AvgAppointmentDurationColumnName, typeof( string ) );
            avgAppointmentDurationColumn.Caption = "Durée moyenne d'un rendez-vous";
            var influenzaStatColumn = new DataColumn( InfluenzaStatColumnName, typeof( string ) );
            influenzaStatColumn.Caption = "Grippe";

            dataTable.Columns.AddRange( new DataColumn[] { hospitalIDColumn, hospitalNameColumn, estimatedTimeToSeeADoctorColumn,
                avgTimeToSeeADoctorColumn, avgAppointmentDurationColumn, influenzaStatColumn } );
            
            foreach ( var h in hospitals )
            {
                var row = dataTable.NewRow();
                row[ HospitalIDColumnName ] = h.Id;
                row[ HospitalNameColumnName ] = h.Name;
                row[ EstimatedTimeToSeeADoctorColumnName ] = 0.ToString("F2");
                row[ AvgTimeToSeeADoctorColumnName ] = 0.ToString( "F2" );
                row[ AvgAppointmentDurationColumnName ] = 0.ToString( "F2" );
                row[ InfluenzaStatColumnName ] = 0.ToString( "F2" );
                dataTable.Rows.Add( row );
            }

            return dataTable;
        }

        #endregion
        
    }
}

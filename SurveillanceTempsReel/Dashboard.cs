using System;
using System.Collections.Generic;
using System.Linq;
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

        public const string AppName = "Système de surveillance médicale";

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
            this.Text = AppName;

            // TODO JS peut-etre laisser exception se propager jusqu'au client?
            var hospitals = MedWatchDAL.FindHospitals();

            LoadHospitalComboBox(hospitals);

            // initialization de l'acteur pour le tableau de bord
            _dashboardActor = Program.MediWatchActors.ActorOf( Props.Create( () => new DashboardActor( statChart, btnPause ) ), ActorPaths.DashboardActorName );
            _dashboardActor.Tell( new DashboardActor.InitializeStatChart( null ) );

            // initialization du commander
            _commanderActor = Program.MediWatchActors.ActorOf( Props.Create( () => new MediWatchCommanderActor( hospitals, _dashboardActor ) ) );
        }

        private void Dashboard_FormClosing( object sender, FormClosingEventArgs e )
        {
            // arrêt du système d'acteurs
            _dashboardActor.Tell( PoisonPill.Instance );
            Program.MediWatchActors.Terminate();
        }

        private void btnPause_Click( object sender, EventArgs e )
        {
            // TODO
        }

        private void comboHospitals_SelectedIndexChanged( object sender, EventArgs e )
        {
            var selectedHospital = comboHospitals.SelectedItem as Hospital;
            //if ( selectedHospital != null )
            //    // TODO
        }

        #endregion

        #region Private methods

        private void LoadHospitalComboBox( IEnumerable<Hospital> hospitals )
        {
            // TODO temp load only 1 hospital for testing
            comboHospitals.Items.Add( hospitals.First() );

            //foreach (var h in hospitals)
            //{
            //    comboHospitals.Items.Add( h );
            //}

            if ( comboHospitals.Items.Count > 0 )
                comboHospitals.SelectedIndex = 0;
        }

        #endregion
    }
}

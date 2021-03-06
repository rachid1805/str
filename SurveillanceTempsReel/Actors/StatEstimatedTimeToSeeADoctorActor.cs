﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Akka.Actor;
using Akka.Event;
using Common.Entities;

namespace SurveillanceTempsReel.Actors
{
    /// <summary>
    /// Acteur responsable d'estimer le temps qui reste pour voir un médecin pour un hôpital en particulier.
    /// </summary>
    public class StatEstimatedTimeToSeeADoctorActor : ReceiveActor
    {
        #region Fields and constants

        private readonly Hospital _hospital;
        
        private readonly HashSet<IActorRef> _subscriptions;

        private ICancelable _cancelPublishing;

        private PerformanceCounter _counter;

        private Dictionary<DiseasePriority, Dictionary<int, RegisterPatient>> _patients;
        private Dictionary<int, BeginAppointmentWithDoctor> _doctors;
        private double _avgDuration;
        private long _statCount;
        private List<long> _remainingTimeToSeeADoctor;

        private readonly ILoggingAdapter _log = Context.GetLogger();

        #endregion

        #region Constructors

        public StatEstimatedTimeToSeeADoctorActor( Hospital hospital )
        {
            _hospital = hospital;
            _subscriptions = new HashSet<IActorRef>();
            
            Processing();
        }

        #endregion

        #region Actor lifecycle methods

        protected override void PreStart()
        {
            _counter = new PerformanceCounter( PerformanceCounterHelper.MainCategory, PerformanceCounterHelper.GetPerformanceCounterName( StatisticType.EstimatedTimeToSeeADoctor, _hospital.Id ), false );
            _counter.RawValue = 0;

            _patients = new Dictionary<DiseasePriority, Dictionary<int, RegisterPatient>>();
            for (var diseasePriority = DiseasePriority.VeryHigh; diseasePriority < DiseasePriority.Invalid; ++diseasePriority)
            {
                _patients.Add(diseasePriority, new Dictionary<int, RegisterPatient>());
            }

            _doctors = new Dictionary<int, BeginAppointmentWithDoctor>();
            _avgDuration = 0.0d;
            _statCount = 0;
            _remainingTimeToSeeADoctor = new List<long>(_hospital.AssignedDoctors);

            _cancelPublishing = ScheduleGatherStatsTask();
        }

        protected override void PostStop()
        {
            try
            {
                _cancelPublishing.Cancel( false );
                _counter.RawValue = 0;
                _counter.Dispose();
            }
            catch
            {
                // oh well... on ne se préoccupe pas des autres exceptions
            }
            finally
            {
                base.PostStop();
            }
        }

        #endregion

        #region Private methods

        private void Processing()
        {
            Receive<GatherStats>( gs =>
            {
                var stat = new Stat( _hospital.Id, StatisticType.EstimatedTimeToSeeADoctor, _avgDuration );

                foreach ( var sub in _subscriptions )
                    sub.Tell( stat );
            } );

            Receive<SubscribeStatistic>( sc =>
            {
                _subscriptions.Add( sc.Subscriber );
            } );

            Receive<UnsubscribeStatistic>( uc =>
            {
                _subscriptions.Remove( uc.Subscriber );
            } );

            Receive<RegisterPatient>( rp =>   
            {
                var sw = Stopwatch.StartNew();

                _patients[rp.Disease.Priority].Add( rp.PatientId, rp );

                if (_doctors.Count == 0)
                {
                    // Aucun docteur n'est encore enregistré, le temsp d'attente est indéfini
                    return;
                }

                // Trier le temps d'occupation de tous les médecins
                _remainingTimeToSeeADoctor.Clear();
                foreach (var doctor in _doctors)
                {
                    var patient = doctor.Value;
                    if (patient == null)
                    {
                        // Médecin sans patient
                        _remainingTimeToSeeADoctor.Add(0);
                    }
                    else
                    {
                        var diseaseInCharge = patient.Disease;
                        var requiredTimeForDisease = ConvertTimeToMilliSec(diseaseInCharge.RequiredTime, diseaseInCharge.TimeUnit);
                        var elapsedTime = (DateTime.Now - patient.StartTime).TotalMilliseconds;
                        if (elapsedTime > requiredTimeForDisease)
                        {
                            // Le médecin a terminé avec ce patient
                            _remainingTimeToSeeADoctor.Add(0);
                        }
                        else
                        {
                            // Le temps qui reste au médecin avant de se libérer
                            _remainingTimeToSeeADoctor.Add((long) (requiredTimeForDisease - elapsedTime));
                        }
                    }
                }

                // Estimer le temps d'attente de chaque patient
                for (var diseasePriority = DiseasePriority.VeryHigh; diseasePriority < DiseasePriority.Invalid; ++diseasePriority)
                {
                    foreach (var patient in _patients[diseasePriority])
                    {
                        // Trier la liste de temps d'ocuppation des médecins
                        _remainingTimeToSeeADoctor.Sort();

                        // Le premier médecin qui va se libérer
                        var waitingTime = _remainingTimeToSeeADoctor[0];
                        _avgDuration = ((_avgDuration * _statCount) + waitingTime) / (++_statCount);
                        _counter.RawValue = (long)_avgDuration;

                        // Rajouter à ce médecin le temps d'occupation avec ce nouveau patient
                        _remainingTimeToSeeADoctor[0] += ConvertTimeToMilliSec(patient.Value.Disease.RequiredTime, patient.Value.Disease.TimeUnit);
                    }
                }

                _log.Info( $"(H{_hospital.Id}) RegisterPatient for patient ID={rp.PatientId} took {sw.ElapsedTicks} ticks" );
            } );

            Receive<BeginAppointmentWithDoctor>(bawd =>
            {
                var sw = Stopwatch.StartNew();

                if ((_doctors.Count >= _hospital.AssignedDoctors) && !_doctors.ContainsKey(bawd.DoctorId))
                {
                    // Un nouveau quart de travail :)
                    _doctors.Clear();
                    for (var diseasePriority = DiseasePriority.VeryHigh; diseasePriority < DiseasePriority.Invalid; ++diseasePriority)
                    {
                        _patients[diseasePriority].Clear();
                    }
                }
                _doctors[bawd.DoctorId] = bawd;

                RemovePatient(bawd.PatientId);

                _log.Info( $"(H{_hospital.Id}) BeginAppointmentWithDoctor for patient ID={bawd.PatientId} took {sw.ElapsedTicks} ticks" );
            } );

            Receive<UnregisterPatient>(urp =>
            {
                var sw = Stopwatch.StartNew();

                foreach (var doctor in _doctors.Where(doctor => (doctor.Value != null) && doctor.Value.PatientId.Equals(urp.PatientId)))
                {
                    _doctors[doctor.Key] = null;
                    break;
                }

                RemovePatient(urp.PatientId);

                _log.Info( $"(H{_hospital.Id}) UnregisterPatient for patient ID={urp.PatientId} took {sw.ElapsedTicks} ticks" );
            } );
        }

        /// <summary>
        /// Cédule une tâche pour nous envoyer régulièrement un message
        /// pour publier la statistique à jour de l'acteur
        /// </summary>
        /// <returns></returns>
        private ICancelable ScheduleGatherStatsTask()
        {
            var cancellation = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(
                TimeSpan.FromMilliseconds( 2000 ),           
                TimeSpan.FromMilliseconds( 1000 ),
                Self,
                new GatherStats(),
                Self );

            return cancellation;
        }

        private static long ConvertTimeToMilliSec(int time, RequiredTimeUnit timeUnit)
        {
            long timeMs;

            switch (timeUnit)
            {
                case RequiredTimeUnit.Min:
                    timeMs = time * 1000 * 60;
                    break;
                case RequiredTimeUnit.Sec:
                    timeMs = time * 1000;
                    break;
                case RequiredTimeUnit.MilliSec:
                    timeMs = time;
                    break;
                default:
                    throw new ApplicationException("Unsupported unit");
            }

            return timeMs;
        }

        private void RemovePatient(int patientId)
        {
            for (var diseasePriority = DiseasePriority.VeryHigh; diseasePriority < DiseasePriority.Invalid; ++diseasePriority)
            {
                var patientList = _patients[diseasePriority];
                if (patientList.ContainsKey(patientId))
                {
                    patientList.Remove(patientId);
                    break;
                }
            }
        }
        
        #endregion
    }
}

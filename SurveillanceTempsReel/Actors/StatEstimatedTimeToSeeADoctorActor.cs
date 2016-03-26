using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Akka.Actor;
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

        private readonly IActorRef _hospitalCoordinator;

        private readonly HashSet<IActorRef> _subscriptions;

        private readonly ICancelable _cancelPublishing;

        private PerformanceCounter _counter;
        private PerformanceCounter _baseCounter;

        private Dictionary<DiseasePriority, Dictionary<int, RegisterPatient>> _patients;
        private Dictionary<int, BeginAppointmentWithDoctor> _doctors;

        #endregion

        public StatEstimatedTimeToSeeADoctorActor( Hospital hospital, IActorRef hospitalCoordinator )
        {
            _hospital = hospital;
            _hospitalCoordinator = hospitalCoordinator;
            _subscriptions = new HashSet<IActorRef>();
            _cancelPublishing = new Cancelable( Context.System.Scheduler );
            
            Processing();
        }

        #region Actor lifecycle methods

        protected override void PreStart()
        {
            _counter = new PerformanceCounter( PerformanceCounterHelper.MainCategory, PerformanceCounterHelper.GetPerformanceCounterName( StatisticType.EstimatedTimeToSeeADoctor, _hospital.Id ), false );
            _baseCounter = new PerformanceCounter( PerformanceCounterHelper.MainCategory, PerformanceCounterHelper.GetPerformanceBaseCounterName( StatisticType.EstimatedTimeToSeeADoctor, _hospital.Id ), false );
            _counter.RawValue = 0;
            _baseCounter.RawValue = 0;

            _patients = new Dictionary<DiseasePriority, Dictionary<int, RegisterPatient>>();
            for (var diseasePriority = DiseasePriority.VeryHigh; diseasePriority < DiseasePriority.Invalid; ++diseasePriority)
            {
                _patients.Add(diseasePriority, new Dictionary<int, RegisterPatient>());
            }

            _doctors = new Dictionary<int, BeginAppointmentWithDoctor>();
        }

        protected override void PostStop()
        {
            try
            {
                _cancelPublishing.Cancel( false ); 
                _counter.Dispose();
                _baseCounter.Dispose();
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

        private void Processing()
        {
            Receive<GatherStats>( bof =>
            {
                var stat = new Stat( StatisticType.EstimatedTimeToSeeADoctor.ToString(), _counter.NextValue() );

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
                _patients[rp.Disease.Priority].Add( rp.PatientId, rp );
                var patientsNumber = _patients.Sum(patient => patient.Value.Count);
                var estimatedWaitingTime = new List<long>(patientsNumber);

                if (_doctors.Count > 0)
                {
                    // Trouver le temps le plus court qui reste pour qu'un médecin se libère
                    var remainingTime = long.MaxValue;
                    foreach (var doctor in _doctors)
                    {
                        if (doctor.Value == null)
                        {
                            remainingTime = 0;
                            break;
                        }
                        var diseaseInCharge = doctor.Value.Disease;
                        var requiredTimeForDisease = ConvertTimeToTicks(diseaseInCharge.RequiredTime, diseaseInCharge.TimeUnit);
                        var elapsedTime = (DateTime.Now - doctor.Value.StartTime).Ticks;
                        if (elapsedTime > requiredTimeForDisease)
                        {
                            remainingTime = 0;
                            break;
                        }
                        if (remainingTime > (requiredTimeForDisease - elapsedTime))
                        {
                            remainingTime = requiredTimeForDisease - elapsedTime;
                        }
                    }

                    // Calculer le temps estimé pour voir un médecin
                    if (remainingTime != long.MaxValue)
                    {
                        
                    }
                }
            } );

            Receive<BeginAppointmentWithDoctor>(bawd =>
            {
                if (_doctors.ContainsKey(bawd.DoctorId))
                {
                    _doctors[bawd.DoctorId] = bawd;
                }
                else
                {
                    _doctors.Add(bawd.DoctorId, bawd);
                }

                foreach (var patientList in _patients.Values)
                {
                    RegisterPatient registredPatient;
                    if (!patientList.TryGetValue(bawd.PatientId, out registredPatient)) continue;
                    patientList.Remove(bawd.PatientId);
                    break;
                }
            } );

            Receive<UnregisterPatient>(urp =>
            {
                foreach (var doctor in _doctors.Where(doctor => doctor.Value.PatientId.Equals(urp.PatientId)))
                {
                    _doctors[doctor.Key] = null;
                    break;
                }
            });
        }

        private static long ConvertTimeToTicks(int time, RequiredTimeUnit timeUnit)
        {
            // 10000 ticks par ms
            long ticks = time * 10000;

            switch (timeUnit)
            {
                case RequiredTimeUnit.Min:
                    ticks = ticks * 1000 * 60;
                    break;
                case RequiredTimeUnit.Sec:
                    ticks = ticks * 1000;
                    break;
                case RequiredTimeUnit.MilliSec:
                    break;
                default:
                    throw new ApplicationException("Unsupported unit");
            }

            return ticks;
        }
    }
}

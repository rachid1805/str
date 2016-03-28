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
        
        private readonly HashSet<IActorRef> _subscriptions;

        private readonly ICancelable _cancelPublishing;

        private PerformanceCounter _counter;
        //private PerformanceCounter _baseCounter;

        private Dictionary<DiseasePriority, Dictionary<int, RegisterPatient>> _patients;
        private Dictionary<int, BeginAppointmentWithDoctor> _doctors;
        private double _avgDuration;
        private long _statCount;

        #endregion

        public StatEstimatedTimeToSeeADoctorActor( Hospital hospital )
        {
            _hospital = hospital;
            _subscriptions = new HashSet<IActorRef>();
            _cancelPublishing = new Cancelable( Context.System.Scheduler );
            
            Processing();
        }

        #region Actor lifecycle methods

        protected override void PreStart()
        {
            _counter = new PerformanceCounter( PerformanceCounterHelper.MainCategory, PerformanceCounterHelper.GetPerformanceCounterName( StatisticType.EstimatedTimeToSeeADoctor, _hospital.Id ), false );
            //_baseCounter = new PerformanceCounter( PerformanceCounterHelper.MainCategory, PerformanceCounterHelper.GetPerformanceBaseCounterName( StatisticType.EstimatedTimeToSeeADoctor, _hospital.Id ), false );
            _counter.RawValue = 0;
            //_baseCounter.RawValue = 0;

            _patients = new Dictionary<DiseasePriority, Dictionary<int, RegisterPatient>>();
            for (var diseasePriority = DiseasePriority.VeryHigh; diseasePriority < DiseasePriority.Invalid; ++diseasePriority)
            {
                _patients.Add(diseasePriority, new Dictionary<int, RegisterPatient>());
            }

            _doctors = new Dictionary<int, BeginAppointmentWithDoctor>();
            _avgDuration = 0.0d;
            _statCount = 0;
        }

        protected override void PostStop()
        {
            try
            {
                var average = _avgDuration;
                _cancelPublishing.Cancel( false );
                _counter.RawValue = 0;
                _counter.Dispose();
                //_baseCounter.Dispose();
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

                var stopwatch = Stopwatch.StartNew();

                if (_doctors.Count == 0)
                {
                    // Aucun docteur n'est encore enregistré, le temsp d'attente est indéfini
                    return;
                }

                // Trier le temps d'occupation de tous les médecins
                var remainingTimeDoctor = new List<long>(_doctors.Count);
                foreach (var doctor in _doctors)
                {
                    var patient = doctor.Value;
                    if (patient == null)
                    {
                        // Médecin sans patient
                        remainingTimeDoctor.Add(0);
                    }
                    else
                    {
                        var diseaseInCharge = patient.Disease;
                        var requiredTimeForDisease = ConvertTimeToMilliSec(diseaseInCharge.RequiredTime, diseaseInCharge.TimeUnit);
                        var elapsedTime = (DateTime.Now - patient.StartTime).Ticks;
                        if (elapsedTime > requiredTimeForDisease)
                        {
                            // Le médecin a terminé avec ce patient
                            remainingTimeDoctor.Add(0);
                        }
                        else
                        {
                            // Le temps qui reste au médecin avant de se libérer
                            remainingTimeDoctor.Add(requiredTimeForDisease - elapsedTime);
                        }
                    }
                }

                // Estimer le temps d'attente de chaque patient
                for (var diseasePriority = DiseasePriority.VeryHigh; diseasePriority < DiseasePriority.Invalid; ++diseasePriority)
                {
                    foreach (var patient in _patients[diseasePriority])
                    {
                        // Trier la liste de temps d'ocuppation des médecins
                        remainingTimeDoctor.Sort();

                        // Le premier médecin qui va se libérer
                        var waitingTime = remainingTimeDoctor[0];
                        _avgDuration = ((_avgDuration * _statCount) + waitingTime) / (++_statCount);
                        _counter.RawValue = (long)_avgDuration;
                        //_counter.IncrementBy(remainingTimeDoctor[0]);
                        //_baseCounter.Increment();

                        // Rajouter à ce médecin le temps d'occupation avec ce nouveau patient
                        remainingTimeDoctor[0] += ConvertTimeToMilliSec(patient.Value.Disease.RequiredTime, patient.Value.Disease.TimeUnit);
                    }
                }

                Console.WriteLine("StatEstimatedTimeToSeeADoctorActor.Processing: Elapsed time = {0} ms. Average time = {1}", stopwatch.ElapsedMilliseconds, _avgDuration);
                stopwatch.Stop();
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

                for (var diseasePriority = DiseasePriority.VeryHigh; diseasePriority < DiseasePriority.Invalid; ++diseasePriority)
                {
                    var patientList = _patients[diseasePriority];
                    if (patientList.ContainsKey(bawd.PatientId))
                    {
                        patientList.Remove(bawd.PatientId);
                        break;
                    }
                }
            } );

            Receive<UnregisterPatient>(urp =>
            {
                //foreach (var doctor in _doctors.Where(doctor => doctor.Value.PatientId.Equals(urp.PatientId)))
                foreach (var doctor in _doctors)
                {
                    if ((doctor.Value != null) && doctor.Value.PatientId.Equals(urp.PatientId))
                    {
                        _doctors[doctor.Key] = null;
                        break;
                    }
                }
            });
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
    }
}

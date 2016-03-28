using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Akka.Actor;
using Common.Entities;

namespace SurveillanceTempsReel.Actors
{
    public class StatAvgAppointmentDurationActor : ReceiveActor
    {
        #region Fields and constants

        private readonly Hospital _hospital;
        
        private readonly HashSet<IActorRef> _subscriptions;

        private readonly ICancelable _cancelPublishing;

        private PerformanceCounter _counter;
        //private PerformanceCounter _baseCounter;

        private Dictionary<int, DateTime> _patients;
        private double _avgDuration;
        private long _statCount;

        #endregion

        public StatAvgAppointmentDurationActor( Hospital hospital )
        {
            _hospital = hospital;
            _subscriptions = new HashSet<IActorRef>();
            _cancelPublishing = new Cancelable( Context.System.Scheduler );

            Processing();
        }

        #region Actor lifecycle methods

        protected override void PreStart()
        {
            _counter = new PerformanceCounter( PerformanceCounterHelper.MainCategory, PerformanceCounterHelper.GetPerformanceCounterName( StatisticType.AvgAppointmentDuration, _hospital.Id ), false);
            //_baseCounter = new PerformanceCounter(PerformanceCounterHelper.MainCategory, PerformanceCounterHelper.GetPerformanceBaseCounterName(StatisticType.AvgAppointmentDuration, _hospital.Id), false);
            _counter.RawValue = 0;

            _patients = new Dictionary<int, DateTime>();
            _avgDuration = 0.0d;
            _statCount = 0;
        }

        protected override void PostStop()
        {
            try
            {
                var moyenne = _avgDuration;
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

        private void Processing()
        {
            // TODO : if we use PerfMon, remove this
            Receive<GatherStats>(bof =>
            {
                var stat = new Stat(StatisticType.AvgTimeToSeeADoctor.ToString(), _counter.NextValue());
                //var stat = new Stat(StatisticType.AvgTimeToSeeADoctor.ToString(), _avgMinutesToSeeADoctor);

                foreach (var sub in _subscriptions)
                    sub.Tell(stat);
            });

            Receive<SubscribeStatistic>(sc =>
            {
                _subscriptions.Add(sc.Subscriber);
            });

            Receive<UnsubscribeStatistic>(uc =>
            {
                _subscriptions.Remove(uc.Subscriber);
            });

            Receive<BeginAppointmentWithDoctor>(bawd =>
            {
                _patients.Add(bawd.PatientId, bawd.StartTime);
            });

            Receive<UnregisterPatient>(urp =>
            {
                DateTime startTime;
                if (_patients.TryGetValue(urp.PatientId, out startTime))
                {
                    var duration = (long) (urp.LeavingTime - startTime).TotalMilliseconds;
                    _avgDuration = ((_avgDuration * _statCount) + duration) / (++_statCount);
                    _counter.RawValue = (long) _avgDuration;
                    //_baseCounter.Increment();

                    _patients.Remove(urp.PatientId);
                }
            });
        }
    }
}

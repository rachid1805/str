using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Akka.Actor;


namespace SurveillanceTempsReel.Actors 
{
    public class DashboardActor : ReceiveActor, IWithUnboundedStash
    {
        public IStash Stash { get; set; }

    }
}

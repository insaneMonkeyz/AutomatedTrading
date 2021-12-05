using System;
using System.Text;

namespace Strategies
{
    public class StrategiesSupervisor
    {
        public Security Security { get; }
        public Security Hedger { get; set; }
        public PriceProvider PriceProvider { get; set; }

        public bool IsEnabled { get; set; }

        public FrontrunningStrategy FrontrunningStrategy { get; }
        public QuotingStrategy QuotingStrategy { get; }
        public GrabStrategy GrabStrategy { get; }
    }
}

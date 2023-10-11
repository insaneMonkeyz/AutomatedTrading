using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Broker
{
    [Serializable]
    public class BrokerConfiguration
    {
        public Guid HostId { get; set; }

        public static BrokerConfiguration CreateDefault()
        {
            return new()
            {
                HostId = Guid.NewGuid(),
            };
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradingConcepts;

namespace Quik.EntityProviders.Notification
{
    internal interface INotifiableEntity : INotifyEntityUpdated
    {
        void NotifyUpdated();
    }
}

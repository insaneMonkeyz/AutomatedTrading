using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppComponents.Delegates
{
    public delegate void FeedSubscriber<TFeed>(Guid senderId, TFeed receiver);
}

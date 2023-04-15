using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradingConcepts;
using Quik.EntityProviders.QuikApiWrappers;
using Quik.EntityProviders.Notification;

namespace Quik.Entities
{
    internal class SecurityBalance : ISecurityBalance, INotifiableEntity
    {
        private string _firmId;
        private string _accountId;

        public Decimal5? Collateral { get; set; }
        public ISecurity Security { get; init; }
        public long Amount { get; set; }
        public string FirmId 
        { 
            get => _firmId;
            init => _firmId = value ?? throw new ArgumentNullException(nameof(value)); 
        }
        public string Account 
        { 
            get => _accountId; 
            init => _accountId = value ?? throw new ArgumentNullException(nameof(value));
        }

        public event Action Updated = delegate { };

        public void NotifyUpdated() => Updated();

        public SecurityBalance(ISecurity security)
        {
            Security = security ?? throw new ArgumentNullException(nameof(security));
        }

        public override string ToString() => $"{Security.Ticker} {Amount}";
    }
}

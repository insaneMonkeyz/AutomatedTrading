using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DecisionMakingService.Strategies
{
    internal abstract class Strategy
    {
        public virtual Guid Id { get; private set; }

        public override string ToString()
        {
            return $"{this} {Id}";
        }
    }
}

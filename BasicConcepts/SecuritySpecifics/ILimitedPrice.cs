using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicConcepts.SecuritySpecifics
{
    public interface ILimitedPrice
    {
        public Decimal5? UpperPriceLimit { get; }
        public Decimal5? LowerPriceLimit { get; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingConcepts
{
    public interface ISecurityBalance
    {
        ISecurity Security { get; }
        long Amount { get; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicConcepts
{
    public interface ISecurityBalance
    {
        ISecurity Security { get; }
        int Amount { get; }
    }
}

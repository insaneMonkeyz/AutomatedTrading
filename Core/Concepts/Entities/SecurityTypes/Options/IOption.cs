using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Concepts.Entities.SecurityTypes.Options
{
    public interface IOption : ISecurity, IExpiringContract
    {
        Decimal5 Strike { get; }
        ISecurity Underlying { get; }
        OptionTypes OptionType { get; }
    }
}

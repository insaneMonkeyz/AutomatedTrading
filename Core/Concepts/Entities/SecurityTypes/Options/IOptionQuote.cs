using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Concepts.Entities.SecurityTypes.Options
{
    internal interface IOptionQuote : IQuote
    {
        IOption ParametersHolder { get; }
        double Volatility { get; }
    }
}

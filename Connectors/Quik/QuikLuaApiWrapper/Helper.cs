using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quik.Entities;
using TradingConcepts;

namespace Quik
{
    internal static class Helper
    {
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static Security CastToMoexSecurity(ISecurity security)
        {
            return security switch
            {
                Security moexSecurity => moexSecurity,
                null => throw new ArgumentNullException(nameof(security)),
                   _ => throw new ArgumentException($"Security {security} does not belong to MOEX and therefore cannot be used in orders for it.")
            };
        }
    }
}

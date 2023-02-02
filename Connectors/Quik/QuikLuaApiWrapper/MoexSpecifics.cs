using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quik
{
    internal static class MoexSpecifics
    {
        public const string STOCK_CLASS_CODE = "TQBR";
        public const string OPTIONS_CLASS_CODE = "SPBOPT";
        public const string FUTURES_CLASS_CODE = "SPBFUT";
        public const string CALENDAR_SPREADS_CLASS_CODE = "FUTSPREAD";

        public static readonly string[] AllowedClassCodes = new[]
        {
            STOCK_CLASS_CODE,
            OPTIONS_CLASS_CODE,
            CALENDAR_SPREADS_CLASS_CODE,
            FUTURES_CLASS_CODE
        };

        public static readonly TimeSpan CommonExpiryTime = TimeSpan.FromHours(19);
        public static readonly TimeSpan MoscowUtcOffset = TimeSpan.FromHours(3);
    }
}

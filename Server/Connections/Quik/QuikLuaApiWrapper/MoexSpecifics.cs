using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quik.EntityProviders.QuikApiWrappers;
using TradingConcepts;
using TradingConcepts.SecuritySpecifics.Options;
using TradingConcepts.SecuritySpecifics;

namespace Quik
{
    public static class MoexSpecifics
    {
        internal const string STOCK_CLASS_CODE = "TQBR";
        internal const string OPTIONS_CLASS_CODE = "SPBOPT";
        internal const string FUTURES_CLASS_CODE = "SPBFUT";
        internal const string CALENDAR_SPREADS_CLASS_CODE = "FUTSPREAD";

        internal const string STOCK_SETTLEMENT_TAG = "EQTV";
        internal const string FOREX_SETTLEMENT_TAG = "USDR";
        internal const string FOREX_TOD_SETTLEMENT_TAG = "RTOD";
        internal const string FOREX_TOM_SETTLEMENT_TAG = "RTOM";

        internal const string SUR_CURRENCY = "SUR";
        internal const string RUB_CURRENCY = "RUB";
        internal const string USD_CURRENCY = "USD";

        public static readonly Decimal5 DefaultTradingSize = 1;
        public static readonly Guid MoexExchangeId = new("2B9F0D6D-62C9-46BC-8923-0C789DB826C2");

        public static readonly TimeSpan CommonExpiryTime = new (18, 45, 0);
        public static readonly TimeSpan MoscowUtcOffset = TimeSpan.FromHours(3);
        public static readonly TimeZoneInfo MoscowTimeZone;

        static MoexSpecifics()
        {
            try
            {
                MoscowTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time");
            }
            catch (Exception)
            {
                MoscowTimeZone = TimeZoneInfo.GetSystemTimeZones().First(tz => tz.BaseUtcOffset == MoscowUtcOffset);
            }
        }

        public static readonly string[] AllowedClassCodes = new[]
        {
            STOCK_CLASS_CODE,
            OPTIONS_CLASS_CODE,
            CALENDAR_SPREADS_CLASS_CODE,
            FUTURES_CLASS_CODE
        };

        public static readonly IReadOnlyDictionary<Type, string> SecurityTypesToClassCodes = new Dictionary<Type, string>()
        {
            { typeof(IStock),   STOCK_CLASS_CODE },
            { typeof(IOption),  OPTIONS_CLASS_CODE },
            { typeof(IFutures), FUTURES_CLASS_CODE },
            { typeof(ICalendarSpread), CALENDAR_SPREADS_CLASS_CODE },
        };
    }
}

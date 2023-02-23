using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradingConcepts;

namespace Quik
{
    internal static class MoexSpecifics
    {
        public const string STOCK_CLASS_CODE = "TQBR";
        public const string OPTIONS_CLASS_CODE = "SPBOPT";
        public const string FUTURES_CLASS_CODE = "SPBFUT";
        public const string CALENDAR_SPREADS_CLASS_CODE = "FUTSPREAD";

        public const string STOCK_SETTLEMENT_TAG = "EQTV";
        public const string FOREX_SETTLEMENT_TAG = "USDR";
        public const string FOREX_TOD_SETTLEMENT_TAG = "RTOD";
        public const string FOREX_TOM_SETTLEMENT_TAG = "RTOM";

        public const string SUR_CURRENCY = "SUR";
        public const string RUB_CURRENCY = "RUB";
        public const string USD_CURRENCY = "USD";

        public static readonly string[] AllowedClassCodes = new[]
        {
            STOCK_CLASS_CODE,
            OPTIONS_CLASS_CODE,
            CALENDAR_SPREADS_CLASS_CODE,
            FUTURES_CLASS_CODE
        };

        public static readonly Decimal5 DefaultTradingSize = 1;
        public static readonly Guid MoexExchangeId = new("2B9F0D6D-62C9-46BC-8923-0C789DB826C2");

        public static readonly TimeSpan CommonExpiryTime = TimeSpan.FromHours(19);
        public static readonly TimeSpan MoscowUtcOffset = TimeSpan.FromHours(3);
    }
}

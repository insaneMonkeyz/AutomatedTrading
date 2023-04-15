using Quik.EntityProviders.Attributes;
using Quik.Lua;
using TradingConcepts;

namespace Quik.EntityProviders.QuikApiWrappers
{
    internal class DerivativesPositionsWrapper
    {
        public const string NAME = "futures_client_holding";
        public const string CALLBACK_METHOD = "OnFuturesClientHolding";
        public const string GET_METOD = "getFuturesHolding";

        public const string LIMIT_TYPE = "4";

        private const string ACCOUNT_ID = "trdaccid";
        private const string FIRM_ID = "firmid";
        private const string TICKER = "sec_code";
        private const string CURRENT_POS = "totalnet";
        private const string CURRENT_LONG = "todaybuy";
        private const string CURRENT_SHORT = "todaysell";
        private const string PENDING_LONG = "openbuys";
        private const string PENDING_SHORT = "opensells";
        private const string AVG_PRICE = "avrposnprice";
        private const string COLLATERAL = "varmargin";

        private static LuaWrap _stack;

        public static readonly object Lock = new();

        public static void Set(LuaWrap stack)
        {
            _stack = stack;
        }

        [QuikCallbackField(FIRM_ID)]
        public static string? FirmId
        {
            get => _stack.ReadRowValueString(FIRM_ID);
        }
        [QuikCallbackField(ACCOUNT_ID)]
        public static string? AccountId
        {
            get => _stack.ReadRowValueString(ACCOUNT_ID);
        }
        [QuikCallbackField(TICKER)]
        public static string? Ticker
        {
            get => _stack.ReadRowValueString(TICKER);
        }
        [QuikCallbackField(CURRENT_POS)]
        public static long? CurrentPos
        {
            get => _stack.TryFetchIntegerFromTable(CURRENT_POS, out long result) ? result : null;
        }
        [QuikCallbackField(COLLATERAL)]
        public static Decimal5? Collateral
        {
            get => _stack.TryFetchDecimalFromTable(COLLATERAL, out Decimal5 result) ? result : null;
        }
    }
}

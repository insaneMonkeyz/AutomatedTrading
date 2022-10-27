using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasicConcepts;
using QuikLuaApi;
using QuikLuaApi.QuikApi;
using QuikLuaApiWrapper.Extensions;

namespace QuikLuaApiWrapper.ApiWrapper.QuikApi
{
    internal static class QuikSecurity
    {
        public const string NAME = "securities";

        public const string CALLBACK_METHOD = "OnFuturesLimitChange";
        public const string GET_METOD = "getSecurityInfo";
        public const string GET_CLASSES_LIST_METHOD = "getClassesList";
        public const string GET_SECURITIES_OF_A_CLASS_METHOD = "getClassSecurities";

        public const string CALENDAR_SPREADS_CLASS_CODE = "FUTSPREAD";
        public const string OPTIONS_CLASS_CODE = "SPBOPT";
        public const string FUTURES_CLASS_CODE = "SPBFUT";
        public const string STOCK_CLASS_CODE = "TQBR";

        public const string PRICE_STEP_VALUE = "STEPPRICE";
        public const string OPTIONTYPE = "OPTIONTYPE";
        public const string UPPER_PRICE_LIMIT = "PRICEMAX";
        public const string LOWER_PRICE_LIMIT = "PRICEMIN";

        private const string TICKER = "code";
        private const string CLASS_CODE = "class_code";
        private const string CLASS_NAME = "class_name";
        private const string DESCRIPTION = "name";
        private const string CURRENCY = "face_unit";
        private const string MIN_PRICE_STEP = "min_price_step";
        private const string PRICE_SCALE = "scale";
        private const string CONTRACT_SIZE = "lot_size";
        private const string EXPIRY_DATE = "mat_date";
        private const string STRIKE = "option_strike";
        private const string UNDERLYING_CLASS_CODE = "base_active_classcode";
        private const string NEAR_TERM_LEG_CLASS_CODE = "sell_leg_classcode";
        private const string LONG_TERM_LEG_CLASS_CODE = "buy_leg_classcode";
        private const string NEAR_TERM_LEG_SEC_CODE = "sell_leg_seccode";
        private const string LONG_TERM_LEG_SEC_CODE = "buy_leg_seccode";
        private const string UNDERLYING_SEC_CODE = "base_active_seccode";
        private const string LONG_CURRCODE = "first_currcode";  //USD в USDRUB
        private const string SHORT_CURRCODE = "second_currcode";//RUB в USDRUB
        private const string QUOTE_CURRENCY = "step_price_currency";
        private const string TRADE_CURRENCY = "trade_currency";

        private static LuaState _stack;

        public static void Set(LuaState stack)
        {
            _stack = stack;
        }

        public static Currencies? Currency
        {
            get => _stack.ReadRowValueString(CURRENCY)?.CodeToCurrency();
        }
        public static string? Ticker
        {
            get => _stack.ReadRowValueString(TICKER);
        }
        public static string? ClassCode
        {
            get => _stack.ReadRowValueString(CLASS_CODE);
        }
        public static string? ClassName
        {
            get => _stack.ReadRowValueString(CLASS_NAME);
        }
        public static string? Description
        {
            get => _stack.ReadRowValueString(DESCRIPTION);
        }
        public static string? UnderlyingClassCode
        {
            get => _stack.ReadRowValueString(UNDERLYING_CLASS_CODE);
        }
        public static string? UnderlyingSecCode
        {
            get => _stack.ReadRowValueString(UNDERLYING_SEC_CODE);
        }
        public static string? NearTermLegClassCode
        {
            get => _stack.ReadRowValueString(NEAR_TERM_LEG_CLASS_CODE);
        }
        public static string? NearTermLegSecCode
        {
            get => _stack.ReadRowValueString(NEAR_TERM_LEG_SEC_CODE);
        }
        public static string? LongTermLegClassCode
        {
            get => _stack.ReadRowValueString(LONG_TERM_LEG_CLASS_CODE);
        }
        public static string? LongTermLegSecCode
        {
            get => _stack.ReadRowValueString(LONG_TERM_LEG_SEC_CODE);
        }
        public static string? LongCurrencyCode
        {
            get => _stack.ReadRowValueString(LONG_CURRCODE);
        }
        public static string? ShortCurrencyCode
        {
            get => _stack.ReadRowValueString(SHORT_CURRCODE);
        }
        public static string? QuoteCurrency
        {
            get => _stack.ReadRowValueString(QUOTE_CURRENCY);
        }
        public static string? TradeCurrency
        {
            get => _stack.ReadRowValueString(TRADE_CURRENCY);
        }
        public static long? PricePrecisionScale
        {
            get => _stack.TryFetchLongFromTable(PRICE_SCALE, out long result) ? result : null;
        }
        public static long? ContractSize
        {
            get => _stack.TryFetchLongFromTable(CONTRACT_SIZE, out long result) ? result : null;
        }
        public static Decimal5? MinPriceStep
        {
            get => _stack.TryFetchDecimalFromTable(MIN_PRICE_STEP, out Decimal5 result) ? result : null;
        }
        public static Decimal5? Strike
        {
            get => _stack.TryFetchDecimalFromTable(STRIKE, out Decimal5 result) ? result : null;
        }
        public static DateTimeOffset? Expiry
        {
            get
            {
                return _stack.TryFetchStringFromTable(EXPIRY_DATE, out string exp) &&
                        exp.TryConvertToMoexExpiry(out DateTimeOffset expiry)
                    ? expiry : null;
            }
        }
    }
}

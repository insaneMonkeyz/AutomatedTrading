using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuikLuaApi
{
    internal class QuikApi
    {
        public const string IS_CONNECTED_METHOD = "isConnected";
        public const string GET_ORDERBOOK_METHOD = "getQuoteLevel2";
        public const string GET_SECURITY_METHOD = "getSecurityInfo";
        public const string GET_CLASSES_LIST_METHOD = "getClassesList";
        public const string GET_SECURITIES_OF_A_CLASS_METHOD = "getClassSecurities";
        public const string GET_PARAM_METHOD = "getParamEx";
        
        public const string SECURITY_TICKER_PROPERTY = "code";
        public const string SECURITY_CLASS_CODE_PROPERTY = "class_code";
        public const string SECURITY_DESCRIPTION_PROPERTY = "name";
        public const string SECURITY_CURRENCY_PROPERTY = "face_unit";
        public const string SECURITY_MIN_PRICE_STEP_PROPERTY = "min_price_step";
        public const string SECURITY_PRICE_SCALE_PROPERTY = "scale";
        public const string SECURITY_CONTRACT_SIZE_PROPERTY = "lot_size";
        public const string SECURITY_EXPIRY_DATE_PROPERTY = "mat_date";
        public const string SECURITY_STRIKE_PROPERTY = "option_strike";
        public const string SECURITY_UNDERLYING_CLASS_CODE_PROPERTY = "base_active_classcode";
        public const string SECURITY_UNDERLYING_SEC_CODE_PROPERTY = "base_active_seccode";
        public const string SECURITY_NEAR_TERM_LEG_CLASS_CODE_PROPERTY = "sell_leg_classcode";
        public const string SECURITY_NEAR_TERM_LEG_SEC_CODE_PROPERTY = "sell_leg_seccode";
        public const string SECURITY_LONG_TERM_LEG_CLASS_CODE_PROPERTY = "buy_leg_classcode";
        public const string SECURITY_LONG_TERM_LEG_SEC_CODE_PROPERTY = "buy_leg_seccode";

        public const string PARAM_SECURITY_OPTION_TYPE = "optiontype";

        public const string PARAM_RESPONSE_RESULT_PROPERTY = "result";
        public const string PARAM_RESPONSE_VALUE_PROPERTY = "param_value";

        public const long PARAM_RESPONSE_TYPE_DOUBLE = 1;
        public const long PARAM_RESPONSE_TYPE_LONG = 2;
        public const long PARAM_RESPONSE_TYPE_CHAR = 3;
        public const long PARAM_RESPONSE_TYPE_ENUM = 4;
        public const long PARAM_RESPONSE_TYPE_TIME = 5;
        public const long PARAM_RESPONSE_TYPE_DATE = 6;
        
        public const string RUB_CURRENCY = "SUR";
        public const string USD_CURRENCY = "USD";

        public const int DEFAULT_TRADING_SIZE = 1;

        public const string CALENDAR_SPREADS_CLASS_CODE = "FUTSPREAD";
        public const string OPTIONS_CLASS_CODE = "SPBOPT";
        public const string FUTURES_CLASS_CODE = "SPBFUT";
        public const string STOCK_CLASS_CODE = "TQBR";

        public static readonly Guid MoexExchangeId = new("2E37326B-1A1D-41F8-86B6-BA4AA8F7593C");
        
        public static readonly TimeSpan CommonExpiryTime = TimeSpan.FromHours(19);
        public static readonly TimeSpan MoexUtcOffset = TimeSpan.FromHours(3);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quik.QuikApi
{
    internal static class Account
    {
        public const string SPOT_CURRENT_FUNDS_LIMIT = "money_current_limit";
        public const string SPOT_INCOMING_FUNDS_LIMIT = "money_open_limit";
        public const string SPOT_CURRENT_FUNDS = "money_current_balance";
        public const string SPOT_INCOMING_FUNDS = "money_open_balance";
        public const string SPOT_AVAILABLE_FUNDS = "money_limit_available";
        public const string SPOT_ODRERS_COLLATERAL = "money_limit_locked";
        public const string SPOT_NON_MARGIN_ODRERS_COLLATERAL = "money_limit_locked_nonmarginal_value";
        
        public const string DER_TOTAL_FUNDS = "cbplimit";
        public const string DER_COLLATERAL = "cbplused";
        public const string DER_AVAILABLE_FUNDS = "cbplplanned";
        public const string DER_FLOATING_INCOME = "varmargin";
        public const string DER_RECORDED_INCOME = "accruedint";

        public const string CURRENCY = "currcode";

        public const string TRADING_ACCOUNTS_SRC = "trade_accounts";
        public const string DERIVATIVE_ACCOUNTS_SRC = "futures_client_limits";

        public const string TRADING_ACCOUNTS_ID = "trdaccid";
        public const string FIRM_ID = "firmid";

        public const string LIMIT_TYPE = "limit_type";

        /// <summary>
        /// Тип лимита "Денежные средства"
        /// </summary>
        public const string MONEY_LIMIT_TYPE = "0";
        public const string CLEARING_LIMIT_TYPE = "3";
        
        public const string STOCK_SETTLEMENT_TAG = "EQTV";
        public const string FOREX_SETTLEMENT_TAG = "USDR";
        public const string FOREX_TOD_SETTLEMENT_TAG = "RTOD";
        public const string FOREX_TOM_SETTLEMENT_TAG = "RTOM";

        public const string SUR_CURRENCY = "SUR";
        public const string RUB_CURRENCY = "RUB";
        public const string USD_CURRENCY = "USD";
    }
    internal static class Methods
    {
        public const string GET_ITEM = "getItem";
        public const string GET_NUMBER_OF_ITEMS = "getNumberOf";
        public const string GET_SPOT_FUNDS = "getMoney";
        public const string GET_DERIVATIVES_FUNDS = "getFuturesLimit";
    }
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

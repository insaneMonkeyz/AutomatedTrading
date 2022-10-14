using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuikLuaApi
{
    internal class QuikApi
    {
        public const string GET_SECURITY_METHOD = "getSecurityInfo";
        public const string SECURITY_DESCRIPTION_PROPERTY = "name";
        public const string SECURITY_CURRENCY_PROPERTY = "face_value";
        public const string SECURITY_MIN_PRICE_STEP_PROPERTY = "min_price_step";
        public const string SECURITY_PRICE_SCALE_PROPERTY = "scale";
        public const string SECURITY_CONTRACT_SIZE_PROPERTY = "lot_size";
        public const string SECURITY_EXPIRY_DATE_PROPERTY = "mat_date";
        public const string SECURITY_STRIKE_PROPERTY = "option_strike";
        public const string SECURITY_UNDERLYING_CLASS_CODE_PROPERTY = "base_active_classcode";
        public const string SECURITY_UNDERLYING_SEC_CODE_PROPERTY = "base_active_seccode";
        
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

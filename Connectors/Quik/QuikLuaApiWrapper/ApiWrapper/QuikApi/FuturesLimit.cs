using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasicConcepts;
using QuikLuaApi;
using QuikLuaApi.QuikApi;
using QuikLuaApiWrapper.Extensions;
using static QuikLuaApi.QuikLuaApiWrapper;
using static QuikLuaApiWrapper.ApiWrapper.GenericQuikTable;

namespace QuikLuaApiWrapper.ApiWrapper.QuikApi
{
    /// <summary>
    /// Wrapper таблицы лимитов по клиентским счетам
    /// </summary>
    internal static class FuturesLimits
    {
        public const string NAME = "futures_client_limits";

        public const string GET_METOD = "getFuturesLimit";
        public const string CALLBACK_METHOD = "OnFuturesLimitChange";

        private const string COLLATERAL = "cbplused";
        private const string TOTAL_FUNDS = "cbplimit";
        private const string FREE_FUNDS = "cbplplanned";
        private const string FLOATING_INCOME = "varmargin";
        private const string RECORDED_INCOME = "accruedint";

        private const string LIMIT_TYPE = "limit_type";
        private const long MONEY_LIMIT_TYPE = 0;

        private const string ACCOUNT_CURRENCY = "currcode";
        private const string ACCOUNT_ID = "trdaccid";
        private const string FIRM_ID = "firmid";

        private static LuaState _stack;

        public static void Set(LuaState stack)
        {
            _stack = stack;
        }

        public static Currencies? AccountCurrency
        {
            get => _stack.ReadRowValueString(ACCOUNT_CURRENCY)?.CodeToCurrency();
        }
        public static Decimal5 TotalFunds
        {
            get => _stack.ReadRowValueDecimal5(TOTAL_FUNDS);
        }
        public static Decimal5 UnusedFunds
        {
            get => _stack.ReadRowValueDecimal5(FREE_FUNDS);
        }
        public static Decimal5 Collateral
        {
            get => _stack.ReadRowValueDecimal5(COLLATERAL);
        }
        public static Decimal5 FloatingIncome
        {
            get => _stack.ReadRowValueDecimal5(FLOATING_INCOME);
        }
        public static Decimal5 RecorderIncome
        {
            get => _stack.ReadRowValueDecimal5(RECORDED_INCOME);
        }
        public static string? ClientCode
        {
            get => _stack.ReadRowValueString(ACCOUNT_ID);
        }
        public static string? FirmId
        {
            get => _stack.ReadRowValueString(FIRM_ID);
        }
        public static bool IsMainAccount
        {
            get => _stack.ReadRowValueLong(LIMIT_TYPE) == MONEY_LIMIT_TYPE;
        }
    }
}

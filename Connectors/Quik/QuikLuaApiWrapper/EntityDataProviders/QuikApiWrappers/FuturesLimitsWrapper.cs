using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradingConcepts;
using Quik.Lua;
using static Quik.Quik;

namespace Quik.EntityProviders.QuikApiWrappers
{
    /// <summary>
    /// Wrapper таблицы лимитов по клиентским счетам
    /// </summary>
    internal static class FuturesLimitsWrapper
    {
        public const string NAME = "futures_client_limits";

        public const string GET_METOD = "getFuturesLimit";
        public const string CALLBACK_METHOD = "OnFuturesLimitChange";

        public const long MONEY_LIMIT_TYPE = 0;

        private const string COLLATERAL = "cbplused";
        private const string TOTAL_FUNDS = "cbplimit";
        private const string FREE_FUNDS = "cbplplanned";
        private const string FLOATING_INCOME = "varmargin";
        private const string RECORDED_INCOME = "accruedint";

        private const string LIMIT_TYPE = "limit_type";

        private const string ACCOUNT_CURRENCY = "currcode";
        private const string ACCOUNT_ID = "trdaccid";
        private const string FIRM_ID = "firmid";

        private static LuaWrap _stack;

        public static readonly object Lock = new();

        public static void Set(LuaWrap stack)
        {
            _stack = stack;
        }

        public static string? MoexCurrencyCode
        {
            get => _stack.ReadRowValueString(ACCOUNT_CURRENCY);
        }
        public static Currencies AccountCurrency
        {
            get => _stack.ReadRowValueString(ACCOUNT_CURRENCY).CodeToCurrency();
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
        public static Decimal5 RecordedIncome
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
        public static long LimitType
        {
            get => _stack.ReadRowValueLong(LIMIT_TYPE);
        }
    }
}

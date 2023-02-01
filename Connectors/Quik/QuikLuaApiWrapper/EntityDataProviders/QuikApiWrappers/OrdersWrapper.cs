using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasicConcepts;

namespace Quik.EntityProviders.QuikApiWrappers
{
    internal class OrdersWrapper
    {
        public const string NAME = "orders";
        public const string GET_METOD = "getOrderByNumber";
        public const string CALLBACK_METHOD = "OnOrder";

        private const string EXCHANGE_ORDER_ID = "order_num";
        private const string TRANSACTION_ID = "trans_id";
        private const string FLAGS = "flags";
        private const string ACCOUNT_ID = "account";
        private const string PRICE = "price";
        private const string AVERAGE_PRICE = "awg_price";
        private const string SIZE = "qty";
        private const string REST = "balance";
        private const string EXPIRY = "expiry";
        private const string TICKER = "sec_code";
        private const string CLASS_CODE = "class_code";
        private const string EXECUTION_MODE = "exec_type";
        private const string REJECT_REASON = "reject_reason";

        private static LuaState _stack;

        public static readonly object Lock = new();

        public static void Set(LuaState stack)
        {
            _stack = stack;
        }

        public static long ExchangeOrderId
        {
            get => _stack.ReadRowValueLong(EXCHANGE_ORDER_ID);
        }
        public static long TransactionId
        {
            get => _stack.ReadRowValueLong(TRANSACTION_ID);
        }
        public static OrderFlags Flags
        {
            get => (OrderFlags)_stack.ReadRowValueLong(FLAGS);
        }
        public static string? Account
        {
            get => _stack.ReadRowValueString(ACCOUNT_ID);
        }
        public static Decimal5 Price
        {
            get => _stack.ReadRowValueDecimal5(PRICE);
        }
        public static Decimal5 AveragePrice
        {
            get => _stack.ReadRowValueDecimal5(AVERAGE_PRICE);
        }
        public static long Size
        {
            get => _stack.ReadRowValueLong(SIZE);
        }
        public static long Rest
        {
            get => _stack.ReadRowValueLong(REST);
        }
        public static MoexOrderExecutionModes OrderExecutionMode
        {
            get => (MoexOrderExecutionModes)_stack.ReadRowValueLong(EXECUTION_MODE);
        }
        public string? RejectReason
        {
            get => _stack.ReadRowValueString(REJECT_REASON);
        }
        public static DateTimeOffset? Expiry
        {
            get
            {
                return _stack.TryFetchStringFromTable(EXPIRY, out string exp) &&
                        exp.TryConvertToMoexExpiry(out DateTimeOffset expiry)
                    ? expiry : null;
            }
        }
        public static string? Ticker
        {
            get => _stack.ReadRowValueString(TICKER);
        }
        public static string? ClassCode
        {
            get => _stack.ReadRowValueString(CLASS_CODE);
        }
    }
}

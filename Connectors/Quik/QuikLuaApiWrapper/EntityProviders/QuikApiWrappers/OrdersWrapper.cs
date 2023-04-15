using Quik.EntityProviders.Attributes;
using Quik.Lua;
using TradingConcepts;

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

        private static LuaWrap _stack;

        public static readonly object Lock = new();

        public static void Set(LuaWrap stack)
        {
            _stack = stack;
        }

        [QuikCallbackField(EXCHANGE_ORDER_ID)]
        public static long ExchangeOrderId
        {
           get => _stack.ReadRowValueInteger(EXCHANGE_ORDER_ID);
        }

        [QuikCallbackField(TRANSACTION_ID)]
        public static long TransactionId
        {
            get => _stack.ReadRowValueInteger(TRANSACTION_ID);
        }

        [QuikCallbackField(FLAGS)]
        public static OrderFlags Flags
        {
            get => (OrderFlags)_stack.ReadRowValueInteger(FLAGS);
        }

        [QuikCallbackField(ACCOUNT_ID)]
        public static string? Account
        {
            get => _stack.ReadRowValueString(ACCOUNT_ID);
        }

        [QuikCallbackField(PRICE)]
        public static Decimal5 Price
        {
            get => _stack.ReadRowValueDecimal5(PRICE);
        }

        [QuikCallbackField(AVERAGE_PRICE)]
        public static Decimal5 AveragePrice
        {
            get => _stack.ReadRowValueDecimal5(AVERAGE_PRICE);
        }

        [QuikCallbackField(SIZE)]
        public static long Size
        {
            get => _stack.ReadRowValueInteger(SIZE);
        }

        [QuikCallbackField(REST)]
        public static long Rest
        {
            get => _stack.ReadRowValueInteger(REST);
        }

        [QuikCallbackField(EXECUTION_MODE)]
        public static MoexOrderExecutionModes OrderExecutionMode
        {
            get => (MoexOrderExecutionModes)_stack.ReadRowValueInteger(EXECUTION_MODE);
        }

        [QuikCallbackField(REJECT_REASON)]
        public string? RejectReason
        {
            get => _stack.ReadRowValueString(REJECT_REASON);
        }

        [QuikCallbackField(EXPIRY)]
        public static DateTimeOffset? Expiry
        {
            get
            {
                return _stack.TryFetchStringFromTable(EXPIRY, out string exp) &&
                        exp.TryConvertToMoexExpiry(out DateTimeOffset expiry)
                    ? expiry : null;
            }
        }

        [QuikCallbackField(TICKER)]
        public static string? Ticker
        {
            get => _stack.ReadRowValueString(TICKER);
        }

        [QuikCallbackField(CLASS_CODE)]
        public static string? ClassCode
        {
            get => _stack.ReadRowValueString(CLASS_CODE);
        }
    }
}

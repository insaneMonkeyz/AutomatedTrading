using Quik.EntityProviders.Attributes;
using Quik.Lua;
using TradingConcepts;

namespace Quik.EntityProviders.QuikApiWrappers
{
    internal static class ExecutionWrapper
    {
        public const string NAME = "trades";
        public const string CALLBACK_METHOD = "OnTrade";

        private const string ACCOUNT = "account";
        private const string CLIENT_CODE = "client_code";
        private const string FIRM_ID = "firmid";
        private const string TRADE_ID = "trade_num";
        private const string EXCHANGE_ORDER_ID = "order_num";
        private const string TRANSACTION_ID = "trans_id";
        private const string TICKER = "sec_code";
        private const string CLASS_CODE = "class_code";
        private const string FLAGS = "flags";
        private const string PRICE = "price";
        private const string SIZE = "qty";
        private const string TIME = "datetime";

        private const string ORDER_EXCHANGE_CODE = "order_exchange_code"; // STRING Биржевой номер заявки 
        private const string CANCELED_DATETIME = "canceled_datetime"; // TABLE Дата и время снятия сделки 

         

        private const long SELL_OPERATION_BIT = 0x4;

        private static LuaWrap _stack;

        public static readonly object Lock = new();

        public static void Set(LuaWrap stack)
        {
            _stack = stack;
        }

        [QuikCallbackField(ACCOUNT)]
        public static string? Account
        {
            get => _stack.ReadRowValueString(ACCOUNT);
        }

        [QuikCallbackField(CLIENT_CODE)]
        public static string? ClientCode
        {
            get => _stack.ReadRowValueString(CLIENT_CODE);
        }

        [QuikCallbackField(FIRM_ID)]
        public static string? FirmId
        {
            get => _stack.ReadRowValueString(FIRM_ID);
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

        [QuikCallbackField(EXCHANGE_ORDER_ID)]
        public static long ExchangeOrderId
        {
            get => _stack.ReadRowValueInteger(EXCHANGE_ORDER_ID);
        }

        [QuikCallbackField(ORDER_EXCHANGE_CODE)]
        public static string? ExchangeOrderCode
        {
            get => _stack.ReadRowValueString(ORDER_EXCHANGE_CODE);
        }

        [QuikCallbackField(TRANSACTION_ID)]
        public static long TransactionId
        {
            get => _stack.ReadRowValueInteger(TRANSACTION_ID);
        }

        [QuikCallbackField(TRADE_ID)]
        public static long TradeId
        {
            get => _stack.ReadRowValueInteger(TRADE_ID);
        }

        [QuikCallbackField(PRICE)]
        public static Decimal5 Price
        {
            get => _stack.ReadRowValueDecimal5(PRICE);
        }

        [QuikCallbackField(SIZE)]
        public static long Size
        {
            get => _stack.ReadRowValueInteger(SIZE);
        }

        [QuikCallbackField(FLAGS)]
        public static Operations Operation
        {
            get => (_stack.ReadRowValueInteger(FLAGS) & SELL_OPERATION_BIT) == SELL_OPERATION_BIT
                ? Operations.Sell
                : Operations.Buy;
        }

        [QuikCallbackField(TIME)]
        public static DateTimeOffset Timestamp
        {
            get => TimeWrapper.GetTime(_stack, TIME).GetValueOrDefault();
        }

        [QuikCallbackField(CANCELED_DATETIME)]
        public static DateTimeOffset CancelledTimestamp
        {
            get => TimeWrapper.GetTime(_stack, CANCELED_DATETIME).GetValueOrDefault();
        }
    }
}

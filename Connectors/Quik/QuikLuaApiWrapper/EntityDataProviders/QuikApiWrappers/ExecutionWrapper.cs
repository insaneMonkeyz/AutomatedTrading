using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasicConcepts;

namespace Quik.EntityDataProviders.QuikApiWrappers
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

        private const long SELL_OPERATION_BIT = 0x4;

        private static LuaState _stack;

        public static void Set(LuaState stack)
        {
            _stack = stack;
        }
        public static string? Account
        {
            get => _stack.ReadRowValueString(ACCOUNT);
        }
        public static string? ClientCode
        {
            get => _stack.ReadRowValueString(CLIENT_CODE);
        }
        public static string? FirmId
        {
            get => _stack.ReadRowValueString(FIRM_ID);
        }
        public static string? Ticker
        {
            get => _stack.ReadRowValueString(TICKER);
        }
        public static string? ClassCode
        {
            get => _stack.ReadRowValueString(CLASS_CODE);
        }
        public static long ExchangeOrderId
        {
            get => _stack.ReadRowValueLong(EXCHANGE_ORDER_ID);
        }
        public static long TransactionId
        {
            get => _stack.ReadRowValueLong(TRANSACTION_ID);
        }
        public static long TradeId
        {
            get => _stack.ReadRowValueLong(TRADE_ID);
        }
        public static Decimal5 Price
        {
            get => _stack.ReadRowValueDecimal5(PRICE);
        }
        public static long Size
        {
            get => _stack.ReadRowValueLong(SIZE);
        }
        public static Operations Operation
        {
            get => (_stack.ReadRowValueLong(FLAGS) & SELL_OPERATION_BIT) == SELL_OPERATION_BIT
                ? Operations.Sell
                : Operations.Buy;
        }
        public static DateTimeOffset Timestamp
        {
            get => TimeWrapper.GetTime(_stack, TIME).GetValueOrDefault();
        }
    }
}

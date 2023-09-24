using System.Runtime.CompilerServices;
using Quik.Entities;
using Quik.EntityProviders.Attributes;
using Quik.Lua;
using Tools;
using TradingConcepts;

namespace Quik.EntityProviders.QuikApiWrappers
{
    internal partial class TransactionWrapper
    {
        private static LuaWrap _context;

        public static void SetContext(LuaWrap lua)
        {
            _context = lua;
        }

        [QuikCallbackField(ID)]
        public static long Id
        {
            get => _context.ReadRowValueInteger(ID);
        }
        [QuikCallbackField(SERVER_TRANSACTION_ID)]
        public static long ServerTransactionId
        {
            get => _context.ReadRowValueInteger(SERVER_TRANSACTION_ID);
        }
        [QuikCallbackField(ORDER_ID)]
        public static long OrderId
        {
            get => _context.ReadRowValueInteger(ORDER_ID);
        }
        [QuikCallbackField(ERROR_SITE)]
        public static ErrorSite ErrorSource
        {
            get => (ErrorSite)_context.ReadRowValueInteger(ERROR_SITE);
        }
        [QuikCallbackField(ERROR_CODE)]
        public static ErrorSite ErrorCode
        {
            get => (ErrorSite)_context.ReadRowValueInteger(ERROR_CODE);
        }
        [QuikCallbackField(STATUS)]
        public static TransactionStatus Status
        {
            get => (TransactionStatus)_context.ReadRowValueInteger(STATUS);
        }
        [QuikCallbackField(RESULT_DESCRIPTION)]
        public static string? ResultDescription
        {
            get => _context.ReadRowValueString(RESULT_DESCRIPTION);
        }
        [QuikCallbackField(TIMESTAMP)]
        public static DateTimeOffset Timestamp
        {
            get => TimeWrapper.GetTime(_context, TIMESTAMP).GetValueOrDefault();
        }
        [QuikCallbackField(RESPONSE_RECEPTION_TIMESTAMP)]
        public static DateTimeOffset ResponseReceptionTimestamp
        {
            get => TimeWrapper.GetTime(_context, RESPONSE_RECEPTION_TIMESTAMP).GetValueOrDefault();
        }
        [QuikCallbackField(CLASS_CODE)]
        public static string? ClassCode
        {
            get => _context.ReadRowValueString(CLASS_CODE);
        }
        [QuikCallbackField(TICKER)]
        public static string? Ticker
        {
            get => _context.ReadRowValueString(TICKER);
        }
        [QuikCallbackField(REST)]
        public static long RejectedSize
        {
            get => _context.TryFetchIntegerFromTable(REST, out long result) ? result : 0;
        }
        [QuikCallbackField(PRICE)]
        public static Decimal5 Price
        {
            get => _context.ReadRowValueDecimal5(PRICE);
        }
        [QuikCallbackField(SIZE)]
        public static long Size
        {
            get => _context.ReadRowValueInteger(SIZE);
        }
        [QuikCallbackField(UNIQUE_ID)]
        public static long Uid
        {
            get => _context.ReadRowValueInteger(UNIQUE_ID);
        }

        public struct NewOrderArgs
        {
            public Order Order;
            public string Price;
            public string Expiry;
            public string Operation;
            public string ExecutionCondition;
            public string ExecutionType;
        }

        public static string? CancelOrder(Order order)
        {
            static void cancel(Order order)
            {
                Quik.Lua.SetTableValue(TRANSACTION_ID_PARAM, order.TransactionId.ToString());
                Quik.Lua.SetTableValue(CLASS_CODE_PARAM, order.Security.ClassCode);
                Quik.Lua.SetTableValue(ORDER_ID_PARAM, order.ExchangeAssignedIdString);
                Quik.Lua.SetTableValue(TICKER_PARAM, order.Security.Ticker);
                Quik.Lua.SetTableValue(ACTION_PARAM, ACTION_CANCEL_ORDER_PARAM);
            }

            return SendTransaction(5, cancel, order);
        }
        public static string? PlaceNewOrder(ref NewOrderArgs orderArgs)
        {
            static void place(ref NewOrderArgs args)
            {
                Quik.Lua.SetTableValue(EXECUTION_CONDITION_PARAM, args.ExecutionCondition);
                Quik.Lua.SetTableValue(TRANSACTION_ID_PARAM, args.Order.TransactionId.ToString());
                Quik.Lua.SetTableValue(ORDER_TYPE_PARAM, args.ExecutionType);
                //Quik.Lua.SetTableValue(CLIENT_CODE_PARAM, args.Order.ClientCode); probably not used
                Quik.Lua.SetTableValue(EXPIRY_DATE_PARAM, args.Expiry);
                Quik.Lua.SetTableValue(CLASS_CODE_PARAM, args.Order.Security.ClassCode);
                Quik.Lua.SetTableValue(OPERATION_PARAM, args.Operation);
                Quik.Lua.SetTableValue(ACCOUNT_PARAM, args.Order.AccountCode);
                Quik.Lua.SetTableValue(TICKER_PARAM, args.Order.Security.Ticker);
                Quik.Lua.SetTableValue(ACTION_PARAM, ACTION_NEW_ORDER_PARAM);
                Quik.Lua.SetTableValue(PRICE_PARAM, args.Price);
                Quik.Lua.SetTableValue(SIZE_PARAM, args.Order.Quote.Size.ToString());
            }

            return SendTransaction(12, place, ref orderArgs);
        }
        public static string? ChangeOrder(Order order)
        {
            static void change(Order order)
            {
                Quik.Lua.SetTableValue(TRANSACTION_ID_PARAM, order.TransactionId.ToString());
                Quik.Lua.SetTableValue(MOVE_ORDER_PRICE_PARAM, order.Quote.Price.ToString((uint)order.Security.PricePrecisionScale));
                Quik.Lua.SetTableValue(MOVE_ORDER_SIZE_PARAM, order.Quote.Size.ToString());
                Quik.Lua.SetTableValue(MOVE_ORDER_ID_PARAM, order.ExchangeAssignedIdString);
                Quik.Lua.SetTableValue(CLASS_CODE_PARAM, order.Security.ClassCode);
                Quik.Lua.SetTableValue(TICKER_PARAM, order.Security.Ticker);
                Quik.Lua.SetTableValue(ACTION_PARAM, ACTION_MOVE_ORDER_PARAM);
                Quik.Lua.SetTableValue(MODE_PARAM, MOVE_ORDER_NEW_SIZE_MODE);
            }

            return SendTransaction(8, change, order);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string? SendTransaction<TTableArgs>(int tableSize, CompleteTable<TTableArgs> completeTable, ref TTableArgs tableArgs) where TTableArgs : struct
        {
            lock (Quik.SyncRoot)
            {
                string? result = null;

                if (Quik.Lua.ExecFunction(SEND_TRANSACTION_METHOD, Api.TYPE_STRING, tableSize, completeTable, ref tableArgs))
                {
                    result = Quik.Lua.PopString();
                }

                Quik.Lua.PopFromStack();

                return result; 
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string? SendTransaction<TTableArgs>(int tableSize, Action<TTableArgs> completeTable, TTableArgs tableArgs) where TTableArgs : class
        {
            lock (Quik.SyncRoot)
            {
                string? result = null;

                if (Quik.Lua.ExecFunction(SEND_TRANSACTION_METHOD, Api.TYPE_STRING, tableSize, completeTable, tableArgs))
                {
                    result = Quik.Lua.PopString();
                }

                Quik.Lua.PopFromStack();

                return result; 
            }
        }
    }
}

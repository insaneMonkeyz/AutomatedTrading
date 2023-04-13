using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Core.Tools;
using Quik.Entities;
using Quik.EntityProviders.QuikApiWrappers;
using Quik.Lua;
using TradingConcepts;
using TradingConcepts.CommonImplementations;

namespace Quik.EntityProviders.QuikApiWrappers
{
    internal partial class TransactionWrapper
    {
        private static LuaWrap _context;

        public static void SetContext(LuaWrap lua)
        {
            _context = lua;
        }

        public static long Id
        {
            get => _context.ReadRowValueLong(ID);
        }
        public static ErrorSite ErrorSource
        {
            get => (ErrorSite)_context.ReadRowValueLong(ERROR_SITE);
        }
        public static TransactionStatus Status
        {
            get => (TransactionStatus)_context.ReadRowValueLong(STATUS);
        }
        public static string? ResultDescription
        {
            get => _context.ReadRowValueString(RESULT_DESCRIPTION);
        }
        public static DateTimeOffset Timestamp
        {
            get => TimeWrapper.GetTime(_context, TIMESTAMP).GetValueOrDefault();
        }
        public static DateTimeOffset ResponseReceptionTimestamp
        {
            get => TimeWrapper.GetTime(_context, RESPONSE_RECEPTION_TIMESTAMP).GetValueOrDefault();
        }
        public static string? ExchangeAssignedOrderId
        {
            get => _context.ReadRowValueString(EXCHANGE_ORDER_ID);
        }
        public static string? ClassCode
        {
            get => _context.ReadRowValueString(CLASS_CODE);
        }
        public static string? Ticker
        {
            get => _context.ReadRowValueString(TICKER);
        }
        public static long RejectedSize
        {
            get => _context.TryFetchLongFromTable(REST, out long result) ? result : 0;
        }
        public static Decimal5 Price
        {
            get => _context.ReadRowValueDecimal5(PRICE);
        }
        public static long Size
        {
            get => _context.ReadRowValueLong(SIZE);
        }
        public static long Uid
        {
            get => _context.ReadRowValueLong(UNIQUE_ID);
        }

        public struct CancelOrderArgs
        {
            public Order Order;
        }
        public struct ChangeOrderArgs
        {
            public Order Order;
            public Decimal5 NewPrice;
            public long NewSize;
        }
        public struct NewOrderArgs
        {
            public MoexOrderSubmission OrderSubmission;
            public string Price;
            public string Expiry;
            public string Operation;
            public string ExecutionCondition;
            public string ExecutionType;
        }

        public static string? CancelOrder(ref CancelOrderArgs cancelArgs)
        {
            static void cancel(ref CancelOrderArgs args)
            {
                Quik.Lua.SetTableValue(TRANSACTION_ID_PARAM, TransactionIdGenerator.CreateId().ToString());
                Quik.Lua.SetTableValue(CLASS_CODE_PARAM, args.Order.Security.ClassCode);
                Quik.Lua.SetTableValue(ORDER_ID_PARAM, args.Order.ExchangeAssignedIdString);
                Quik.Lua.SetTableValue(TICKER_PARAM, args.Order.Security.Ticker);
                Quik.Lua.SetTableValue(ACTION_PARAM, ACTION_CANCEL_ORDER_PARAM);
            }

            return SendTransaction(5, cancel, ref cancelArgs);
        }
        public static string? PlaceNewOrder(ref NewOrderArgs orderArgs)
        {
            static void place(ref NewOrderArgs args)
            {
                Quik.Lua.SetTableValue(EXECUTION_CONDITION_PARAM, args.ExecutionCondition);
                Quik.Lua.SetTableValue(TRANSACTION_ID_PARAM, args.OrderSubmission.TransactionId.ToString());
                Quik.Lua.SetTableValue(ORDER_TYPE_PARAM, args.ExecutionType);
                Quik.Lua.SetTableValue(CLIENT_CODE_PARAM, args.OrderSubmission.ClientCode);
                Quik.Lua.SetTableValue(EXPIRY_DATE_PARAM, args.Expiry);
                Quik.Lua.SetTableValue(CLASS_CODE_PARAM, args.OrderSubmission.Security.ClassCode);
                Quik.Lua.SetTableValue(OPERATION_PARAM, args.Operation);
                Quik.Lua.SetTableValue(ACCOUNT_PARAM, args.OrderSubmission.AccountCode);
                Quik.Lua.SetTableValue(TICKER_PARAM, args.OrderSubmission.Security.Ticker);
                Quik.Lua.SetTableValue(ACTION_PARAM, ACTION_NEW_ORDER_PARAM);
                Quik.Lua.SetTableValue(PRICE_PARAM, args.Price);
                Quik.Lua.SetTableValue(SIZE_PARAM, args.OrderSubmission.Quote.Size.ToString());
            }

            return SendTransaction(12, place, ref orderArgs);
        }
        public static string? ChangeOrder(ref ChangeOrderArgs changeArgs)
        {
            static void change(ref ChangeOrderArgs args)
            {
                Quik.Lua.SetTableValue(TRANSACTION_ID_PARAM, TransactionIdGenerator.CreateId().ToString());
                Quik.Lua.SetTableValue(MOVE_ORDER_PRICE_PARAM, args.NewPrice.ToString((uint)args.Order.Security.PricePrecisionScale));
                Quik.Lua.SetTableValue(MOVE_ORDER_SIZE_PARAM, args.NewSize.ToString());
                Quik.Lua.SetTableValue(MOVE_ORDER_ID_PARAM, args.Order.ExchangeAssignedIdString);
                Quik.Lua.SetTableValue(CLASS_CODE_PARAM, args.Order.Security.ClassCode);
                Quik.Lua.SetTableValue(TICKER_PARAM, args.Order.Security.Ticker);
                Quik.Lua.SetTableValue(ACTION_PARAM, ACTION_MOVE_ORDER_PARAM);
                Quik.Lua.SetTableValue(MODE_PARAM, MOVE_ORDER_NEW_SIZE_MODE);
            }

            return SendTransaction(8, change, ref changeArgs);
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
    }
}

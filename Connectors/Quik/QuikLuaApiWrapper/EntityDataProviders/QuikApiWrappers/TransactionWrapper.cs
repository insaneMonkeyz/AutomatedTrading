using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Core.Tools;
using Quik.Entities;
using Quik.EntityProviders.QuikApiWrappers;
using Quik.Lua;
using TradingConcepts;

namespace Quik.EntityDataProviders.QuikApiWrappers
{
    internal partial class TransactionWrapper
    {
        private const string ORDER_NOT_ACTIVE_ERROR = "Order is not active";

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
        public static long RemainingSize
        {
            get => _context.TryFetchLongFromTable(REST, out long result) ? result : 0;
        }

        public static string? CancelOrder(Order order)
        {
            if (order.State != OrderStates.Active)
            {
                return ORDER_NOT_ACTIVE_ERROR;
            }

            return SendTransaction(5, () =>
            {
                Quik.Lua.SetTableValue(TRANSACTION_ID_PARAM, TransactionIdGenerator.CreateId().ToString());
                Quik.Lua.SetTableValue(CLASS_CODE_PARAM, order.Security.ClassCode);
                Quik.Lua.SetTableValue(ORDER_ID_PARAM, order.ExchangeAssignedIdString);
                Quik.Lua.SetTableValue(TICKER_PARAM, order.Security.Ticker);
                Quik.Lua.SetTableValue(ACTION_PARAM, ACTION_CANCEL_ORDER_PARAM);
            });
        }
        public static string? PlaceNewOrder(MoexOrderSubmission submission)
        {
            if (submission.ClientCode is null)
            {
                throw new ArgumentException($"{nameof(submission.ClientCode)} of the order is not set");
            }

            var exectype = string.Empty;
            var execondition = string.Empty;
            var expiry = string.Empty;
            var price = string.Empty;

            var operation = submission.Quote.Operation == Operations.Buy
                ? BUY_OPERATION_PARAM
                : SELL_OPERATION_PARAM;

            if (submission.IsMarket)
            {
                exectype = ORDER_TYPE_MARKET_PARAM;
            }
            else
            {
                exectype = ORDER_TYPE_LIMIT_PARAM;
                price = submission.Quote.Price.ToString((uint)submission.Security.PricePrecisionScale);
            }

            switch (submission.ExecutionCondition)
            {
                case OrderExecutionConditions.FillOrKill:
                    {
                        execondition = FILL_OR_KILL_ORDER_PARAM;
                        break;
                    }
                case OrderExecutionConditions.CancelRest:
                    {
                        execondition = CANCEL_BALANCE_ORDER_PARAM;
                        break;
                    }
                case OrderExecutionConditions.Session:
                    {
                        execondition = QUEUE_ORDER_PARAM;
                        expiry = ORDER_TODAY_EXPIRY_PARAM;
                        break;
                    }
                case OrderExecutionConditions.GoodTillCancelled:
                    {
                        execondition = QUEUE_ORDER_PARAM;
                        expiry = ORDER_GTC_EXPIRY_PARAM;
                        break;
                    }
                case OrderExecutionConditions.GoodTillDate:
                    {
                        execondition = QUEUE_ORDER_PARAM;
                        expiry = submission.Expiry.ToString("yyyyMMdd");
                        break;
                    }
                default:
                    throw new NotImplementedException(
                    $"Add support for {nameof(OrderExecutionConditions)}.{submission.ExecutionCondition} case");
            }

            return SendTransaction(12, () =>
            {
                Quik.Lua.SetTableValue(EXECUTION_CONDITION_PARAM, execondition);
                Quik.Lua.SetTableValue(TRANSACTION_ID_PARAM, submission.TransactionId.ToString());
                Quik.Lua.SetTableValue(ORDER_TYPE_PARAM, exectype);
                Quik.Lua.SetTableValue(CLIENT_CODE_PARAM, submission.ClientCode);
                Quik.Lua.SetTableValue(EXPIRY_DATE_PARAM, expiry);
                Quik.Lua.SetTableValue(CLASS_CODE_PARAM, submission.Security.ClassCode);
                Quik.Lua.SetTableValue(OPERATION_PARAM, operation);
                Quik.Lua.SetTableValue(ACCOUNT_PARAM, submission.AccountCode);
                Quik.Lua.SetTableValue(TICKER_PARAM, submission.Security.Ticker);
                Quik.Lua.SetTableValue(ACTION_PARAM, ACTION_NEW_ORDER_PARAM);
                Quik.Lua.SetTableValue(PRICE_PARAM, price);
                Quik.Lua.SetTableValue(SIZE_PARAM, submission.Quote.Size.ToString());
            });
        }
        public static string? ChangeOrder(Order order, Decimal5 newprice, int newsize)
        {
            if (order.State != OrderStates.Active)
            {
                return ORDER_NOT_ACTIVE_ERROR;
            }

            return SendTransaction(8, () =>
            {
                Quik.Lua.SetTableValue(TRANSACTION_ID_PARAM, TransactionIdGenerator.CreateId().ToString());
                Quik.Lua.SetTableValue(MOVE_ORDER_PRICE_PARAM, newprice.ToString((uint)order.Security.PricePrecisionScale));
                Quik.Lua.SetTableValue(MOVE_ORDER_SIZE_PARAM, newsize.ToString());
                Quik.Lua.SetTableValue(MOVE_ORDER_ID_PARAM, order.ExchangeAssignedIdString);
                Quik.Lua.SetTableValue(CLASS_CODE_PARAM, order.Security.ClassCode);
                Quik.Lua.SetTableValue(TICKER_PARAM, order.Security.Ticker);
                Quik.Lua.SetTableValue(ACTION_PARAM, ACTION_MOVE_ORDER_PARAM);
                Quik.Lua.SetTableValue(MODE_PARAM, MOVE_ORDER_NEW_SIZE_MODE);
            });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string? SendTransaction(int tableSize, Action completeTable)
        {
            lock (Quik.SyncRoot)
            {
                string? result = null;

                if (Quik.Lua.ExecFunction(SEND_TRANSACTION_METHOD, Api.TYPE_STRING, tableSize, completeTable))
                {
                    result = Quik.Lua.PopString();
                }

                Quik.Lua.PopFromStack();

                return result; 
            }
        }
    }
}

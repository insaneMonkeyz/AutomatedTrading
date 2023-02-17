using System.Runtime.CompilerServices;
using System.Text;
using BasicConcepts;
using BasicConcepts.SecuritySpecifics;
using Quik.Entities;

namespace Quik.EntityDataProviders.QuikApiWrappers
{
    internal partial class TransactionBuilder
    {
        public static string MakeNewOrder(DerivativesTradingAccount account, Order order)
        {
            var operation = order.Quote.Operation == Operations.Buy
                ? BUY_OPERATION_PARAM
                : SELL_OPERATION_PARAM;

            var exectype = order.IsLimit
                ? ORDER_TYPE_LIMIT_PARAM
                : ORDER_TYPE_MARKET_PARAM;

            var execondition = string.Empty;
            var expiry       = string.Empty;
            
            switch (order.ExecutionCondition)
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
                        expiry = order.Expiry.ToString("yyyyMMdd");
                        break;
                    }
                default: throw new NotImplementedException(
                    $"Add support for {nameof(OrderExecutionConditions)}.{order.ExecutionCondition} case");
            }

            var price = order is not IDerivative
                ? order.Quote.Price
                : default;

            return
                $"{ACTION_PARAM}={ACTION_NEW_ORDER_PARAM};" +

                $"{TRANSACTION_ID_PARAM}={order.TransactionId};" +

                $"{EXECUTION_CONDITION_PARAM}={execondition};" +

                $"{EXECUTION_TYPE_PARAM}={exectype};" +

                $"{EXPIRY_DATE_PARAM}={expiry};" +

                $"{ACCOUNT_PARAM}={account.AccountCode};" +

                $"{CLIENT_CODE_PARAM}={order.ClientCode};" +

                $"{CLASS_CODE_PARAM}={order.Security.ClassCode};" +

                $"{TICKER_PARAM}={order.Security.Ticker};" +

                $"{OPERATION_PARAM}={operation};" +

                $"{PRICE_PARAM}={price};" +

                $"{SIZE_PARAM}={order.Quote.Size};";
        }
    }
}

using Quik.Entities;
using Quik.EntityProviders.Resolvers.Fakes;
using Quik.EntityProviders.QuikApiWrappers.Fakes;
using TradingConcepts;
using TradingConcepts.CommonImplementations;
using Quik.EntityProviders.RequestContainers;
using Quik.EntityProviders.Resolvers;
using Quik.EntityProviders;

namespace QuikLuaWrapperTests.EntityProvidersTests
{
    internal class OrderSubmissionTestContextFactory
    {
        private static MoexDerivativeBase CreateSecurityStub()
        {
            var buildparams = new SecurityParamsContainer
            {
                ClassCode = StubConstants.DEFAULT_CLASSCODE,
                Ticker = StubConstants.DEFAULT_TICKER,
                ContractSize = 1000,
                DenominationCurrency = Currencies.RUB,
                MinPriceStep = 0.01m,
                PricePrecisionScale = 2
            };
            return new Futures(ref buildparams);
        }
        private static Order CreateOrderStub(ISecurity sec) => new(sec)
        {
            TransactionId = StubConstants.DEFAULT_TRANSACTION_ID,
            AccountCode = StubConstants.DEFAULT_ACCOUNT,
            Quote = new Quote
            {
                Operation = Operations.Sell,
                Price = StubConstants.DEFAULT_ORDER_PRICE,
                Size = StubConstants.DEFAULT_ORDER_SIZE
            }
        };

        public static OrderSubmissionTestContext SetupOrderSuccessfullyQueuedContext()
        {
            var sec = CreateSecurityStub();
            var order = CreateOrderStub(sec);

            var transactionWrapper = TransactionWrapperBehaviourFactory.Instance.CreateSuccessfulOrderSubmissionBehaviour();

            ShimTransactionWrapper.IdGet = () => transactionWrapper.Id;
            ShimTransactionWrapper.StatusGet = () => transactionWrapper.Status;
            ShimTransactionWrapper.OrderIdGet = () => transactionWrapper.OrderId;
            ShimTransactionWrapper.ClassCodeGet = () => transactionWrapper.ClassCode;
            ShimTransactionWrapper.ErrorSourceGet = () => transactionWrapper.ErrorSource;
            ShimTransactionWrapper.RejectedSizeGet = () => transactionWrapper.RejectedSize;
            ShimTransactionWrapper.PlaceNewOrderTransactionWrapperNewOrderArgsRef = transactionWrapper.PlaceNewOrder;

            var ordersWrapper = OrdersWrapperBehaviourFactory.Instance.CreateSuccessfulOrderSubmissionBehaviour();

            ShimOrdersWrapper.SizeGet = () => ordersWrapper.Size;
            ShimOrdersWrapper.RestGet = () => ordersWrapper.Rest;
            ShimOrdersWrapper.FlagsGet = () => ordersWrapper.Flags;
            ShimOrdersWrapper.PriceGet = () => ordersWrapper.Price;
            ShimOrdersWrapper.ExpiryGet = () => ordersWrapper.Expiry;
            ShimOrdersWrapper.TickerGet = () => ordersWrapper.Ticker;
            ShimOrdersWrapper.AccountGet = () => ordersWrapper.Account;
            ShimOrdersWrapper.ClassCodeGet = () => ordersWrapper.ClassCode;
            ShimOrdersWrapper.TransactionIdGet = () => ordersWrapper.TransactionId;
            ShimOrdersWrapper.ExchangeOrderIdGet = () => ordersWrapper.ExchangeOrderId;
            ShimOrdersWrapper.OrderExecutionModeGet = () => ordersWrapper.OrderExecutionMode;

            var result = new OrderSubmissionTestContext()
            {
                Security = sec,
                Order = order,
                NotificationLoop = new ExecutionLoop(),
            };
            var resolver = new EntityResolver<OrderRequestContainer, Order>(10, result.FakeOrderResolver);

            ShimEntityResolvers.GetOrdersResolver = () => resolver;

            TransactionsProvider.Instance.Initialize(result.NotificationLoop);
                  OrdersProvider.Instance.Initialize(result.NotificationLoop);

            return result;
        }
    }
}
using Quik.EntityProviders.QuikApiWrappers;
using TradingConcepts;
using Moq;

namespace QuikLuaWrapperTests.EntityProvidersTests
{
    internal class OrdersWrapperBehaviourFactory : IAbstractBehaviourFactory<IOrdersWrapperFake>
    {
        public static OrdersWrapperBehaviourFactory Instance { get; } = new();
        private OrdersWrapperBehaviourFactory()
        {

        }

        public IOrdersWrapperFake CreateSuccessfulOrderSubmissionBehaviour()
        {
            var mock = new Mock<IOrdersWrapperFake>();

            mock.Setup(tw => tw.Size)
                .Returns(StubConstants.DEFAULT_ORDER_SIZE);

            mock.Setup(tw => tw.ExchangeOrderId)
                .Returns(StubConstants.DEFAULT_EXCHANGE_ORDER_ID);

            mock.Setup(tw => tw.TransactionId)
                .Returns(StubConstants.DEFAULT_TRANSACTION_ID);

            mock.Setup(tw => tw.ClassCode)
                .Returns(StubConstants.DEFAULT_CLASSCODE);

            mock.Setup(tw => tw.Ticker)
                .Returns(StubConstants.DEFAULT_TICKER);

            mock.Setup(tw => tw.Account)
                .Returns(StubConstants.DEFAULT_ACCOUNT);

            mock.Setup(tw => tw.Price)
                .Returns(StubConstants.DEFAULT_ORDER_SIZE);

            mock.Setup(tw => tw.Rest)
                .Returns((Decimal5)StubConstants.DEFAULT_ORDER_PRICE);

            mock.Setup(tw => tw.Flags)
                .Returns((OrderFlags)29);

            mock.Setup(tw => tw.OrderExecutionMode)
                .Returns(MoexOrderExecutionModes.Normal);

            mock.Setup(tw => tw.Expiry)
                .Returns(default(DateTimeOffset?));

            return mock.Object;
        }
    }
}
using Moq;
using Quik.EntityProviders;
using static Quik.EntityProviders.QuikApiWrappers.TransactionWrapper;
using TransactionStatus = Quik.EntityProviders.QuikApiWrappers.TransactionWrapper.TransactionStatus;

namespace QuikLuaWrapperTests.EntityProvidersTests
{
    internal class TransactionWrapperBehaviourFactory : IAbstractBehaviourFactory<ITransactionWrapperFake>
    {
        public static TransactionWrapperBehaviourFactory Instance { get; } = new();
        private TransactionWrapperBehaviourFactory() { }

        public ITransactionWrapperFake CreateSuccessfulOrderSubmissionBehaviour()
        {
            var mock = new Mock<ITransactionWrapperFake>();

            mock.Setup(tw => tw.PlaceNewOrder(ref It.Ref<NewOrderArgs>.IsAny))
                .Returns(() =>
                {
                    Task.Run(() =>
                    {
                        Thread.Sleep(20);
                        TransactionsProvider.Instance.InvokePrivateMethod("OnNewData", IntPtr.Zero);
                    });

                    return default;
                });

            mock.Setup(tw => tw.RejectedSize)
                .Returns(0L);

            mock.Setup(tw => tw.OrderId)
                .Returns(StubConstants.DEFAULT_EXCHANGE_ORDER_ID);

            mock.Setup(tw => tw.Id)
                .Returns(StubConstants.DEFAULT_TRANSACTION_ID);

            mock.Setup(tw => tw.ClassCode)
                .Returns(StubConstants.DEFAULT_CLASSCODE);

            mock.Setup(tw => tw.ResultDescription)
                .Returns($"Заявка {StubConstants.DEFAULT_EXCHANGE_ORDER_ID} успешно зарегистрирована.");

            mock.Setup(tw => tw.ErrorSource)
                .Returns(ErrorSite.None);

            mock.Setup(tw => tw.Status)
                .Returns(TransactionStatus.Completed);

            return mock.Object;
        }
    }
}
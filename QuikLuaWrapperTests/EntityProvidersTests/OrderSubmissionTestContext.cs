using Quik.Entities;
using Quik.EntityProviders;
using Quik.EntityProviders.RequestContainers;

namespace QuikLuaWrapperTests.EntityProvidersTests
{
    internal class OrderSubmissionTestContext
    {
        public MoexDerivativeBase Security { get; init; }
        public Order Order { get; init; }
        public ExecutionLoop NotificationLoop { get; init; }

        public Order? FakeOrderResolver(ref OrderRequestContainer request)
        {
            return Order;
        }

        public void BeginTest(Action<OrderSubmissionTestContext> testBody)
        {
            var notificationTask = Task.Run(NotificationLoop.Enter);

            testBody(this);

            NotificationLoop.Abort();
            notificationTask.Wait();
        }
    }
}
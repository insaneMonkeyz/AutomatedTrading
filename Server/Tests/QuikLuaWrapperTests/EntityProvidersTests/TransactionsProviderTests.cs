using Microsoft.QualityTools.Testing.Fakes;
using Quik.Entities;
using Quik.EntityProviders;
using Quik.EntityProviders.RequestContainers;
using Quik.EntityProviders.Resolvers;
using Quik.EntityProviders.Resolvers.Fakes;
using TradingConcepts;

using Assert = NUnit.Framework.Assert;
using TimeoutAttribute = NUnit.Framework.TimeoutAttribute;

namespace QuikLuaWrapperTests.EntityProvidersTests
{

    public class TransactionsProviderTests
    {
        [Test, Timeout(1000)]
        public void OrderChangedInvokationTest()
        {
            using (var _ = ShimsContext.Create())
            {
                static void test(OrderSubmissionTestContext context)
                {
                    bool orderChangedInvoked = false;
                    bool orderUpdatedInvoked = false;

                    TransactionsProvider.Instance.OrderChanged = (order) => orderChangedInvoked = true;

                    TransactionsProvider.Instance.PlaceNew(context.Order);

                    context.Order.Updated += () => orderUpdatedInvoked = true;

                    // give callbacks some time to complete
                    Task.Delay(100).Wait();

                    Assert.That(orderChangedInvoked, Is.True);
                    Assert.That(orderUpdatedInvoked, Is.True);
                }

                OrderSubmissionTestContextFactory
                    .SetupOrderSuccessfullyQueuedContext()
                    .BeginTest(test);
            }
        }

        [Test, Timeout(1000)]
        public void ActiveStatusOnNewOrderSubmissionTest()
        {
            using (var _ = ShimsContext.Create())
            {
                static void test(OrderSubmissionTestContext context)
                {
                    bool callbackInvoked = false;
                    TransactionsProvider.Instance.OrderChanged = (order) => callbackInvoked = true;

                    TransactionsProvider.Instance.PlaceNew(context.Order);
                    Assert.That(context.Order.State, Is.EqualTo(OrderStates.Registering));

                    // give callbacks some time to complete
                    Task.Delay(100).Wait();

                    Assert.That(callbackInvoked, Is.True);
                    Assert.That(context.Order.State, Is.EqualTo(OrderStates.Active));
                }

                OrderSubmissionTestContextFactory
                    .SetupOrderSuccessfullyQueuedContext()
                    .BeginTest(test);
            }
        }
    }
}
using Broker;
using DecisionMakingService;
using DecisionMakingService.Strategies.PairTrading;
using MarketExecutionService;
using Moq;
using NUnit.Framework;
using Tools;
using TradingConcepts;
using TradingConcepts.SecuritySpecifics;

namespace DecisionMakingServiceUnitTests
{
    public class InitializationTests
    {
        private static ITradingAccount _account;
        private static ISecurity _secNearExpiry;
        private static ISecurity _secFarExpiry;

        [SetUp]
        public void Setup()
        {
            var broker = new Mock<IBroker>();

            broker
                .Setup(b => b.HostId)
                .Returns(() => new Guid("046B7276-342A-4C45-937F-8861E1174972"));

            DI.RegisterInstance(broker.Object);

            var execution = new Mock<IMarketExecutionService>();

            DI.RegisterInstance(execution.Object);

            var acc = new Mock<ITradingAccount>();

            acc.Setup(a => a.AccountCode)
                .Returns(() => "SPBFUT00AA");

            var nearSec = new Mock<IFutures>();

            nearSec
                .Setup(s => s.Ticker)
                .Returns(() => "BRX3");

            nearSec
                .Setup(s => s.Expiry)
                .Returns(() => new DateTime(2023, 11, 3));

            var farSec = new Mock<IFutures>();

            farSec
                .Setup(s => s.Ticker)
                .Returns(() => "BRZ3");

            farSec
                .Setup(s => s.Expiry)
                .Returns(() => new DateTime(2023, 12, 3));

            _account = acc.Object;
            _secNearExpiry = nearSec.Object;
            _secFarExpiry = farSec.Object;
        }

        [Test]
        public void EnsureSuccessfullyCreatedFromParameters()
        {
            var service = new DecisionMakingService.DecisionMakingService() as IDecisionMakingService;

            service.AddStrategy(new PairQuotingStrategyConfiguration()
            {
                Account = _account,
                BaseSecurity = _secNearExpiry,
                QuotedSecurity = _secFarExpiry,
                OrderSize = 1,
                PositionLimit = 1,
            });

            Assert.IsTrue(service.RunningStrategies.First() is PairQuotingStrategy);
        }
    }
}
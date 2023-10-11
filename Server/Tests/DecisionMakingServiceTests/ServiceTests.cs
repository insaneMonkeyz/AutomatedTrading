using Broker;
using DecisionMakingService;
using DecisionMakingService.Strategies;
using DecisionMakingService.Strategies.PairTrading;
using MarketExecutionService;
using Moq;
using Tools;
using TradingConcepts;
using TradingConcepts.SecuritySpecifics;

namespace DecisionMakingServiceUnitTests
{
    public class ServiceTests
    {
        private PairQuotingStrategyConfiguration _defaultStrategyConfiguration;
        private IDecisionMakingService _service;
        private ITradingAccount _account;
        private ISecurity _secNearExpiry;
        private ISecurity _secFarExpiry;

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
            _defaultStrategyConfiguration = new PairQuotingStrategyConfiguration()
            {
                Account = _account,
                BaseSecurity = _secNearExpiry,
                QuotedSecurity = _secFarExpiry,
                OrderSize = 1,
                PositionLimit = 1,
            };
        }

        [Test]
        public void StrategyAddedToListTest()
        {
            _service = new DecisionMakingService.DecisionMakingService() as IDecisionMakingService;
            _service.AddStrategy(_defaultStrategyConfiguration);

            Assert.IsTrue(_service.Strategies.First() is PairQuotingStrategy);
        }
        [Test]
        public void CantAddUnknownStrategyTest()
        {
            var unknownStrategyConfig = new Mock<ITradingStrategyConfiguration>().Object;

            _service = new DecisionMakingService.DecisionMakingService() as IDecisionMakingService;

            var additionResult = _service.AddStrategy(unknownStrategyConfig);

            Assert.IsTrue(additionResult.IsError);
        }
        [Test]
        public void StrategyRemovedFromListTest()
        {
            StrategyAddedToListTest();

            var strategy = _service.Strategies.First();

            _service.RemoveStrategy(strategy);

            Assert.That(_service.Strategies, Is.Empty);
        }
        [Test]
        public void CannotRemoveUnknownStrategy()
        {
            StrategyAddedToListTest();

            var strategy = new Mock<ITradingStrategy>().Object;
            var removalResult = _service.RemoveStrategy(strategy);

            Assert.IsTrue(removalResult.IsError);
        }
    }
}
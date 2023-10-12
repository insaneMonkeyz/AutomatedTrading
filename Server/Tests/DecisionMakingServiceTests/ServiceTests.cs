using AppComponents.Messaging.Results;
using Broker;
using DecisionMakingService;
using DecisionMakingService.Strategies;
using DecisionMakingService.Strategies.Fakes;
using DecisionMakingService.Strategies.PairTrading;
using MarketExecutionService;
using Microsoft.QualityTools.Testing.Fakes;
using Moq;
using Tools;
using TradingConcepts;
using TradingConcepts.SecuritySpecifics;

namespace DecisionMakingServiceUnitTests
{
    public interface ITestStrategy : ITradingStrategy, ITradingStrategyController, IConfigurable<ITradingStrategyConfiguration> { }
    public interface ISpecificTradingStrategyConfiguration : ITradingStrategyConfiguration { }
    public interface ISpecificTestStrategy : ITradingStrategy, ITradingStrategyController, IConfigurable<ISpecificTradingStrategyConfiguration> { }

    public class ServiceTests
    {
        private Guid _defaultStrategyId = new Guid("12347276-342A-4C45-937F-8861E1174972");
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

        private Result AddStrategy(FakesDelegates.Func<ITradingStrategyConfiguration, ITradingStrategy> create)
        {
            using var _ = ShimsContext.Create();

            ShimStrategiesFactory.CreateStrategyITradingStrategyConfiguration = create;

            _service = new DecisionMakingService.DecisionMakingService() as IDecisionMakingService;
            return _service.AddStrategy(new Mock<ITradingStrategyConfiguration>().Object);
        }
        private Result AddDefaultStrategy()
        {
            ITestStrategy create(ITradingStrategyConfiguration cfg)
            {
                var config = cfg;
                var fake = new Mock<ITestStrategy>();

                fake.Setup(s => s.Id)
                    .Returns(() => _defaultStrategyId);

                fake.Setup(s => s.Configuration)
                    .Returns(() => config);

                fake.Setup(s => s.Configure(It.IsAny<ITradingStrategyConfiguration>()))
                    .Callback<ITradingStrategyConfiguration>(c => config = c);

                return fake.Object;
            }

            return AddStrategy(create);
        }

        [Test]
        public void StrategyAddedToListTest()
        {
            var additionResult = AddDefaultStrategy();

            Assert.Multiple(() =>
            {
                Assert.That(additionResult.IsSuccess);
                Assert.That(additionResult.Data, Is.EqualTo(_service.Strategies.First()));
            });
        }
        [Test]
        public void CantAddUnknownStrategyTest()
        {
            var unknownStrategyConfig = new Mock<ITradingStrategyConfiguration>().Object;

            _service = new DecisionMakingService.DecisionMakingService() as IDecisionMakingService;

            var additionResult = _service.AddStrategy(unknownStrategyConfig);

            Assert.That(additionResult.IsError);
        }
        [Test]
        public void StrategyRemovedFromListTest()
        {
            AddDefaultStrategy();

            var strategy = _service.Strategies.First();

            _service.RemoveStrategy(strategy.Id);

            Assert.That(_service.Strategies, Is.Empty);
        }
        [Test]
        public void CannotRemoveUnknownStrategy()
        {
            AddDefaultStrategy();

            var removalResult = _service.RemoveStrategy(Guid.NewGuid());

            Assert.That(removalResult.IsError);
        }
        [Test]
        public void StrategyConfiguringTest()
        {
            AddDefaultStrategy();

            var strategy = _service.Strategies.First() as ITestStrategy;

            var cfg = new Mock<ITradingStrategyConfiguration>();
            cfg.Setup(x => x.Id).Returns(() => _defaultStrategyId);
            cfg.Setup(x => x.IsEnabled).Returns(() => true);

            var result = _service.ConfigureStrategy(cfg.Object);

            Assert.That(result.IsSuccess);
        }
        [Test]
        public void CannotConfigureUnknownStrategyTest()
        {
            AddDefaultStrategy();

            var strategy = _service.Strategies.First() as ITestStrategy;

            var cfg = new Mock<ITradingStrategyConfiguration>();
            cfg.Setup(x => x.Id).Returns(() => Guid.NewGuid());

            var result = _service.ConfigureStrategy(cfg.Object);

            Assert.That(result.IsError);
        }
        [Test]
        public void CannotConfigureWrongTypeStrategyTest()
        {
            AddDefaultStrategy();

            var cfg = new Mock<ISpecificTradingStrategyConfiguration>();
            cfg.Setup(x => x.Id).Returns(() => _defaultStrategyId);

            var result = _service.ConfigureStrategy(cfg.Object);

            Assert.That(result.IsError);
        }
    }
}
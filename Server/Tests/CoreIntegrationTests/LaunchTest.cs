using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core;
using DecisionMakingService;
using MarketDataProvisionService;
using MarketExecutionService;
using Moq;
using Quik;
using Tools;
using TradingConcepts.CommonImplementations;
using TradingConcepts.SecuritySpecifics;
using TradingConcepts;
using DecisionMakingService.Strategies.PairTrading;
using Broker;
using AppComponents;

namespace CoreIntegrationTests
{
    internal static class LaunchTest
    {
        public static IEnumerable<IService> GetServices()
        {
            yield return (IService)DI.Resolve<IMarketExecutionService>();
            yield return (IService)DI.Resolve<IMarketDataProvisionService>();
            yield return (IService)DI.Resolve<IDecisionMakingService>();
        }

        public static bool EnsureCorrectInitialization()
        {
            var quik = new Mock<IQuik>();
            DI.RegisterInstance(quik.Object);

            try
            {
                Core.Core.Initialize();
                return GetServices().All(s => s.Status == ServiceStatus.Running);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public static bool EnsureStrategyAddition()
        {
            var exec = DI.Resolve<IMarketExecutionService>();
            var logic = DI.Resolve<IDecisionMakingService>();
            var marketData = DI.Resolve<IMarketDataProvisionService>();

            var acc = new Mock<ITradingAccount>();
            var baseSec = new Mock<ISecurity>();
            var quoteSec = new Mock<ISecurity>();

            logic.AddStrategy(new PairQuotingStrategyConfiguration()
            {
                Account = acc.Object,
                BaseSecurity = baseSec.Object,
                QuotedSecurity = quoteSec.Object,
                OrderSize = 1,
                PositionLimit = 1,
            });

            logic.RunningStrategies
        }
    }
}

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
    }
}

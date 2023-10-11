using AppComponents;
using Broker;
using DecisionMakingService;
using MarketDataProvisionService;
using MarketExecutionService;
using Newtonsoft.Json.Linq;
using Tools.FileOperations;
using Tools.Logging;

namespace Core
{
    public class Core
    {
        private const string CONFIG_PATH = "AutomatedTrading.cfg";

        private static readonly Log _log = LogManagement.GetLogger<Core>();

        public static void Initialize()
        {
            if(!TextFileReader.TryReadFromFile(CONFIG_PATH, out JObject? configuration, out Exception? error))
            {
                _log.Error($"Failed to load the application's configuration from {CONFIG_PATH}. " +
                    "A default configuration will be created", error);
            }

            ServiceInitializerAttribute.Initialize<IBroker>(configuration);
            ServiceInitializerAttribute.Initialize<IMarketExecutionService>(configuration);
            ServiceInitializerAttribute.Initialize<IMarketDataProvisionService>(configuration);
            ServiceInitializerAttribute.Initialize<IDecisionMakingService>(configuration);
        }
    } 
}

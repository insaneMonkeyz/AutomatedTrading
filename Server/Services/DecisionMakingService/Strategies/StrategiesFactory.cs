using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DecisionMakingService.Strategies
{
    internal static class StrategiesFactory
    {
        public static ITradingStrategy CreateStrategy(ITradingStrategyConfiguration parameters)
        {
            return parameters switch
            {
                GrabStrategyConfiguration grab => CreateStrategy(grab),
                _ => throw new NotSupportedException($"Strategy with {parameters.GetType().Name} is not supported yet")
            };
        }

        public static ITradingStrategy CreateStrategy(GrabStrategyConfiguration parameters)
        {
            throw new NotImplementedException();
        }
    }
}

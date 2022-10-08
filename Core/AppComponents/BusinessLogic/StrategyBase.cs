using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.AppComponents.BusinessLogicConcepts;

namespace Core.AppComponents.BusinessLogic
{
    public abstract class StrategyBase : IStrategy
    {
        public StrategyBase(IAccountProvider accountProvider, IExecutionProvider executionProvider, IMarketDataProvider marketDataProvider, Guid id)
            : this(accountProvider,executionProvider,marketDataProvider)
        {
            if (id == default)
            {
                throw new ArgumentException("Id must not be empty", nameof(id));
            }

            Id = id;
        }
        public StrategyBase(IAccountProvider accountProvider, IExecutionProvider executionProvider, IMarketDataProvider marketDataProvider)
        {
            _accountProvider = accountProvider ?? throw new ArgumentNullException(nameof(accountProvider));
            _executionProvider = executionProvider ?? throw new ArgumentNullException(nameof(executionProvider));
            _marketDataProvider = marketDataProvider ?? throw new ArgumentNullException(nameof(marketDataProvider));
        }

        public Guid Id { get; } = Guid.NewGuid();
        public abstract string Name { get; set; }
        public abstract string Description { get; set; }
        public virtual bool IsEnabled { get; private set; }

        public abstract event StrategyAction StrategyAction;

        protected IAccountProvider _accountProvider;
        protected IExecutionProvider _executionProvider;
        protected IMarketDataProvider _marketDataProvider;

        public abstract void Disable();
        public abstract void Enable();
    }
}

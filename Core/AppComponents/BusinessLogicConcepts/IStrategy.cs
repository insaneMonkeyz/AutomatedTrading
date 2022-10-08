using System;

namespace Core.AppComponents.BusinessLogicConcepts
{
    public interface IStrategy
    {
        Guid Id { get; }
        string Name { get; }
        string Description { get; }
        bool IsEnabled { get; }
        void Enable();
        void Disable();
        event StrategyAction StrategyAction;
    }
}

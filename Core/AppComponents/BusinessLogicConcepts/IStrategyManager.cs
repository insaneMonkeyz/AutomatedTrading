using System.Collections.Generic;
using BasicConcepts;

namespace Core.AppComponents.BusinessLogicConcepts
{
    public interface IStrategyManager
    {
        ICollection<IStrategy> Strategies { get; }
        bool IsRunning { get; }
        void Start();
        void Stop();
    }
}

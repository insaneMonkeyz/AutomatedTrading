namespace BasicConcepts
{
    public delegate void StrategyActionEventHandler(IStrategy sender, IStrategyActionEventArgs e);
    public interface IStrategy
    {
        string Name { get; }
        string Description { get; }
        bool IsEnabled { get; }
        void Enable();
        void Disable();
        event StrategyActionEventHandler StrategyAction;
    }
}

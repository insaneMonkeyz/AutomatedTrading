namespace Core.AppComponents.BusinessLogicConcepts
{
    public delegate void StrategyAction(IStrategy sender, IStrategyActionEventArgs e);
    public interface IStrategyActionEventArgs
    {
    }
}

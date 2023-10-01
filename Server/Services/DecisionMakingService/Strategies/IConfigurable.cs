namespace DecisionMakingService.Strategies
{
    public interface IConfigurable<T>
    {
        T? Configuration { get; }
        void Configure(T? config);
    }
}

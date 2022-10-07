namespace BasicConcepts
{
    public interface IExpiring
    {
        DateTimeOffset Expiry { get; }
    }
}
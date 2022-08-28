namespace Core.Concepts.Entities
{
    public interface IExpiringContract
    {
        DateTimeOffset Expiry { get; }
    }
}
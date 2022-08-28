namespace Core.Concepts.Entities
{
    public interface IExpiring
    {
        DateTimeOffset Expiry { get; }
    }
}
namespace TradingConcepts
{
    public interface IQuote : IEquatable<IQuote>
    {
        Operations Operation { get; }
        Decimal5 Price { get; }
        long Size { get; }

    }
}
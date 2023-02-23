namespace TradingConcepts
{
    public interface ITrade
    {
        DateTimeOffset TimeStamp { get; }
        ISecurity Security { get; }
        Decimal5 Price { get; }
        Operations Operation { get; }
        long Size { get; }
    }
}
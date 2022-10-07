namespace BasicConcepts
{
    public interface IQuote
    {
        Decimal5 Price { get; }
        Operations Operation { get; }
        long Size { get; }
    }
}
using TradingConcepts;

namespace Quik.Entities
{
    internal class OrderQuote : IQuote
    {
        public Operations Operation { get; init; }
        public Decimal5 Price { get; init; }
        public long Size { get; init; }

        public bool Equals(IQuote? other)
        {
            return other is OrderQuote q &&
                q.Operation == Operation &&
                q.Price == Price &&
                q.Size == Size;
        }
    }
}

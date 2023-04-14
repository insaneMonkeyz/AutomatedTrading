using System.Diagnostics.CodeAnalysis;
using Quik.Entities;
using TradingConcepts;

namespace Quik.EntityProviders.RequestContainers
{
    /// <summary>
    /// Attention! This container must not be used to recover old orders! <para/>
    /// Its only purpose is to help find orders that were recently submitted and are expecting to be approved by the exchange
    /// </summary>
    internal struct TransactionRequestContainer : IRequestContainer<Order>, IEquatable<TransactionRequestContainer>
    {
        public string? Ticker;
        public Decimal5 Price;
        public long Size;

        public bool HasData
        {
            get => Ticker.HasValue()
                && Price != default
                && Size != default;
        }

        public bool IsMatching(Order? entity)
        {
            return entity != null
                && entity.State.HasFlag(OrderStates.Registering)
                && entity.Security.Ticker == Ticker
                && entity.Quote.Price == Price
                && entity.Quote.Size == Size;
        }

        public bool Equals(TransactionRequestContainer other)
        {
            return other.Ticker == Ticker
                && other.Price == Price
                && other.Size == Size;
        }
        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            return obj is TransactionRequestContainer container
                && container.Ticker == Ticker
                && container.Price == Price
                && container.Size == Size;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(Ticker, Price, Size);
        }
        public override string? ToString()
        {
            return $"{{{nameof(TransactionRequestContainer)}: {Ticker} {Size}x{Price}}}";
        }
    }
}

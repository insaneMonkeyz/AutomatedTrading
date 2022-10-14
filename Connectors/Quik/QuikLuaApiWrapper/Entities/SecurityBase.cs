using BasicConcepts;

namespace QuikLuaApi.Entities
{
    public abstract class SecurityBase : ISecurity
    {
        public string Ticker { get; init; }
        public string Description { get; init; }
        public string ClassCode { get; init; }
        public Guid ExchangeId { get; init; }
        public Decimal5 MinPriceStep { get; init; }
        public Decimal5 MinTradingSize { get; init; }
        public Currencies DenominationCurrency { get; init; }
    }
}
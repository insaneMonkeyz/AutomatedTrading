namespace BasicConcepts
{
    public interface ISecurity
    {
        Guid ExchangeId { get; } 
        string Ticker { get; }
        string Description { get; }

        Decimal5 MinPriceStep { get; }
        Decimal5 MinTradingSize { get; }
        Currencies DenominationCurrency { get; }
    }
}
namespace TradingConcepts
{
    public interface ISecurity
    {
        Guid ExchangeId { get; } 
        string Ticker { get; }
        string? Description { get; }

        /// <summary>
        /// Number of digits apter the point. PricePrecisionScale of 0.01 is 2
        /// </summary>
        long PricePrecisionScale { get; }
        long ContractSize { get; }
        Decimal5 MinPriceStep { get; }
        Decimal5? PriceStepValue { get; }
        Decimal5 MinTradingSize { get; }
        Currencies DenominationCurrency { get; }
    }
}
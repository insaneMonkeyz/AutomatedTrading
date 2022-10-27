namespace BasicConcepts
{
    public interface ISecurity
    {
        Guid ExchangeId { get; } 
        string Ticker { get; }
        string? Description { get; }

        /// <summary>
        /// Number of digits apter the point. E.g. 2 = 0.01
        /// </summary>
        long PricePrecisionScale { get; }
        long ContractSize { get; }
        Decimal5 MinPriceStep { get; }
        Decimal5? PriceStepValue { get; }
        Decimal5 MinTradingSize { get; }
        Currencies DenominationCurrency { get; }
    }
}
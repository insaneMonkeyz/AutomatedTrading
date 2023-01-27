﻿using BasicConcepts;

namespace Quik.Entities
{
    internal abstract class Security : ISecurity, IUniquelyIdentifiable
    {
        public Decimal5 MinTradingSize => QuikApi.QuikApi.DEFAULT_TRADING_SIZE;
        public Guid ExchangeId => QuikApi.QuikApi.MoexExchangeId;
        public abstract string ClassCode { get; }
        public string Ticker { get; init; }
        public string? Description { get; init; }
        public long PricePrecisionScale { get; init; }
        public long ContractSize { get; init; }
        public Decimal5 MinPriceStep { get; init; }
        public Currencies DenominationCurrency { get; init; }
        public Decimal5? PriceStepValue { get; internal set; }
        public int UniqueId
        {
            get => _uniqueId ??= 
                HashCode.Combine(nameof(Security), ClassCode, Ticker);
        }
        private int? _uniqueId;

        public Security(ref SecurityParamsContainer container)
        {
            Ticker = container.Ticker;
            Description = container.Description;
            MinPriceStep = container.MinPriceStep;
            ContractSize = container.ContractSize;
            PricePrecisionScale = container.PricePrecisionScale;
            DenominationCurrency = container.DenominationCurrency;
        }

        public override string ToString() => Ticker;
    }
}
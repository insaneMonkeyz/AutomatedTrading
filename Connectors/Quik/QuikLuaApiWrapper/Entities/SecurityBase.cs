﻿using BasicConcepts;

namespace QuikLuaApi.Entities
{
    internal abstract class SecurityBase : ISecurity
    {
        public Decimal5 MinTradingSize => QuikApi.DEFAULT_TRADING_SIZE;
        public Guid ExchangeId => QuikApi.MoexExchangeId;
        public abstract string ClassCode { get; }
        public string Ticker { get; init; }
        public string Description { get; init; }
        public Currencies DenominationCurrency { get; init; }
        public long PricePrecisionScale { get; init; }
        public long ContractSize { get; init; }
        public Decimal5 MinPriceStep { get; init; }

        public SecurityBase(SecurityParamsContainer container)
        {
            Ticker = container.Ticker;
            Description = container.Description;
            MinPriceStep = container.MinPriceStep;
            ContractSize = container.ContractSize;
            PricePrecisionScale = container.PricePrecisionScale;
            DenominationCurrency = container.DenominationCurrency;
        }
    }
}
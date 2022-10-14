﻿using BasicConcepts;

namespace QuikLuaApi.Entities
{
    public abstract class SecurityBase : ISecurity
    {
        public string Ticker { get; init; }
        public string Description { get; init; }
        public string ClassCode { get; init; }
        public Guid ExchangeId { get; init; }
        public Currencies DenominationCurrency { get; init; }
        public long PricePrecisionScale { get; init; }
        public long ContractSize { get; init; }
        public Decimal5 MinPriceStep { get; init; }
        public Decimal5 MinTradingSize => QuikApi.DEFAULT_TRADING_SIZE;
    }
}
using TradingConcepts;

namespace QuikGrpcTestClient.Entities
{
    internal class Security : ISecurity
    {
        private readonly Quik.Grpc.Entities.Security _security;
        private Currencies? _currency;
        private Guid? _exchangeId;

        internal Security(Quik.Grpc.Entities.Security security)
        {
            _security = security;
        }

        string ISecurity.Ticker => _security.Ticker;

        string? ISecurity.Description => _security.HasDescription ? _security.Description : default;

        long ISecurity.PricePrecisionScale => _security.PricePrecisionScale;

        long ISecurity.ContractSize => _security.ContractSize;

        Decimal5 ISecurity.MinPriceStep => Decimal5.FromMantissa(_security.MinPriceStep.Mantissa);

        Decimal5? ISecurity.PriceStepValue => Decimal5.FromMantissa(_security.PriceStepValue.Mantissa);

        Decimal5 ISecurity.MinTradingSize => _security.MinTradinSize;

        Guid ISecurity.ExchangeId
        {
            get
            {
                return _exchangeId ??= Guid.Parse(_security.ExchangeId.Value);
            }
        }

        Currencies ISecurity.DenominationCurrency
        {
            get
            {
                return _currency ??= Enum.Parse<Currencies>(_security.DenominationCurrency);
            }
        }
    }
}

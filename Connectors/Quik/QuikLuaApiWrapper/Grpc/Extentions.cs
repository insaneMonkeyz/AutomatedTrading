using Google.Protobuf.Collections;
using TradingConcepts;

namespace Quik.Grpc
{
    public static class Extentions
    {
        public static Entities.Primitives.Decimal5 ToGrpcType(this Decimal5 value)
        {
            return new()
            {
                Mantissa = value.InternalValue
            };
        }

        public static Entities.Primitives.ProtoGuid ToGrpcType(this Guid value)
        {
            return new()
            {
                Value = value.ToString()
            };
        }

        public static Entities.Security ToGrpcType(this ISecurity source)
        {
            var result = new Entities.Security()
            {
                ContractSize = source.ContractSize,
                DenominationCurrency = source.DenominationCurrency.ToString(),
                Description = source.Description,
                ExchangeId = source.ExchangeId.ToGrpcType(),
                MinPriceStep = source.MinPriceStep.ToGrpcType(),
                MinTradinSize = source.MinTradingSize,
                PricePrecisionScale = source.PricePrecisionScale,
                Ticker = source.Ticker,
            };

            if (source.PriceStepValue.HasValue)
            {
                result.PriceStepValue = source.PriceStepValue.Value.ToGrpcType();
            }

            if (source.Description is string description)
            {
                result.Description = description;
            }

            return result;
        }

        public static Entities.SecurityBalance ToGrpcType(this ISecurityBalance source)
        {
            return new()
            {
                Amount = source.Amount,
                Security = source.Security.ToGrpcType(),
            };
        }
    }
}

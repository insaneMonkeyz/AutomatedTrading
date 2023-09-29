using TradingConcepts;
using TradingConcepts.SecuritySpecifics;
using TradingConcepts.SecuritySpecifics.Options;

namespace Quik.Entities
{
    internal class Option : MoexDerivativeBase, IOption
    {
        public override string ClassCode => MoexSpecifics.OPTIONS_CLASS_CODE;
        public Decimal5 Strike { get; init; }
        public OptionTypes OptionType { get; }
        public ISecurity? Underlying { get; init; }

        public double IV => throw new NotImplementedException();
        public double Delta => throw new NotImplementedException();
        public double Vega => throw new NotImplementedException();
        public double Gamma => throw new NotImplementedException();
        public double Theta => throw new NotImplementedException();

        public Option(ref SecurityParamsContainer container) : base(ref container)
        {
            OptionType = InferOptionType(Ticker);
        }

        private static OptionTypes InferOptionType(string ticker)
        {
            // Si65000BL2D

            //  01   23456   7  8   9  10
            // [Si] [65000] [B][L] [2] [D]

            // [7]B - American margin type
            // [8]A-L inclusive = calls
            // [8]M-X inclusive = puts

            var indexOfMarginType = ticker.LastIndexOf('B');
            var codeIndex = indexOfMarginType + 1;

            if (indexOfMarginType > -1 && codeIndex < ticker.Length)
            {
                var code = ticker[codeIndex];

                if (code >= 'A' && code <= 'X')
                {
                    return code <= 'L'
                        ? OptionTypes.Call
                        : OptionTypes.Put;
                }
                else if (code >= 'a' && code <= 'x')
                {
                    return code <= 'l'
                        ? OptionTypes.Call
                        : OptionTypes.Put;
                }
            }

            throw new Exception($"Could not resolve option type from ticker {ticker}");
        }

        public override bool Equals(object? obj)
        {
            return obj is Option option
                && option.Strike == Strike
                && option.OptionType == OptionType
                && (option.Underlying?.Equals(Underlying) ?? Underlying is null);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(Strike, OptionType, Underlying, 2999);
        }
    }
}
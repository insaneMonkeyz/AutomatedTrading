using BasicConcepts;
using BasicConcepts.SecuritySpecifics;
using BasicConcepts.SecuritySpecifics.Options;

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
            OptionType = GetOptionType(Ticker);

            if (OptionType == OptionTypes.Undefined)
            {
                throw new Exception("Failed to figure out option type from ticker " + Ticker);
            }
        }

        private static OptionTypes GetOptionType(string ticker)
        {
            var span = ticker.AsSpan();

            // Si65000BL2D

            //  01   23456   7  8   9  10
            // [Si] [65000] [B][L] [2] [D]

            // [7]B - American margin type
            // [8]A-L inclusive = calls
            // [8]M-X inclusive = puts

            var bIndex = ticker.LastIndexOf('B');
            var codeIndex = bIndex + 1;

            if (bIndex > -1 && codeIndex < ticker.Length)
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

            return OptionTypes.Undefined;
        }
    }
}
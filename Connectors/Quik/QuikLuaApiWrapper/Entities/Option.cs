using BasicConcepts;
using BasicConcepts.SecuritySpecifics.Options;

namespace QuikLuaApi.Entities
{
    internal class Option : IOption
    {
        public Decimal5 Strike { get; init; }

        public OptionTypes OptionType { get; init; }

        public ISecurity Underlying { get; init; }

        public string Ticker { get; init; }

        public string Description { get; init; }

        public DateTimeOffset Expiry { get; init; }
    }
}
using BasicConcepts;
using BasicConcepts.SecuritySpecifics.Options;

namespace QuikLuaApi.Entities
{
    internal class Option : SecurityBase, IOption
    {
        public Decimal5 Strike { get; init; }

        public OptionTypes OptionType { get; init; }

        public ISecurity Underlying { get; init; }

        public DateTimeOffset Expiry { get; init; }
    }
}
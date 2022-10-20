using BasicConcepts;
using BasicConcepts.SecuritySpecifics.Options;

namespace QuikLuaApi.Entities
{
    internal class Option : SecurityBase, IOption
    {
        public override string ClassCode => QuikApi.QuikApi.OPTIONS_CLASS_CODE;
        public Decimal5 Strike { get; init; }
        public OptionTypes OptionType { get; init; }
        public ISecurity Underlying { get; init; }
        public DateTimeOffset Expiry { get; init; }

        public Option(SecurityParamsContainer container) : base(container)
        {

        }
    }
}
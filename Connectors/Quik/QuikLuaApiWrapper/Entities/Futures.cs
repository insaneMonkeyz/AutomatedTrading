using BasicConcepts;
using BasicConcepts.SecuritySpecifics;

namespace QuikLuaApi.Entities
{
    internal class Futures : IFutures
    {
        public ISecurity Underlying { get; init; }
        public string Ticker { get; init; }
        public string Description { get; init; }
        public DateTimeOffset Expiry { get; init; }
    }
}
using BasicConcepts;
using BasicConcepts.SecuritySpecifics;

namespace QuikLuaApi.Entities
{
    internal class Futures : SecurityBase, IFutures
    {
        public ISecurity Underlying { get; init; }
        public DateTimeOffset Expiry { get; init; }
    }
}
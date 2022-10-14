using BasicConcepts;

namespace QuikLuaApi.Entities
{
    internal class Stock : ISecurity
    {
        public string Ticker { get; init; }
        public string Description { get; init; }
        public string ClassCode { get; init; }
    }
}
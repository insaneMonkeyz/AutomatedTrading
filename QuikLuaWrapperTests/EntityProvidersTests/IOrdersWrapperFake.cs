using Quik.EntityProviders.QuikApiWrappers;
using TradingConcepts;

namespace QuikLuaWrapperTests.EntityProvidersTests
{
    internal interface IOrdersWrapperFake
    {
        long Rest { get; }
        long ExchangeOrderId { get; }
        long TransactionId { get; }
        string Ticker { get; }
        string? ClassCode { get; }
        string? Account { get; }
        OrderFlags Flags { get; }
        MoexOrderExecutionModes OrderExecutionMode { get; }
        DateTimeOffset? Expiry { get; }
        Decimal5 Price { get; }
        long Size { get; }

    }
}
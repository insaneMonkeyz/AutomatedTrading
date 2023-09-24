using static Quik.EntityProviders.QuikApiWrappers.TransactionWrapper;

namespace QuikLuaWrapperTests.EntityProvidersTests
{
    internal interface ITransactionWrapperFake
    {
        string? PlaceNewOrder(ref NewOrderArgs args);
        TransactionStatus Status { get; }
        ErrorSite ErrorSource { get; }
        long Id { get; }
        long OrderId { get; }
        long RejectedSize { get; }
        string ClassCode { get; }
        string ResultDescription { get; }
    }
}
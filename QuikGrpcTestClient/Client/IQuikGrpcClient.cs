using TradingConcepts;

namespace QuikGrpcTestClient.Client
{
    internal interface IQuikGrpcClient
    {
        bool IsConnected();
        ITradingAccount GetTradingAccount();
    }
}
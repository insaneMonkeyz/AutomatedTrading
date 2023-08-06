using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Quik.Grpc.Entities;
using Quik.Grpc.Prototypes;

namespace Quik.Grpc.Services
{
    public interface IQuikService
    {
        public Task<QuikConnectionStatusResponse> IsConnected(Empty _, ServerCallContext context);
        public Task<TradingAccount> GetTradingAccount(Empty _, ServerCallContext context);
    }
}
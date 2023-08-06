using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Quik.Grpc;
using QuikGrpcTestClient.Entities;
using TradingConcepts;

namespace QuikGrpcTestClient.Client
{
    internal class QuikGrpcClient : QuikApi.QuikApiClient, IQuikGrpcClient
    {
        private static readonly Empty _emptyRequest = new();

        public QuikGrpcClient(ChannelBase channel) : base(channel) { }

        public bool IsConnected()
        {
            return IsConnected(_emptyRequest).IsConnected;
        }

        public ITradingAccount GetTradingAccount()
        {
            return new TradingAccount(GetTradingAccount(_emptyRequest));
        }
    }
}
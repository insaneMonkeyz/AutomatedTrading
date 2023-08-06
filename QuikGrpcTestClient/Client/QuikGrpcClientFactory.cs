using Grpc.Core;

namespace QuikGrpcTestClient.Client
{
    internal static class QuikGrpcClientFactory
    {
        public static IQuikGrpcClient CreateClient(ChannelBase channel) => new QuikGrpcClient(channel);
    }
}
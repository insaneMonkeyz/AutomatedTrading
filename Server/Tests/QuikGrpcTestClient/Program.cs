using Grpc.Net.Client;
using QuikGrpcTestClient.Client;

namespace QuikGrpcTestClient
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Provide port to connect to");

            string? portInput;
            int grpcPort;
            do
            {
                portInput = Console.ReadLine();
            } 
            while (!int.TryParse(portInput, out grpcPort));

            using var channel = GrpcChannel.ForAddress($"http://localhost:{grpcPort}");

            var client = QuikGrpcClientFactory.CreateClient(channel);
            var reply = client.GetTradingAccount();

            Console.WriteLine(reply.AccountCode);
            Console.ReadLine();
        }
    }
}
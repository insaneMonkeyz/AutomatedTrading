using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Quik.Grpc.Entities;
using Quik.Grpc;
using Microsoft.Extensions.Logging;
using Quik;

namespace Quik.Grpc.Services
{
    internal class QuikService : QuikApi.QuikApiBase, IQuikService
    {
        private readonly ILogger<QuikService> _logger;
        private readonly IQuik _quik;

        public QuikService(ILogger<QuikService> logger)
        {
            _logger = logger;
            _quik = Tools.DI.Resolve<IQuik>() ?? throw new Exception("failed to resolve " + nameof(IQuik));
        }

        public override Task<TradingAccount> GetTradingAccount(Empty _, ServerCallContext context)
        {
            return Task.Run(GetTradingAccount, context.CancellationToken);
        }
        public override Task<QuikConnectionStatusResponse> IsConnected(Empty _, ServerCallContext context)
        {
            return Task.Run(GetConnectionStatus, context.CancellationToken);
        }

        private QuikConnectionStatusResponse GetConnectionStatus()
        {
            return new QuikConnectionStatusResponse
            {
                IsConnected = _quik.IsConnected
            };
        }

        private TradingAccount GetTradingAccount()
        {
            var account = _quik.Account;

            if (account == null)
            {
                return null;
            }

            var result = new TradingAccount
            {
                AccountCode = account.AccountCode,
                AccountCurrency = account.AccountCurrency.ToString(),
                CollateralMargin = account.CollateralMargin.ToGrpcType(),
                FloatingIncome = account.FloatingIncome.ToGrpcType(),
                TotalFunds = account.TotalFunds.ToGrpcType(),
                UnusedFunds = account.UnusedFunds.ToGrpcType(),
            };

            if (account.Description is string description)
            {
                result.Description = description;
            }

            result.SecurityBalance.AddRange(
                account.SecuritiesBalance.Select(Extentions.ToGrpcType));

            return result;
        }
    }
}
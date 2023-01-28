using System.Reflection;
using Quik.Entities;
using Quik.EntityDataProviders.RequestContainers;

namespace Quik.EntityDataProviders
{
    internal static class EntityResolvers
    {
        private static EntityResolver<SecurityBalanceRequestContainer, SecurityBalance>? _balanceResolver;
        private static EntityResolver<SecurityRequestContainer, Security>? _securityResolver;
        private static EntityResolver<AccountRequestContainer, DerivativesTradingAccount>? _accountsResolver;
        private static EntityResolver<OrderbookRequestContainer, OrderBook>? _orderbooksResolver;
        private static EntityResolver<OrderRequestContainer, Order>? _ordersResolver;
        private static EntityResolver<OrderExecutionRequestContainer, OrderExecution>? _orderExecutionsResolver;

        public static EntityResolver<SecurityBalanceRequestContainer, SecurityBalance> GetBalanceResolver()
        {
            return _balanceResolver ??= new(20, DerivativesBalanceDataProvider.Instance.Create);
        }
        public static EntityResolver<SecurityRequestContainer, Security> GetSecurityResolver()
        {
            throw new NotImplementedException("Need to implement fetching from quik");
            return _securityResolver ??= new(500, null);
        }
        public static EntityResolver<AccountRequestContainer, DerivativesTradingAccount> GetAccountsResolver()
        {
            return _accountsResolver ??= new(5, AccountDataProvider.Instance.Create);
        }
        public static EntityResolver<OrderbookRequestContainer, OrderBook> GetOrderbooksResolver()
        {
            return _orderbooksResolver ??= new(10, OrderbookDataProvider.Instance.Create);
        }
        public static EntityResolver<OrderRequestContainer, Order> GetOrdersResolver()
        {
            return _ordersResolver ??= new(10_000, OrderDataProvider.Instance.Create);
        }
        public static EntityResolver<OrderExecutionRequestContainer, OrderExecution> GetOrderExecutionsResolver()
        {
            return _orderExecutionsResolver ??= new(1_000, null);
        }

        public static EntityResolver<TRequest, TEntity> GetResolver<TRequest, TEntity>()
            where TEntity : class
            where TRequest : IRequestContainer<TEntity>
        {
#pragma warning disable CS8603

            return typeof(EntityResolvers)
                .GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Where(m => m.ReturnType == typeof(EntityResolver<TRequest, TEntity>))
                .First()
                .Invoke(null, null) as EntityResolver<TRequest, TEntity>;

#pragma warning restore
        }
    }
}

using System.Reflection;
using Quik.Entities;
using Quik.EntityProviders.RequestContainers;

namespace Quik.EntityProviders
{
    internal static class EntityResolvers
    {
        private static EntityResolver<OrderExecutionRequestContainer, OrderExecution>? _orderExecutionsResolver;
        private static EntityResolver<AccountRequestContainer, DerivativesTradingAccount>? _accountsResolver;
        private static EntityResolver<SecurityBalanceRequestContainer, SecurityBalance>? _balanceResolver;
        private static EntityResolver<OrderbookRequestContainer, OrderBook>? _orderbooksResolver;
        private static EntityResolver<OrderRequestContainer, Order>? _ordersResolver;
        private static SecurityResolver? _securityResolver;

        private static readonly object _securityResolverInitLock = new();

        public static EntityResolver<SecurityBalanceRequestContainer, SecurityBalance> GetBalanceResolver()
        {
            return _balanceResolver ??= new(20, DerivativesBalanceProvider.Instance.Create);
        }
        public static SecurityResolver GetSecurityResolver()
        {
            lock (_securityResolverInitLock)
            {
                if (_securityResolver == null)
                {
                    var map = new SecuritiesToClasscodesMap(
                        SecuritiesProvider.GetAvailableClasses,
                        SecuritiesProvider.GetAvailableSecuritiesOfType);

                    SecuritiesProvider.OnInitialized += map.Initialize;

                    _securityResolver = new SecurityResolver(SecuritiesProvider.Create, map);
                }

                return _securityResolver;
            } 
        }
        public static EntityResolver<AccountRequestContainer, DerivativesTradingAccount> GetAccountsResolver()
        {
            return _accountsResolver ??= new(5, AccountsProvider.Instance.Create);
        }
        public static EntityResolver<OrderbookRequestContainer, OrderBook> GetOrderbooksResolver()
        {
            return _orderbooksResolver ??= new(10, OrderbooksProvider.Instance.Create);
        }
        public static EntityResolver<OrderRequestContainer, Order> GetOrdersResolver()
        {
            return _ordersResolver ??= new(10_000, OrdersProvider.Instance.Create);
        }
        public static EntityResolver<OrderExecutionRequestContainer, OrderExecution> GetOrderExecutionsResolver()
        {
            return _orderExecutionsResolver ??= new(1_000, default);
        }
        
        public static EntityResolver<TRequest, TEntity> GetResolver<TRequest, TEntity>()
            where TEntity : class
            where TRequest : struct, IRequestContainer<TEntity>
        {
#pragma warning disable CS8603

            return typeof(EntityResolvers)
                .GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Where(m => m.ReturnType == typeof(EntityResolver<TRequest, TEntity>))
                .First()
                .Invoke(null, null) as EntityResolver<TRequest, TEntity>;

#pragma warning restore CS8603
        }
    }
}

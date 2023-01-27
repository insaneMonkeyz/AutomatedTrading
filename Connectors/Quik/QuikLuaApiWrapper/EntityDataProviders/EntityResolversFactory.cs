using Quik.Entities;
using Quik.EntityDataProviders.RequestContainers;

namespace Quik.EntityDataProviders
{
    internal static class EntityResolversFactory
    {
        private static EntityResolver<SecurityBalanceRequestContainer, SecurityBalance>? _balanceResolver;
        private static EntityResolver<SecurityRequestContainer, Security>? _securityResolver;
        private static EntityResolver<AccountRequestContainer, DerivativesTradingAccount>? _accountsResolver;
        private static EntityResolver<OrderbookRequestContainer, OrderBook>? _orderbooksResolver;
        private static EntityResolver<OrderRequestContainer, Order>? _ordersResolver;
        private static EntityResolver<OrderExecutionRequestContainer, OrderExecution>? _orderExecutionsResolver;

        public static EntityResolver<SecurityBalanceRequestContainer, SecurityBalance> GetBalanceResolver()
        {
            return _balanceResolver ??= new(20);
        }
        public static EntityResolver<SecurityRequestContainer, Security> GetSecurityResolver()
        {
            return _securityResolver ??= new(500);
        }
        public static EntityResolver<AccountRequestContainer, DerivativesTradingAccount> GetAccountsResolver()
        {
            return _accountsResolver ??= new(5);
        }
        public static EntityResolver<OrderbookRequestContainer, OrderBook> GetOrderbooksResolver()
        {
            return _orderbooksResolver ??= new(10);
        }
        public static EntityResolver<OrderRequestContainer, Order> GetOrdersResolver()
        {
            return _ordersResolver ??= new(10_000);
        }
        public static EntityResolver<OrderExecutionRequestContainer, OrderExecution> GetOrderExecutionsResolver()
        {
            return _orderExecutionsResolver ??= new(1_000);
        }
    }
}

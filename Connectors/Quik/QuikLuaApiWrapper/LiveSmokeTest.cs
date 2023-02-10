using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BasicConcepts;
using BasicConcepts.SecuritySpecifics;
using Quik.Entities;
using Quik.EntityProviders;
using Quik.EntityProviders.RequestContainers;
using Quik.Lua;

namespace Quik
{
    internal class LiveSmokeTest
    {
        private static class OrderbookProbes
        {
            public static bool NewBookReceived;
            public static bool OrderBookUpdated;
            public static bool OrderBookCreated;
        }
        private static class SecurityProbes
        {
            public static bool GetClasses;
            public static bool GetSecuritiesOfType;
            public static bool GetSecurityOfKnownClass;
            public static bool GetSecurityOfUnknownClass;
            public static bool SecurityUpdatesReceived;
            public static bool NewSecurityReceived;
        }

        private readonly string[] _approvedTickers = new[]
        {
            "SiH3",
            "BRH3",
            "BRJ3",
            "BRM3",
            "BRN3",
            "BRH3BRJ3"
        };

        private ExecutionLoop _notificationsLoop;

        public static LiveSmokeTest Instance { get; } = new();

        private LiveSmokeTest() 
        {
            _notificationsLoop = new ExecutionLoop();
        }

        public void Initialize()
        {
            SecuritiesProvider.SubscribeCallback();
            OrderbooksProvider.Instance.SubscribeCallback();
            //AccountsProvider.Instance.SubscribeCallback();
            //DerivativesBalanceProvider.Instance.SubscribeCallback();
            //OrdersProvider.Instance.SubscribeCallback();
            //ExecutionsProvider.Instance.SubscribeCallback();
        }
        public void Begin()
        {
            SecuritiesProvider.NewEntity = (sec) =>
            {
                SecurityProbes.NewSecurityReceived = true;
            };
            SecuritiesProvider.EntityChanged = (sec) =>
            {
                SecurityProbes.SecurityUpdatesReceived = true;
            };

            OrderbooksProvider.Instance.NewEntity = (book) =>
            {
                SecurityProbes.NewSecurityReceived = true;
            };
            OrderbooksProvider.Instance.EntityChanged = (book) =>
            {
                SecurityProbes.NewSecurityReceived = true;
            };

            OrderbooksProvider.Instance.CreationIsApproved = (ref OrderbookRequestContainer request) =>
            {
                return IsApproved(ref request.SecurityRequest);
            };
            SecuritiesProvider.CreationIsApproved = (ref SecurityRequestContainer request) =>
            {
                return IsApproved(ref request);
            };

            OrderbooksProvider.Instance.Initialize(_notificationsLoop);
            SecuritiesProvider.Initialize(_notificationsLoop);

            Task.Run(_notificationsLoop.Enter);

            for (int i = 0; i < 5; i++)
            {
                Api.lua_pushstring(Quik.Lua, "AUTOMATED TRADING");
            }

            SecurityProbes.GetClasses = SecuritiesProvider.GetAvailableClasses().Any();
            SecurityProbes.GetSecuritiesOfType = SecuritiesProvider.GetAvailableSecuritiesOfType(typeof(IFutures)).Any();

            var securityResolver = EntityResolvers.GetSecurityResolver();
            var brmRequest = new SecurityRequestContainer
            {
                Ticker = "BRM3"
            };
            var brnRequest = new SecurityRequestContainer
            {
                ClassCode = MoexSpecifics.FUTURES_CLASS_CODE,
                Ticker = "BRN3",
            };

            SecurityProbes.GetSecurityOfUnknownClass = securityResolver.Resolve(ref brmRequest) is Security brm;
            SecurityProbes.GetSecurityOfKnownClass = securityResolver.Resolve(ref brnRequest) is Security brn;

            var brjBookRequest = new OrderbookRequestContainer
            {
                SecurityRequest = new()
                {
                    Ticker = "BRJ3"
                }
            };

            OrderbookProbes.OrderBookCreated = 
                OrderbooksProvider.Instance.Create(ref brjBookRequest) 
                    is OrderBook brjBook;

            //OrdersProvider.Instance.Initialize();
            //ExecutionsProvider.Instance.Initialize();

            //foreach (var subscriber in SingletonProvider.GetInstances<IQuikDataSubscriber>())
            //{
            //    subscriber.SubscribeCallback(_localState);
            //}

            //-------------------------------------------------------------------------
            // PASSED
            //var accResolver = EntityResolvers.GetAccountsResolver();

            //var account = AccountsProvider.Instance.GetAllEntities().FirstOrDefault(acc => acc.IsMoneyAccount);

            //if (account != null)
            //{
            //    Debug.Print($"-- {DateTime.Now:T} {account}");
            //}

            //AccountsProvider.Instance.EntityChanged = (acc) =>
            //{
            //    Debug.Print($"-- {DateTime.Now:T} ENTITY CHANGED: {acc}");
            //};
            //-------------------------------------------------------------------------

            //-------------------------------------------------------------------------
            // PASSED
            //var pos = DerivativesBalanceProvider.Instance.GetAllEntities();

            //foreach (var p in pos)
            //{
            //    Debug.Print($"-- {DateTime.Now:T} {p}");
            //}

            //DerivativesBalanceProvider.Instance.EntityChanged = (balance) =>
            //{
            //    Debug.Print($"-- {DateTime.Now:T} ENTITY CHANGED: {balance}");
            //};
            //DerivativesBalanceProvider.Instance.NewEntity = (balance) =>
            //{
            //    Debug.Print($"-- {DateTime.Now:T} NEW ENTITY: {balance}");
            //};
            //-------------------------------------------------------------------------

            //var orders = OrdersProvider.Instance.GetAllEntities();
            //var ordersResolver = EntityResolvers.GetOrdersResolver();

            //foreach (var order in orders)
            //{
            //    Debug.Print($"-- {DateTime.Now:O} LOADED {order}");
            //}

            //OrdersProvider.Instance.NewEntity = (order) =>
            //{
            //    Debug.Print($"-- {DateTime.Now:O} NEW ORDER {order}");
            //};
            //OrdersProvider.Instance.EntityChanged = (order) =>
            //{
            //    Debug.Print($"-- {DateTime.Now:O} ORDER CHANGED {order}");
            //};

            //var execs = ExecutionsProvider.Instance.GetAllEntities();

            //foreach (var exec in execs)
            //{
            //    Debug.Print($"-- {DateTime.Now:O} LOADED {exec}");
            //}
            //ExecutionsProvider.Instance.NewEntity = (exec) =>
            //{
            //    Debug.Print($"-- {DateTime.Now:O} NEW ENTITY {exec}");
            //};

            //void printbook(Quote[] bids, Quote[] asks, long depth)
            //{
            //    var maxdepth = Math.Min(depth, 5);

            //    if (maxdepth <= 0)
            //    {
            //        "Zero size orderbook received".DebugPrintWarning();
            //    }

            //    var sb = new StringBuilder();

            //    sb.AppendLine("-------------------------");

            //    for (int i = 0; i < maxdepth; i++)
            //    {
            //        sb.AppendLine(asks[i].ToString());
            //    }

            //    for (int i = 0; i < maxdepth; i++)
            //    {
            //        sb.AppendLine(bids[i].ToString());
            //    }

            //    sb.AppendLine("-------------------------");

            //    Debug.Print(sb.ToString());
            //};

            //OrderbooksProvider.Instance.EntityChanged = (b) =>
            //{
            //    b.UseQuotes(printbook);
            //};

            //var bookRequest = new OrderbookRequestContainer
            //{
            //    SecurityRequest = new()
            //    {
            //        Ticker = "SiH3",
            //        ClassCode = MoexSpecifics.FUTURES_CLASS_CODE
            //    }
            //};
            //var book = OrderbooksProvider.Instance.Create(ref bookRequest);

            //book?.UseQuotes(printbook);
        }

        public void Complete() 
        {
            _notificationsLoop.Abort();
            PrintResults(typeof(SecurityProbes));
            PrintResults(typeof(OrderbookProbes));
        }

        private static void PrintResults(Type probesContainer)
        {
            var sb = new StringBuilder();

            foreach (var field in probesContainer.GetFields(BindingFlags.Static))
            {
                sb.AppendLine(field.Name);
                sb.Append('=');
                sb.Append(field.GetValue(null));
            }

            Debug.Print(sb.ToString());
        }

        private bool IsApproved(ref SecurityRequestContainer request)
        {
            return request.HasData && _approvedTickers.Contains(request.Ticker);
        }
    }
}

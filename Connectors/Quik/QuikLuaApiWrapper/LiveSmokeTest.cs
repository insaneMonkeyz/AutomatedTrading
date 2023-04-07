using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TradingConcepts;
using TradingConcepts.SecuritySpecifics;
using Quik.Entities;
using Quik.EntityProviders;
using Quik.EntityProviders.RequestContainers;
using Quik.Lua;

namespace Quik
{
    internal class LiveSmokeTest
    {
        private class OrderbookProbes
        {
            public bool NewBookReceived;
            public bool OrderBookUpdated;
            public bool OrderBookCreated;

            public void Initialize()
            {
                OrderbooksProvider.Instance.NewEntity = (book) =>
                {
                    NewBookReceived = true;
                };
                OrderbooksProvider.Instance.EntityChanged = (book) =>
                {
                    OrderBookUpdated = true;
                };
            }
        }
        private class SecurityProbes
        {
            public bool GetClasses;
            public bool GetSecuritiesOfType;
            public bool GetSecurityOfKnownClass;
            public bool GetSecurityOfUnknownClass;
            public bool SecurityUpdatesReceived;
            public bool NewSecurityReceived;

            public void Initialize()
            {
                SecuritiesProvider.Instance.NewEntity = (sec) =>
                {
                    NewSecurityReceived = true;
                };
                SecuritiesProvider.Instance.EntityChanged = (sec) =>
                {
                    SecurityUpdatesReceived = true;
                };
            }
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
        private readonly OrderbookProbes _bookProbes = new();
        private readonly SecurityProbes _secProbes = new();
        private readonly ExecutionLoop _notificationsLoop;

        public static LiveSmokeTest Instance { get; } = new();

        private LiveSmokeTest() 
        {
            _notificationsLoop = new ExecutionLoop();
        }

        public void Initialize()
        {
            SecuritiesProvider.Instance.SubscribeCallback();
            OrderbooksProvider.Instance.SubscribeCallback();

            _bookProbes.Initialize();
            _secProbes.Initialize();

            OrderbooksProvider.Instance.CreationIsApproved = (ref OrderbookRequestContainer request) =>
            {
                return IsApproved(ref request.SecurityRequest);
            };
            SecuritiesProvider.Instance.CreationIsApproved = (ref SecurityRequestContainer request) =>
            {
                return IsApproved(ref request);
            };

            //AccountsProvider.Instance.SubscribeCallback();
            //DerivativesBalanceProvider.Instance.SubscribeCallback();
            //OrdersProvider.Instance.SubscribeCallback();
            //ExecutionsProvider.Instance.SubscribeCallback();
        }
        public void Begin()
        {
            OrderbooksProvider.Instance.Initialize(_notificationsLoop);            
            SecuritiesProvider.Instance.Initialize(_notificationsLoop);

            _secProbes.GetClasses = SecuritiesProvider.Instance.GetAvailableClasses().Any();
            _secProbes.GetSecuritiesOfType = SecuritiesProvider.Instance.GetAvailableSecuritiesOfType(typeof(IFutures)).Any();

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

            _secProbes.GetSecurityOfUnknownClass = securityResolver.Resolve(ref brmRequest) is Security brm;
            _secProbes.GetSecurityOfKnownClass = securityResolver.Resolve(ref brnRequest) is Security brn;

            var brjBookRequest = new OrderbookRequestContainer
            {
                SecurityRequest = new()
                {
                    Ticker = "BRJ3"
                }
            };

            _bookProbes.OrderBookCreated = OrderbooksProvider.Instance.Create(ref brjBookRequest) is not null;

            _notificationsLoop.Enter();

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
            PrintResults(_secProbes);
            PrintResults(_bookProbes);
        }

        private static void PrintResults(object probesContainer)
        {
            var sb = new StringBuilder();

            foreach (FieldInfo field in probesContainer.GetType().GetFields().OfType<FieldInfo>())
            {
                static string getString(object host, FieldInfo f)
                {
                    return f.GetValue(host)?.ToString() ?? string.Empty;
                }

                sb.Append(field.Name);
                sb.Append('=');
                sb.AppendLine(getString(probesContainer, field));
            }

            Debug.Print(sb.ToString());
        }

        private bool IsApproved(ref SecurityRequestContainer request)
        {
            return request.HasData && _approvedTickers.Contains(request.Ticker);
        }
    }
}

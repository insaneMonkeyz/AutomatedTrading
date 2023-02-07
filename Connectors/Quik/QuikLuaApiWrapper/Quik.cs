using System.Diagnostics;
using System.Text;
using Quik.EntityProviders;
using Quik.EntityProviders.RequestContainers;
using Quik.Lua;

namespace Quik
{
    public delegate void CallbackSubscriber(LuaFunction handler, string quikTableName);

    public class Quik
    {
        internal static readonly object SyncRoot = new();
#if DEBUG
        internal static LuaWrap Lua { get; private set; }
#else
        internal static LuaWrap Lua;
#endif

        /// <summary>
        /// Entry Point. This method gets called from the lua wrapper of the Quik trading terminal
        /// </summary>
        /// <param name="L">Pointer to the Lua state object</param>
        /// <returns></returns>
        public int Initialize(IntPtr luaState)
        {
            Lua = new(luaState, "Initializing thread");

            try
            {
                Lua.TieProxyLibrary("NativeToManagedProxy");
                Lua.RegisterCallback(Main, "main");

                //AccountsProvider.Instance.SubscribeCallback(lua);
                //DerivativesBalanceProvider.Instance.SubscribeCallback();
                SecuritiesProvider.SubscribeCallback();
                OrdersProvider.Instance.SubscribeCallback();
                ExecutionsProvider.Instance.SubscribeCallback();
                OrderbooksProvider.Instance.SubscribeCallback();
            }
            catch (Exception ex)
            {
                ex.Message.DebugPrintWarning();
                return -1;
            }

            return 0;
        }

        private static int Main(IntPtr state)
        {
            Lua = new(state, "Main thread");

            try
            {
                SecuritiesProvider.CreationIsApproved = (ref SecurityRequestContainer request) =>
                {
                    return false;
                };
                SecuritiesProvider.Initialize();
                OrdersProvider.Instance.Initialize();
                ExecutionsProvider.Instance.Initialize();
                OrderbooksProvider.Instance.Initialize();

                Api.lua_pushstring(Lua, "one");
                Api.lua_pushstring(Lua, "two");
                Api.lua_pushstring(Lua, "three");

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

                var orders = OrdersProvider.Instance.GetAllEntities();
                var ordersResolver = EntityResolvers.GetOrdersResolver();

                foreach (var order in orders)
                {
                    Debug.Print($"-- {DateTime.Now:O} LOADED {order}");
                }

                OrdersProvider.Instance.NewEntity = (order) =>
                {
                    Debug.Print($"-- {DateTime.Now:O} NEW ORDER {order}");
                };
                OrdersProvider.Instance.EntityChanged = (order) =>
                {
                    Debug.Print($"-- {DateTime.Now:O} ORDER CHANGED {order}");
                };

                var execs = ExecutionsProvider.Instance.GetAllEntities();

                foreach (var exec in execs)
                {
                    Debug.Print($"-- {DateTime.Now:O} LOADED {exec}");
                }
                ExecutionsProvider.Instance.NewEntity = (exec) =>
                {
                    Debug.Print($"-- {DateTime.Now:O} NEW ENTITY {exec}");
                };

                Debugger.Launch();
                var sec = orders.Last().Security;
                var bookRequest = new OrderbookRequestContainer
                {
                    SecurityRequest = new()
                    {
                        Ticker = sec.Ticker,
                        ClassCode = sec.ClassCode
                    }
                };
                var book = OrderbooksProvider.Instance.Create(ref bookRequest);

                OrderbooksProvider.Instance.EntityChanged = (b) =>
                {
                    b.UseQuotes((bids, asks, depth) =>
                    {
                        var maxdepth = Math.Min(depth, 5);

                        if (maxdepth <= 0)
                        {
                            "Zero size orderbook received".DebugPrintWarning();
                        }

                        var sb = new StringBuilder();

                        sb.AppendLine("-------------------------");

                        for (int i = 0; i < maxdepth; i++)
                        {
                            sb.AppendLine(asks[i].ToString());
                        }

                        for (int i = 0; i < maxdepth; i++)
                        {
                            sb.AppendLine(bids[i].ToString());
                        }

                        sb.AppendLine("-------------------------");

                        Debug.Print(sb.ToString());
                    });
                };

                //==========================================================================
                //
                //  WARNING! It turns out that quik rounds long numbers of type 'number'
                //           when reading them as Int64.
                //           The only way to get precise values is to read them as strings.
                //
                //  TODO: Check all API calls that return type 'number' and make sure
                //        they are being treated as strings!
                //
                //==========================================================================

                while (true)
                {
                    Thread.Sleep(50);
                }
            }
            catch (Exception ex)
            {
                ex.Message.DebugPrintWarning();
                return -1;
            }
            return 1;
        }
    }
}
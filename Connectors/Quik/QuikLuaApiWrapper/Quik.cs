using System.Diagnostics;

using Quik.EntityProviders;
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

                //AccountsProvider.Instance.Initialize();
                //AccountsProvider.Instance.SubscribeCallback(lua);
                Debugger.Launch();
                SecuritiesProvider.Initialize();
                //DerivativesBalanceProvider.Instance.Initialize();
                //DerivativesBalanceProvider.Instance.SubscribeCallback(lua);
                OrdersProvider.Instance.Initialize();
                OrdersProvider.Instance.SubscribeCallback();
                ExecutionsProvider.Instance.Initialize();
                ExecutionsProvider.Instance.SubscribeCallback();
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
                SecuritiesProvider.Initialize();

                Api.lua_pushstring(Lua, "one");
                Api.lua_pushstring(Lua, "two");
                Api.lua_pushstring(Lua, "three");

                System.Diagnostics.Debugger.Launch();

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
                    Thread.Sleep(10);
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
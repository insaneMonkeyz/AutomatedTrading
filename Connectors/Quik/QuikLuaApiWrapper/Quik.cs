﻿using System.Diagnostics;
using Tools;

using Quik.Entities;
using Quik.EntityDataProviders;
using Quik.EntityProviders;
using Quik.EntityProviders.Attributes;
using Quik.EntityProviders.RequestContainers;
using Quik.Lua;
using Tools.Logging;
using TradingConcepts;
using TradingConcepts.CommonImplementations;
using TradingConcepts.SecuritySpecifics;

namespace Quik
{
    internal delegate void CallbackSubscriber(LuaFunction handler, string quikTableName);

    internal partial class Quik : IDisposable
    {
        internal static readonly object SyncRoot = new();

#if DEBUG
        internal static LuaWrap Lua { get; private set; }
#else
        internal static LuaWrap Lua;
#endif
        private bool _disposed;

        private readonly Dictionary<string, LuaFunction> _usedCallbacks;
        private readonly IQuikDataConsumer[] _components;
        private readonly ExecutionLoop _executionLoop = new();
        private readonly TextWriter _logWriter = new StreamWriter("Quik.log", append: true);
        private readonly Log _log;

        private const string DLL_NAME = "NativeToManagedProxy";

        public int Initialize(IntPtr luaState)
        {
            try
            {
#if TRACE
                this.Trace();
#endif
                Lua = new(luaState, "Initializing thread");

                Lua.TieProxyLibrary(DLL_NAME);

                _usedCallbacks.ForEach(c => Lua.RegisterCallback(c.Value, c.Key));
                   _components.ForEach(c => c.SubscribeCallback());
            }
            catch (Exception ex)
            {
                _log.Error(nameof(Initialize), ex);
                return -1;
            }

            return 0;
        }

        private int Main(IntPtr state)
        {
#if TRACE
            this.Trace();
#endif
            Lua = new(state, "Main thread");

            for (int i = 0; i < 5; i++)
            {
                Api.lua_pushstring(Quik.Lua, "AUTOMATED TRADING");
            }

            try
            {
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

                // WHEN STATE OF CACHED ENTITY CHANGES
                // THE HASH USED TO RETRIEVE THIS ENTITY WILL REMAIN THE SAME
                // MATHCING STAGE WILL FAIL

                // check if transaction reply comes together with new order callback

                DerivativesBalanceProvider.Instance.EntityChanged = (balance) => SecurityBalanceChanged(balance);
                OrderbooksProvider.Instance.EntityChanged = (book) => OrderBookChanged(book);
                TransactionsProvider.Instance.OrderChanged = (order) => OrderChanged(order);
                SecuritiesProvider.Instance.EntityChanged = (sec) => SecurityChanged(sec);
                AccountsProvider.Instance.EntityChanged = (acc) => AccountChanged(acc);
                OrdersProvider.Instance.EntityChanged = (order) => OrderChanged(order);

                DerivativesBalanceProvider.Instance.NewEntity = (balance) => NewSecurityBalance(balance);
                ExecutionsProvider.Instance.NewEntity = (exec) => NewOrderExecution(exec);
                OrderbooksProvider.Instance.NewEntity = (book) => NewOrderBook(book);
                SecuritiesProvider.Instance.NewEntity = (sec) => NewSecurity(sec);
                AccountsProvider.Instance.NewEntity = (acc) => NewAccount(acc);
                OrdersProvider.Instance.NewEntity = (order) => NewOrder(order);


                _components.ForEach(c => c.Initialize(_executionLoop));
                _executionLoop.Enter();
            }
            catch (Exception ex)
            {
                _log.Error(nameof(Main), ex);
                return -1;
            }
            return 1;
        }
        private int OnClose(IntPtr state)
        {
            return OnStop(state);
        }
        private int OnStop(IntPtr state)
        {
            try
            {
#if TRACE
                this.Trace();
#endif
                Dispose();
            }
            catch (Exception e)
            {
                _log.Error(nameof(OnStop), e);
            }

            return 1;
        }
        private int OnCleanUp(IntPtr state)
        {
            try
            {
#if TRACE
                this.Trace();
#endif
                // TODO : Clean objects cache
            }
            catch (Exception e)
            {
                _log.Error(nameof(OnCleanUp), e);
            }

            return 1;
        }
        private int OnConnedted(IntPtr state)
        {
            try
            {
#if TRACE
                this.Trace();
#endif
                if (Lua.ReadAsBool())
                {
                    Connected(DateTime.UtcNow);
                }
            }
            catch (Exception e)
            {
                _log.Error(nameof(OnConnedted), e);
            }

            return 1;
        }
        private int OnDisonnedted(IntPtr state)
        {
            try
            {
#if TRACE
                this.Trace();
#endif
                ConnectionLost(DateTime.UtcNow);
            }
            catch (Exception e)
            {
                _log.Error(nameof(OnDisonnedted), e);
            }
            return 1;
        }

        #region Singleton
        internal static Quik Instance { get; } = new();
        private Quik()
        {
            _usedCallbacks = new()
            {
                { "main", Main },
                { "OnStop", OnStop },
                { "OnClose", OnClose },
                { "OnCleanUp", OnCleanUp },
                { "OnConnedted", OnConnedted },
                { "OnDisonnedted", OnDisonnedted },
            };

            _components = SingletonInstanceAttribute.GetInstances<IQuikDataConsumer>();
            _log = LogManagement.GetLogger<Quik>(); 
        }
        #endregion

        #region IDisposable
        protected void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _executionLoop.Abort();
                _components.ForEach(c => c.Dispose());
            }

            _usedCallbacks.ForEach(c => Lua.UnregisterCallback(c.Key));

            Lua.UnregisterCallback(DLL_NAME);
            LogManagement.Dispose();
            _logWriter.Dispose();
            _disposed = true;
        }
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        ~Quik()
        {
            Dispose(disposing: false);
        } 
        #endregion
    }
}
using System.Diagnostics;
using System.Text;
using TradingConcepts;
using Quik.Entities;
using Quik.EntityProviders;
using Quik.EntityProviders.RequestContainers;
using Quik.Lua;
using TradingConcepts.CommonImplementations;
using Core.Tools;
using Quik.EntityDataProviders.QuikApiWrappers;
using Quik.EntityDataProviders;

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

        // https://stackoverflow.com/questions/9957544/callbackoncollecteddelegate-in-globalkeyboardhook-was-detected
        private static readonly LuaFunction _onStop = OnStop;
        private static readonly LuaFunction _main = Main;

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
                Lua.RegisterCallback(_main, "main");
                Lua.RegisterCallback(_onStop, "OnStop");

                SecuritiesProvider.SubscribeCallback();
                TransactionsProvider.Instance.SubscribeCallback();
                //LiveSmokeTest.Instance.Initialize();
            }
            catch (Exception ex)
            {
                ex.Message.DebugPrintWarning();
                return -1;
            }

            return 0;
        }

        private static int OnStop(IntPtr state)
        {
            try
            {
                lock (SyncRoot)
                {
#if TRACE
                    Extentions.Trace(nameof(Quik));
#endif
                    //LiveSmokeTest.Instance.Complete(); 
                }
            }
            catch (Exception e)
            {
                $"{e.Message}\n{e.StackTrace ?? "NO_STACKTRACE_PROVIDED"}".DebugPrintWarning();
                return -1;
            }

            return 1;
        }

        private static int Main(IntPtr state)
        {
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


                //LiveSmokeTest.Instance.Begin();
                var loop = new ExecutionLoop();
                SecuritiesProvider.Initialize(loop);
                TransactionsProvider.Instance.Initialize(loop);

                var req = new SecurityRequestContainer
                {
                    ClassCode = MoexSpecifics.FUTURES_CLASS_CODE,
                    Ticker = "SiM3"
                };
                var sec = SecuritiesProvider.Create(ref req) ?? throw new Exception();
                var submission = new MoexOrderSubmission(sec)
                {
                    ClientCode = "SPBFUT00UJA",
                    AccountCode = "U7A0016",
                    TransactionId = TransactionIdGenerator.CreateId(),
                    Quote = new Quote
                    {
                        Operation = Operations.Sell,
                        Price = 79001,
                        Size = 1
                    }
                };

                var order = new Order(submission)
                {
                    ExchangeAssignedIdString = "1892948291012802690"
                };

                order.AddIntermediateState(OrderStates.Registering);
                order.SetSingleState(OrderStates.Active);

                Debugger.Launch();

                // check if transaction reply comes together with new order callback

                TransactionsProvider.Instance.Cancel(order);
            }
            catch (Exception ex)
            {
                ex.DebugPrintException();
                return -1;
            }
            return 1;
        }
    }
}